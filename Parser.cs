using System.Text.RegularExpressions;

namespace LogicWorldAssembler {
    public class Parser {
        private static readonly Regex regex = new(
            @"^(?:(?<label>\w+):[ \t]*)?(?:(?<mnemonic>[a-zA-Z]+)(?:[ \t]+(?<op>\w+))*)?[ \t]*(?:;.*)?$$",
            RegexOptions.Compiled);

        private readonly List<Instruction> instructions = new();
        private readonly Dictionary<string, Label> labels = new();
        private readonly TextReader source;

        private int lineIndex;
        private int memAddress;

        public Parser(TextReader source) {
            this.source = source;
        }

        public bool HasError { get; private set; }

        private void Error(int lineNumber, string errorMessage) {
            if (lineNumber > 0) {
                Console.WriteLine($"Line {lineNumber}: " + errorMessage);
            } else {
                Console.WriteLine(errorMessage);
            }

            HasError = true;
        }

        private void Error(string errorMessage) {
            Error(lineIndex, errorMessage);
        }

        private void Warning(int lineNumber, string errorMessage) {
            Console.WriteLine($"Line {lineNumber}: " + errorMessage);
        }

        private Label AddOrReturnLabel(string label) {
            if (!labels.ContainsKey(label)) {
                Label l = new(label);
                labels.Add(label, l);
            }

            return labels[label];
        }

        public List<Instruction> Scan() {
            List<Label> toBind = new();

            while (true) {
                string? line = source.ReadLine();
                lineIndex++;
                if (line == null) break;
                if (string.IsNullOrWhiteSpace(line)) continue;

                line = line.Trim();

                if (line.StartsWith(';')) continue;

                Match match = regex.Match(line);

                if (!match.Success) {
                    Error(lineIndex, "Line is invalid");
                    continue;
                }

                Label? label = null;

                if (match.Groups["label"].Success) {
                    label = AddOrReturnLabel(match.Groups["label"].Value);
                    if (toBind.Count > 0) {
                        Warning(lineIndex, $"Redundant label \"{label.LabelName}\"");
                    }
                }

                Mnemonic mnemonic;

                if (match.Groups["mnemonic"].Success) {
                    if (!Enum.TryParse(match.Groups["mnemonic"].Value, true, out Mnemonic m)) {
                        Error($"Mnemonic \"{match.Groups["mnemonic"].Value}\" is invalid");
                        continue;
                    }

                    mnemonic = m;
                } else {
                    if (label != null)
                        toBind.Add(label);
                    continue;
                }

                var (optype1, optype2) = mnemonic.Operands();
                int opCount = (optype1.HasValue ? 1 : 0) + (optype2.HasValue ? 1 : 0);

                List<Operand> operands = new();
                if (match.Groups["op"].Captures.Count != opCount) {
                    Error(
                        $"Instruction {mnemonic.ToString()} does not take ${match.Groups["op"].Captures.Count} operands");
                    continue;
                }

                if (optype1.HasValue) {
                    try {
                        operands.Add(ParseOperand(match.Groups["op"].Captures[0].Value, optype1.Value));
                    } catch (Exception e) {
                        Error(e.Message);
                    }
                }

                if (optype2.HasValue) {
                    try {
                        operands.Add(ParseOperand(match.Groups["op"].Captures[1].Value, optype2.Value));
                    } catch (Exception e) {
                        Error(e.Message);
                    }
                }

                var instruction = new Instruction {
                    Mnemonic = mnemonic, Operands = operands.ToArray(), LineNumber = lineIndex, Address = memAddress
                };
                instructions.Add(instruction);
                memAddress += instruction.Mnemonic.Bytes();

                if (label != null) {
                    label.Instruction = instruction;
                }

                foreach (Label l in toBind)
                    l.Instruction = instruction;
                toBind.Clear();
            }

            foreach (Label label in labels.Values.Where(label => label.Instruction == null))
                Error(-1, $"Label \"{label.LabelName}\" was used but was never defined");

            return instructions;
        }

        private byte ParseAsByte(string s) {
            try {
                if (s.StartsWith("0x"))
                    return Convert.ToByte(s, 16);
                if (s.StartsWith("0b"))
                    return Convert.ToByte(s.Remove(0, 2), 2);
                if (s.StartsWith('-'))
                    return unchecked((byte) Convert.ToSByte(s));
                return Convert.ToByte(s);
            } catch (OverflowException) {
                throw new Exception($"Value \"{s}\" is not in range for a byte");
            } catch (FormatException) {
                throw new Exception($"Value \"{s}\" is in the wrong format");
            }
        }

        private Operand ParseOperand(string text, OperandType type) => type switch {
            OperandType.REGISTER => Enum.TryParse(text.ToUpper(), out Register r)
                ? new Operand.RegisterOperand(r)
                : throw new Exception($"Invalid register argument \"{text}\""),
            OperandType.IMM_VALUE => new Operand.ImmediateValueOperand(ParseAsByte(text)),
            OperandType.ADDRESS => char.IsDigit(text[0])
                ? new Operand.AddressOperand.ImmediateAddressOperand(ParseAsByte(text))
                : new Operand.AddressOperand.LabelOperand(AddOrReturnLabel(text)),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}