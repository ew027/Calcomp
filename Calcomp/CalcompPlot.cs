using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calcomp {
    /// <summary>
    /// This class can interpret a CalComp 907 plot file and export the plot file to an image.
    /// </summary>
    public class CalcompPlot {
        // stores the maximum extent of the plot to allow an image scale factor to be calculated
        private int _maxX;
        private int _maxY;

        // keeps track of the current x,y co-ordinates as instructions are added.
        private int _currentX;
        private int _currentY;

        // the pens associated with the plot
        private Dictionary<int, Pen> _pens;

        private List<PlotInstruction> _instructions;

        // if debug mode is enabled, a list of the plot instructions read from the file will be stored in _instructionList
        private bool _debugMode;

        private List<String> _instructionList;
        private List<String> _errorList;

        public CalcompPlot(bool debugMode = false) {
            _debugMode = debugMode;

            _instructions = new List<PlotInstruction>();
            _instructionList = new List<string>();
            _errorList = new List<string>();
            _pens = new Dictionary<int, Pen>();
        }

        /// <summary>
        /// Returns the list of instructions for the plot
        /// </summary>
        public List<PlotInstruction> Instructions {
            get {
                return _instructions;
            }
        }

        /// <summary>
        /// Returns the text version of the instructions for the plot
        /// </summary>
        public List<string> InstructionList {
            get {
                return _instructionList;
            }
        }

        /// <summary>
        /// Returns the list of errors found when reading the plot
        /// </summary>
        public List<string> ErrorList {
            get {
                return _errorList;
            }
        }

        /// <summary>
        /// Returns the dictionary of pens associated with the plot
        /// </summary>
        public Dictionary<int, Pen> Pens {
            get {
                return _pens;
            }
        }

        /// <summary>
        /// Returns the maximum X value for the plot
        /// </summary>
        public int MaxX {
            get { 
                return _maxX; 
            }
        }

        /// <summary>
        /// Returns the mamimum Y value for the plot 
        /// </summary>
        public int MaxY {
            get {
                return _maxY;
            }
        }

        /// <summary>
        /// Add an instruction to the plot
        /// </summary>
        public void AddInstruction(PlotInstruction instruction) {
            if (instruction == null) {
                throw new ArgumentNullException("The instruction must be a valid PlotInstruction object");
            }

            _instructions.Add(instruction);
            
            // update the current co-ordinates and max values if necessary
            if (instruction.InstType == InstructionType.Delta) {
                _currentX += instruction.X;
                _currentY += instruction.Y;

                if (_currentX > _maxX) {
                    _maxX = _currentX;
                }

                if (_currentY > _maxY) {
                    _maxY = _currentY;
                }
            }

            if (_debugMode) {
                _instructionList.Add(instruction.ToString());
            }
        }

        /// <summary>
        /// Log an error that occurred when reading the plot
        /// </summary>
        /// <param name="error"></param>
        public void LogError(string error) {
            _errorList.Add(error);
        }

        /// <summary>
        /// Allows header information to be logged in the list of instructions for debugging purposes
        /// </summary>
        /// <param name="header"></param>
        public void AddHeaderInformation(string header) {
            _instructionList.Add(header);
        }

        /// <summary>
        /// Add a new Pen to be used when drawing the plot
        /// </summary>
        public void AddPen(int penNumber, Pen pen) {
            if (!_pens.ContainsKey(penNumber)) {
                _pens.Add(penNumber, pen);
            }
        }
    }
}
