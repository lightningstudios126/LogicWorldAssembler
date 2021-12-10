namespace LogicWorldAssembler {
    public abstract record Operand {
        public record RegisterOperand(Register register) : Operand;

        public record ImmediateValueOperand(byte value) : Operand;

        public abstract record AddressOperand(byte? address) : Operand {
            public record ImmediateAddressOperand(byte value) : AddressOperand(value);

            public record LabelOperand(Label label) : AddressOperand((byte?) label.Address);
        }
    }
}