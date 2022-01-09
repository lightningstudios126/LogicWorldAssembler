namespace LogicWorldAssembler {
    public class Label {
        public Label(string labelName) {
            LabelName = labelName;
        }

        public string LabelName { get; }
        public Instruction? Instruction { get; set; }

        public override string ToString() {
            return $"{LabelName} ({Instruction?.Address.ToString("X2") ?? "?"})";
        }
    }
}