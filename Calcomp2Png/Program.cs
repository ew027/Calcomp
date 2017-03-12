using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Calcomp2Png {
    class Program {
        static void Main(string[] args) {
            // valid arguments are:
            // input output
            // input output scalefactor
            // input output -instructions
            // input output scalefactor -instructions
            
            if (args.Length == 0) {
                PrintUsageNotes();
                return;
            }

            // minimum 2 arguments required - input & output filenames
            if (args.Length < 2) {
                Console.WriteLine("Insufficient arguments supplied.");
                Console.WriteLine();
                PrintUsageNotes();
                return;
            }

            string inputFilename = args[0];
            string imageFilename = args[1];
            bool includeInstructions = false;
            float scaleFactor = 1F;

            if (args.Length == 3) {
                if (args[2] == "-instructions") {
                    includeInstructions = true;
                } else {
                    // if not "-instructions" then is scale factor
                    if (!float.TryParse(args[2], out scaleFactor)) {
                        Console.WriteLine("Invalid scale factor: {0}", args[2]);
                        Console.WriteLine();
                        PrintUsageNotes();
                        return;
                    }
                }
            }

            // if 4 arguments then last one must be -instructions
            if (args.Length == 4) {
                if (args[3] == "-instructions") {
                    includeInstructions = true;
                } else {
                    Console.WriteLine("Invalid argument supplied: {0}. Expected -instructions", args[3]);
                    Console.WriteLine();
                    PrintUsageNotes();
                }
            }

            try {
                PlotImage image = new PlotImage(inputFilename, includeInstructions, scaleFactor);
                image.Save(imageFilename);
            } catch (IOException ex) {
                Console.WriteLine("File error: {0}", ex.Message);
            }
        }

        private static void PrintUsageNotes() {
            Console.WriteLine("Usage: Calcomp2Png plotfile imagefile [scalefactor -instructions]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("scalefactor: the scale factor to be applied to the plot (defaults to 1).");
            Console.WriteLine("-instructions: save the instructions to a text file.");
            Console.WriteLine("");
            Console.WriteLine("Example: Calcomp2Png input.plt output.png 2 -instructions");
        }
    }
}
