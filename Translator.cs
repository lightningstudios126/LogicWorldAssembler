using System.Collections.Generic;

namespace LogicWorldAssembler {
    public class Translator {
        private readonly List<Instruction> instructions;

        public Translator(List<Instruction> instructions) {
            this.instructions = instructions;
        }

        public byte[] Translate() {
            List<byte> bytes = new();
            foreach (var instruction in instructions) bytes.AddRange(instruction.ToMachineCode());
            return bytes.ToArray();
        }
    }
}