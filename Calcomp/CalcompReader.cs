using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calcomp {
    /// <summary>
    /// Reads a Calcomp 907 plot file and returns a CalcompPlot object
    /// </summary>
    public class CalcompReader {
        // have only ever seen plots with these values but it's possible a future version might need to make these configurable by user
        
        // identifies the start of a plot record
        private const byte Sync = 0x02;
        
        // end of message (i.e. end of plot record)
        private const byte Eom = 0x03;

        // every byte in a plot record is adjusted by this value. a bias of 32 and a radix of 95 (specified in the
        // plot but this is a common value) gives every byte a value of between 32 & 127, i.e. printable ascii values
        private const byte Bias = 0x20; // 32

        private int _radix;
        private int _recCount;

        private bool _debugMode;

        private CalcompPlot _plot;
              
        /// <param name="debugMode">If true then a text version of all the instructions is included in the plot object</param>
        public CalcompReader(bool debugMode) {
            _debugMode = debugMode;
        }

        /// <summary>
        /// Reads the plot data and returns a CalcompPlot object
        /// </summary>
        public CalcompPlot ReadPlotData(string filename) {
            using (FileStream stream = File.Open(filename, FileMode.Open)) {
                return ReadPlotData(stream);
            }
        }

        /// <summary>
        /// Reads the plot data and returns a CalcompPlot object
        /// </summary>
        public CalcompPlot ReadPlotData(Stream stream) {
            _plot = new CalcompPlot(_debugMode);

            try {
                using (BinaryReader reader = new BinaryReader(stream)) {
                    _recCount = 0;

                    List<byte> currentRecord = null;

                    bool recordComplete = false;
                    bool inRecord = false;

                    try {
                        while (true) {
                            byte curByte = reader.ReadByte();

                            if (!inRecord) {
                                // if not in a record, continue reading until we find the sync byte
                                if (curByte == Sync) {
                                    // read next byte & check bias
                                    byte recBias = reader.ReadByte();

                                    if (recBias != Bias) {
                                        throw new IOException("Bias doesn't match");
                                    }

                                    inRecord = true;

                                    currentRecord = new List<byte>();
                                }
                            } else {
                                // we're currently in a record so continue adding to the record until we find the eom byte
                                if (curByte == Eom) {
                                    recordComplete = true;
                                    inRecord = false;

                                    ProcessRecord(currentRecord);
                                } else {
                                    if (curByte >= Bias) {
                                        byte unbiased = (byte)(curByte - Bias);
                                        currentRecord.Add(unbiased);
                                    } else {
                                        throw new IOException("byte value smaller than bias");
                                    }

                                }
                            }
                        }
                    } catch (EndOfStreamException e) {
                        if (!recordComplete) {
                            _plot.LogError("File ended in middle of record: " + e.Message);
                        }
                    } catch (IOException e) {
                        _plot.LogError(e.Message);
                    }
                }
            } catch (IOException e) {
                _plot.LogError(e.Message);
            }

            return _plot;
        }

        /// <summary>
        /// Process a single record from the plot file and add its instructions to the CalcompPlot object
        /// </summary>
        /// <param name="record"></param>
        private void ProcessRecord(List<byte> record) {
            _recCount++;

            if (_recCount == 1) {
                _plot.AddHeaderInformation("Header record:");
            } else {
                _plot.AddHeaderInformation(string.Format("Record {0}:", _recCount));
            }
            
            for (int i = 0; i < record.Count; i++) {
                byte b = record[i];

                bool errorFound = false;

                switch (b) {
                    case 0x00:
                        // no op
                        _plot.AddHeaderInformation("No-op");
                        break;
                    case 0x01:
                        // search address
                        int part1 = record[++i];
                        int part2 = record[++i];
                        int part3 = record[++i];
                        _plot.AddHeaderInformation("Search address: " + Get3ByteNumber(part1, part2, part3));
                        break;
                    case 0x02:
                        _plot.AddInstruction(new PlotInstruction { InstType = InstructionType.PenDown });
                        break;
                    case 0x03:
                        _plot.AddInstruction(new PlotInstruction { InstType = InstructionType.PenUp });
                        break;
                    case 0x04:
                        byte pen = record[++i];

                        _plot.AddInstruction(new PlotInstruction { InstType = InstructionType.PenChange, NewPen = pen });
                        break;
                    case 0x05:
                        // not implemented
                        _plot.LogError(string.Format("Unhandled value: 0x05 (record: {0})", _recCount));
                        errorFound = true;
                        break;
                    case 0x06:
                        // not implemented
                        _plot.LogError(string.Format("Unhandled value: 0x06 (record: {0})", _recCount));
                        errorFound = true;
                        break;
                    case 0x07:
                        // radix is not stored as-is, but as radix - 1 so adjust
                        _radix = record[++i] + 1;
                        _plot.AddHeaderInformation("Radix: " + _radix);
                        break;
                    case 0x08:
                        // additional header information
                        byte subcode = record[++i];

                        if (subcode == 0x0A) {
                            _plot.AddHeaderInformation("Buffer size: 128");
                        }

                        if (subcode == 0x02) {
                            byte data = record[++i];
                            _plot.AddHeaderInformation("Response suffix: " + data);
                        }

                        if (subcode == 0x03) {
                            byte data = record[++i];
                            _plot.AddHeaderInformation("Turnaround delay: " + data);
                        }

                        if (subcode == 0x04 || subcode == 0x05 || subcode == 0x06) {
                            _plot.AddHeaderInformation("Good/bad/request response (but ignoring data)");

                            // this is the length of the data / 2
                            byte length = record[++i];

                            // advance the index over the data
                            i = i + (length * 2);
                        }

                        break;
                    case 0x09:
                        int scale = record[++i];
                        _plot.AddHeaderInformation("Scaling: " + scale);
                        break;
                    case 0x0e:
                        byte nextcode = record[++i];

                        if (nextcode == 0x3f) {
                            // unable to determine what this record does, but it's possible to identify the data related to this command and ignore it
                            _plot.AddHeaderInformation("Unknown command (0e 3f), data identified and ignored");

                            // the next byte stores the length of the data related to this command / 2
                            byte length = record[++i];

                            // advance the index over the data
                            i = i + (length * 2);
                        } else {
                            // if not 0x3f then we don't know how to handle 
                            _plot.AddHeaderInformation("Unhandled value: 0e " + nextcode);
                            _plot.LogError(string.Format("Unhandled value: 0e {0} (record: {1})", nextcode, _recCount));
                            errorFound = true;
                        }

                        break;
                    default:
                        if (b >= 0x10 && b <= 0x3f) {
                            Delta delta = new Delta(b, _radix);

                            for (int j = 0; j < delta.ExpectedByteCount; j++) {
                                delta.AddByte(record[++i]);
                            }

                            _plot.AddInstruction(new PlotInstruction { InstType = InstructionType.Delta, X = delta.Dx, Y = delta.Dy });
                        } else {
                            _plot.LogError(string.Format("Unhandled value: {0} (record: {1})", b, _recCount));
                            errorFound = true;
                        }

                        
                        break;
                }

                // if an error is found we cannot reliably read the rest of the record, so we add the remaining bytes of the record to the instruction text for debugging purposes
                if (errorFound) {
                    string remainingRecord = "";

                    for (int j = i; j < record.Count; j++) {
                        remainingRecord += record[j] + " ";
                    }

                    _plot.AddHeaderInformation(remainingRecord);
                    
                    break;
                }
            }
        }

        /// <summary>
        /// Converts a number represented as 3 bytes in the plot file to an integer.
        /// </summary>
        private int Get3ByteNumber(int part1, int part2, int part3) {
            return (_radix * _radix * part1) + (_radix * part2) + part3;
        }

        
    }
}
