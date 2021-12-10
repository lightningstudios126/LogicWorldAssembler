using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogicWorldAssembler // Note: actual namespace depends on the project name.
{
    public class Program {
        public static void Main(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine("Missing a file name");
                return;
            }

            try {
                using FileStream fileStream = File.OpenRead(args[0]);
                using StreamReader reader = new(fileStream);
                Parser parser = new(reader);
                List<Instruction> instructions = parser.Scan();
                foreach (var instruction in instructions) Console.WriteLine(instruction.Reconstruct());

                if (!parser.HasError) {
                    Translator translator = new(instructions);
                    byte[] bytes = translator.Translate();
                    //string byteString = string.Join('\n', bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
                    string byteString = string.Join("", bytes.Select(b => b.ToString("X2")));
                    Console.WriteLine(byteString);
                }
            } catch {
                Console.Error.WriteLine("Could not open file");
            }
        }
    }
}