namespace LogicWorldAssembler {
    public abstract record Operand {
        public sealed override string ToString() => this switch {
            RegisterOperand registerOperand => registerOperand.Register.ToString(),
            ImmediateValueOperand immediateValueOperand => "0x" + immediateValueOperand.Value.ToString("X2"),
            AddressOperand.ImmediateAddressOperand immediateAddressOperand =>
                immediateAddressOperand.Value.ToString("X2"),
            AddressOperand.LabelOperand labelOperand => labelOperand.Label.ToString(),
            _ => throw new ArgumentOutOfRangeException()
        };

        public record RegisterOperand(Register Register) : Operand;

        public record ImmediateValueOperand(byte Value) : Operand;

        public abstract record AddressOperand : Operand {
            public abstract int Address { get; }

            public record ImmediateAddressOperand(byte Value) : AddressOperand {
                public override int Address => Value;
            }

            public record LabelOperand(Label Label) : AddressOperand() {
                public override int Address => Label.Instruction!.Value.Address;
            }
        }
    }
}