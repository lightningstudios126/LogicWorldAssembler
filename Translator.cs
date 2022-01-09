namespace LogicWorldAssembler {
    public class Translator {
        private readonly List<Instruction> instructions;

        public Translator(List<Instruction> instructions) {
            this.instructions = instructions;
        }

        public byte[] Translate() {
            List<byte> bytes = new();

            foreach (Instruction instruction in instructions) {
                TranslateInstruction(instruction, bytes);
            }

            return bytes.ToArray();
        }

        private int TranslateInstruction(Instruction instr, List<byte> bytes) {
            byte instByte = 0;
            var trailing = new List<byte>();

            // add opcode
            instByte |= instr.Mnemonic.Opcode();

            // combine with register and add immediate value
            switch (instr.Mnemonic.Operands()) {
                case (null, null): break;

                case (OperandType.REGISTER, null):
                    instByte |= (byte) (instr.Operands[0] as Operand.RegisterOperand)!.Register;
                    break;
                case (OperandType.IMM_VALUE, null):
                    trailing.Add((instr.Operands[0] as Operand.ImmediateValueOperand)!.Value);
                    break;
                case (OperandType.ADDRESS, null): {
                    trailing.Add((byte) (instr.Operands[0] as Operand.AddressOperand)!.Address!);
                    break;
                }

                case (OperandType.REGISTER, OperandType.REGISTER):
                    instByte |= (byte) ((byte) (instr.Operands[0] as Operand.RegisterOperand)!.Register << 3);
                    instByte |= (byte) (instr.Operands[1] as Operand.RegisterOperand)!.Register;
                    break;
                case (OperandType.REGISTER, OperandType.IMM_VALUE):
                    instByte |= (byte) (instr.Operands[0] as Operand.RegisterOperand)!.Register;
                    trailing.Add((instr.Operands[1] as Operand.ImmediateValueOperand)!.Value);
                    break;
                case (OperandType.REGISTER, OperandType.ADDRESS):
                    instByte |= (byte) (instr.Operands[0] as Operand.RegisterOperand)!.Register;
                    trailing.Add((byte) (instr.Operands[1] as Operand.AddressOperand)!.Address!);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(null, "Unsupported operand pattern");
            }

            bytes.Add(instByte);
            bytes.AddRange(trailing);
            return 1 + trailing.Count;
        }
    }
}