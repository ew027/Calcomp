using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calcomp {
    /// <summary>
    /// Represents a delta in a Calcomp 907 plot file. To use, create an instance with the command byte and the current radix value.
    /// Then call the ExpectedByteCount property and add the relevant number of bytes from the current plot record to the delta object using the AddByte method.
    /// Once the required number of bytes have been added, the X and Y delta properties can be accessed.
    /// </summary>
    public class Delta {
        // number of bytes required to represent the x & y values
        private int _xByteCount;
        private int _yByteCount;

        private bool _isXNegative;
        private bool _isYNegative;

        // current radix value
        private int _radix;

        // delta command byte
        private byte _deltaInstruction;

        // x & y delta values
        private int _dx;
        private int _dy;

        // the relevant bytes from the plot file
        private List<byte> _values;

        // track whether delta X & Y have been calculated yet
        private bool _deltaCalculated;

        /// <summary>
        /// Initialises a new instance of the Delta class using the delta command byte.
        /// </summary>
        /// <param name="instruction">Delta command byte from the plot file (0x10 - 0x3f)</param>
        /// <param name="radix">The radix value used in the current plot file</param>
        public Delta(byte instruction, int radix) {
            if (instruction < 0x10 || instruction > 0x3f) {
                throw new ArgumentException("Invalid delta command byte");
            }

            _values = new List<byte>();

            _deltaInstruction = instruction;
            _radix = radix;

            _isXNegative = false;
            _isYNegative = false;

            _dx = 0;
            _dy = 0;

            ProcessInstructionByte();

            _deltaCalculated = false;
        }

        #region Public methods

        /// <summary>
        /// Add a byte to the current delta
        /// </summary>
        public void AddByte(byte b) {
            if (_values.Count >= ExpectedByteCount) {
                throw new InvalidOperationException("Expected byte count exceeded");
            }
            _values.Add(b);
        }
        
        public override string ToString() {
            return string.Format("Delta: dx = {0}, dy = {1}", _dx, _dy);
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Get the number of bytes that comprise the delta (read only)
        /// </summary>
        public int ExpectedByteCount {
            get {
                return _xByteCount + _yByteCount;
            }
        }

        /// <summary>
        /// Get the current number of bytes added to the delta (read only)
        /// </summary>
        public int CurrentByteCount {
            get {
                return _values.Count;
            }
        }

        /// <summary>
        /// Get the x value of the delta (read only)
        /// </summary>
        public int Dx {
            get {
                if (!_deltaCalculated) {
                    Calc();
                }

                return _dx; 
            }
        }

        /// <summary>
        /// Get the y value of the delta (read only)
        /// </summary>
        public int Dy {
            get {
                if (!_deltaCalculated) {
                    Calc();
                }

                return _dy;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Calculate the delta values (assumes byte sequence is complete)
        /// </summary>
        private void Calc() {
            // check that calculated x + y count == byte values count first
            if (ExpectedByteCount != CurrentByteCount) {
                throw new InvalidOperationException("Cannot calculate X & Y delta if byte counts don't match byte sequence");
            }

            if (_xByteCount > 0) {
                CalcDx();
            }

            if (_yByteCount > 0) {
                CalcDy();
            }

            _deltaCalculated = true;
        }

        /// <summary>
        /// Calculate the x value of the delta
        /// </summary>
        private void CalcDx() {
            if (_xByteCount == 3) {
                _dx = _radix * _radix * _values[0];
            }

            if (_xByteCount >= 2) {
                // if count = 3 then idx 1, if 2 then idx 0, hence count - 2
                _dx += _radix * _values[_xByteCount - 2];
            }

            // if count = 3 then idx 2, if 2 then idx 1, if 1 then idx 0, hence count - 1
            _dx += _values[_xByteCount - 1];

            if (_isXNegative) {
                _dx = _dx * -1;
            }
        }

        /// <summary>
        /// Calculate the y value of the delta
        /// </summary>
        private void CalcDy() {
            // almost same for y value but need to account for x values as well as they appear first in the byte sequence
            if (_yByteCount == 3) {
                _dy = _radix * _radix * _values[_xByteCount];
            }

            if (_yByteCount >= 2) {
                // if count = 3 then idx 1, if 2 then idx 0, hence total count - 2
                _dy += _radix * _values[_xByteCount + _yByteCount - 2];
            }

            // total count - 1
            _dy += _values[_xByteCount + _yByteCount - 1];

            if (_isYNegative) {
                _dy = _dy * -1;
            }
        }

        /// <summary>
        /// The instruction byte starts at 16 (0x10), and runs up to 63 (0x3f), leaving 48 values to cover
        /// every possible combination of x & y byte sequences. Each delta component can occupy up to 3 bytes
        /// but will use less if possible - a zero component won't use any bytes (delta moves of 0,0 aren't
        /// written to plots and aren't included in one of the 48 combinations).
        /// 
        /// To determine the number of x & y bytes and the signs, we subtract 16 from the byte and then split into
        /// a block code (byte / 4) and a remain code (byte % 4). 4 is chosen as each combination of byte totals
        /// has 4 possible sign combinations (deltas with a 0-length component are treated slightly differently 
        /// but still fit neatly into blocks of 4).
        /// 
        /// Some of the length combinations can be easily derived from the block code (codes 0 - 5), others are
        /// handled manually.
        /// </summary>
        private void ProcessInstructionByte() {
            // subtract 16 and then int divide by 4 to get block code and mod by 4 to get remain code
            int subtracted = _deltaInstruction - 16;
            int blockCode = subtracted / 4;
            int remainCode = subtracted % 4;

            if (blockCode < 3) {
                // x & y same length, length is (3 - block code), calc signs from remain code
                _xByteCount = 3 - blockCode;
                _yByteCount = 3 - blockCode;

                CalcSigns(remainCode);
            } else if (blockCode >= 3 && blockCode <= 5) {
                // x or y = 0, inspect remain code to determine which (0,3=x 1,2=y), length of other = (6 - blockcode)
                // sign of non-zero component does't follow normal rules but can still be derived from remain code: 1 is neg x, 3 is neg y
                if (remainCode == 0 || remainCode == 3) {
                    _xByteCount = 0;
                    _yByteCount = 6 - blockCode;

                    _isYNegative = (remainCode == 3);
                } else {
                    _yByteCount = 0;
                    _xByteCount = 6 - blockCode;

                    _isXNegative = (remainCode == 1);
                }
            } else {
                // currently cannot find a reasonable way to do this easily so just process blocks 6 - 11 manually
                switch (blockCode) {
                    case 6:
                        _xByteCount = 2;
                        _yByteCount = 3;
                        break;
                    case 7:
                        _xByteCount = 1;
                        _yByteCount = 3;
                        break;
                    case 8:
                        _xByteCount = 3;
                        _yByteCount = 2;
                        break;
                    case 9:
                        _xByteCount = 3;
                        _yByteCount = 1;
                        break;
                    case 10:
                        _xByteCount = 1;
                        _yByteCount = 2;
                        break;
                    case 11:
                        _xByteCount = 2;
                        _yByteCount = 1;
                        break;
                }

                // at least signs can be done as normal
                CalcSigns(remainCode);
            }
        }

        /// <summary>
        /// Calculate the sign of each delta part based on the remain code extracted from the command byte
        /// </summary>
        private void CalcSigns(int remainCode) {
            switch (remainCode) {
                case 1:
                    _isXNegative = true;
                    break;
                case 2:
                    _isYNegative = true;
                    break;
                case 3:
                    _isXNegative = true;
                    _isYNegative = true;
                    break;
            }
        }

        #endregion
    }
}
