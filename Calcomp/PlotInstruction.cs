using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calcomp {
    /// <summary>
    /// Respresents the different instructions in a Calcomp 907 plot
    /// </summary>
    public class PlotInstruction {
        public InstructionType InstType { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int NewPen { get; set; }

        public override string ToString() {
            string instructionText = "";

            if (InstType == InstructionType.PenUp || InstType == InstructionType.PenDown) {
                instructionText = InstType.ToString();
            } else if (InstType == InstructionType.PenChange) {
                instructionText = "Pen change: " + NewPen;
            } else if (InstType == InstructionType.Delta) {
                instructionText = string.Format("Delta: DX {0}, DY {1}", X, Y);
            } else {
                instructionText = "Unknown instruction: " + InstType.ToString();
            }

            return instructionText;
        }
    }

    public enum InstructionType {
        PenUp,
        PenDown,
        PenChange,
        Delta
    }
}
