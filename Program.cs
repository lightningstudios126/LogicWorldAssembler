namespace LogicWorldAssembler {
    public static class Program {
        public static void Main(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine("Missing a file name");
                return;
            }

            try {
                using FileStream fileStream = File.OpenRead(args[0]);
                using StreamReader reader = new(fileStream);
                Parser parser = new(reader);
                var instructions = parser.Scan();

                if (parser.HasError) return;

                foreach (Instruction instruction in instructions)
                    Console.WriteLine(instruction.Reconstruct());
                Translator translator = new(instructions);
                byte[] bytes = translator.Translate();

                //string byteString = string.Join('\n', bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
                string byteString = string.Join("", bytes.Select(b => b.ToString("X2")));
                Console.WriteLine(byteString);
            } catch {
                Console.Error.WriteLine("Could not open file");
            }
        }
    }
}