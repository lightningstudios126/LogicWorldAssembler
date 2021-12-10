namespace LogicWorldAssembler {
    public class Label {
        public Label(string labelName, int? address) {
            LabelName = labelName;
            Address = address;
        }

        public string LabelName { get; }
        public int? Address { get; set; }
    }
}