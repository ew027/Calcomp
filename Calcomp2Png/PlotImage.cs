using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

using Calcomp;

namespace Calcomp2Png {
    /// <summary>
    /// Read a Calcomp plot file and export to a PNG image
    /// </summary>
    class PlotImage {
        private int _currentPen;

        // this allows a delta to be drawn even if the current pen is omitted from the defined list of pens
        private int _defaultPen;

        private int _currentX;
        private int _currentY;
        private float _scaleFactor;

        private PenState _currentPenState;

        private CalcompPlot _plot;

        private bool _includeInstructions;

        public PlotImage(string plotFilename, bool includeInstructions, float scaleFactor) {
            _scaleFactor = scaleFactor;
            _includeInstructions = includeInstructions;

            CalcompReader reader = new CalcompReader(includeInstructions);
            _plot = reader.ReadPlotData(plotFilename);

            if (_plot.ErrorList.Count > 0) {
                Console.WriteLine("{0} errors found when reading plot file:", _plot.ErrorList.Count);
                Console.WriteLine();

                foreach (var error in _plot.ErrorList) {
                    Console.WriteLine(error);
                }
            }
        }

        /// <summary>
        /// Save an image of the current plot
        /// </summary>
        /// <param name="imageFilename">The filename to be used for the output image</param>
        public void Save(string imageFilename) {
            string instructionsFilename = Path.GetFileNameWithoutExtension(imageFilename) + ".txt";
            
            _plot.AddPen(1, new Pen(Color.Black, 1));
            _plot.AddPen(2, new Pen(Color.Red, 1));
            _plot.AddPen(3, new Pen(Color.DarkBlue, 1));
            _plot.AddPen(4, new Pen(Color.SpringGreen, 1));
            _plot.AddPen(5, new Pen(Color.Purple, 1));
            _plot.AddPen(6, new Pen(Color.Pink, 1));
            _plot.AddPen(7, new Pen(Color.Yellow, 1));
            _plot.AddPen(8, new Pen(Color.DarkGreen, 1));

            _defaultPen = 1;

            _currentX = 0;

            // when drawing the plot on the image we start from the top left, but the plot starts from the bottom left
            // so set the initial y value = to the max y (the ProcessDelta method adjusts for the scalefactor)
            _currentY = _plot.MaxY;
            
            _currentPenState = PenState.PenUp;

            int height = Convert.ToInt32(_plot.MaxY * _scaleFactor);
            int width = Convert.ToInt32(_plot.MaxX * _scaleFactor);

            using (var bmp = new Bitmap(width, height)) {
                using (var g = Graphics.FromImage(bmp)) {
                    // set the background to white
                    g.Clear(Color.White);

                    foreach (var inst in _plot.Instructions) {
                        switch (inst.InstType) {
                            case InstructionType.Delta:
                                ProcessDelta(inst.X, (inst.Y * -1), g);
                                break;
                            case InstructionType.PenDown:
                                _currentPenState = PenState.PenDown;
                                break;
                            case InstructionType.PenUp:
                                _currentPenState = PenState.PenUp;
                                break;
                            case InstructionType.PenChange:
                                if (_plot.Pens.ContainsKey(inst.NewPen)) {
                                    _currentPen = inst.NewPen;
                                } else {
                                    _currentPen = _defaultPen;
                                    Console.WriteLine("Pen not defined: {0}, using default pen.", inst.NewPen);
                                }

                                // this doesn't seem to be strictly part of the spec but is perhaps implied in some way
                                _currentPenState = PenState.PenUp;
                                break;
                        }
                    }
                }

                bmp.Save(imageFilename);

                Console.WriteLine("Plot written to {0}", imageFilename);
            }

            // write out the instructions if required
            if (_includeInstructions) {
                using (StreamWriter writer = new StreamWriter(instructionsFilename)) {
                    foreach (var instruction in _plot.InstructionList) {
                        writer.WriteLine(instruction);
                    }
                }

                Console.WriteLine("Plot instructions written to {0}", instructionsFilename);
            }
        }

        private void ProcessDelta(int x, int y, Graphics g) {
            if (_currentPenState == PenState.PenDown) {
                PointF orig = new PointF(_currentX * _scaleFactor, _currentY * _scaleFactor);
                PointF dest = new PointF((_currentX + x) * _scaleFactor, (_currentY + y) * _scaleFactor);

                // we have already checked the current pen is defined
                g.DrawLine(_plot.Pens[_currentPen], orig, dest);
            }

            _currentX += x;
            _currentY += y;
        }
    }

    enum PenState {
        PenUp,
        PenDown
    }
}
