namespace LogicWorldAssembler {
    public struct Instruction {
        public Mnemonic Mnemonic { get; init; }
        public Operand[] Operands { get; init; }
        public int Address { get; set; }
        public int LineNumber { get; init; }

        public override string ToString() {
            return
                $"{nameof(Mnemonic)}: {Mnemonic}, {nameof(Operands)}: [{string.Join(", ", Operands.Select(o => o.ToString()))}], {nameof(Address)}: {Address}, {nameof(LineNumber)}: {LineNumber}";
        }

        public string Reconstruct() {
            System.Text.StringBuilder builder = new();
            builder.Append($"{Address:X2}: ");
            builder.Append(Mnemonic.ToString());

            foreach (Operand operand in Operands) {
                builder.Append(' ');
                builder.Append(operand);
            }

            return builder.ToString();
        }
    }
}