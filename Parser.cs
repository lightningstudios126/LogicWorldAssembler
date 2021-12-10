using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace LogicWorldAssembler {
    public class Parser {
        private static readonly Regex regex = new(
            @"^(?:(?<label>\w+):[ \t]*)?(?:(?<mnemonic>[a-zA-Z]+)(?:[ \t]+(?<op1>\w+)(?:,?[ \t]*(?<op2>\w+))?)?)?[ \t]*(?:;.*)?$",
            RegexOptions.Compiled);

        private readonly List<Instruction> instructions = new();
        private readonly Dictionary<string, Label> labels = new();

        private int lineIndex;
        private readonly TextReader source;

        public Parser(TextReader source) {
            this.source = source;
        }

        public bool HasError { get; private set; }

        private void Error(int lineNumber, string errorMessage) {
            Console.WriteLine($"Line {lineNumber}: " + errorMessage);
            HasError = true;
        }

        private void Error(string errorMessage) {
            Error(lineIndex, errorMessage);
        }

        private Label AddOrReturnLabel(string label, int? address) {
            if (!labels.ContainsKey(label)) {
                Label l = new(label, address);
                labels.Add(label, l);
            }

            return labels[label];
        }

        public List<Instruction> Scan() {
            var byteIndex = 0;

            string? line;
            while (true) {
                line = source.ReadLine();
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

                if (match.Groups["label"].Success) {
                    if (labels.ContainsKey(match.Groups["label"].Value))
                        labels[match.Groups["label"].Value].Address = byteIndex;
                    else AddOrReturnLabel(match.Groups["label"].Value, byteIndex);
                }

                Mnemonic mnemonic;
                int address;
                Register? register = null;
                Register? register2 = null;
                byte? immValue = null;
                Label? label = null;

                if (match.Groups["mnemonic"].Success) {
                    if (!Enum.TryParse(match.Groups["mnemonic"].Value, true, out Mnemonic m)) {
                        Error($"Mnemonic \"{match.Groups["mnemonic"].Value}\" is invalid");
                        continue;
                    }

                    mnemonic = m;
                    address = byteIndex;
                    byteIndex++;
                } else {
                    continue;
                }

                (var optype1, var optype2) = mnemonic.Operands();

                if (optype1.HasValue) {
                    if (!match.Groups["op1"].Success)
                        Error(lineIndex, $"Operand missing for instruction \"{mnemonic.ToString()}\"");

                    if (optype1.Value == OperandType.REGISTER) {
                        if (Enum.TryParse(match.Groups["op1"].Value.ToUpper(), out Register r)) register = r;
                        else
                            Error(
                                $"Invalid register argument \"{match.Groups["op1"].Value}\" for instruction \"{mnemonic.ToString()}\"");
                    } else if (optype1.Value == OperandType.IMM_VALUE) {
                        var result = ParseAsByte(match.Groups["op1"].Value);
                        if (result.HasValue) immValue = result;
                        byteIndex++;
                    } else if (optype1.Value == OperandType.ADDRESS) {
                        if (char.IsDigit(match.Groups["op1"].Value[0])) {
                            var result = ParseAsByte(match.Groups["op1"].Value);
                            if (result.HasValue) immValue = result;
                        } else {
                            label = AddOrReturnLabel(match.Groups["op1"].Value, null);
                        }

                        byteIndex++;
                    }
                } else {
                    if (match.Groups["op1"].Success)
                        Error(lineIndex,
                            $"Operand \"{match.Groups["op1"].Value}\" for instruction \"{mnemonic.ToString()}\" not expected");
                }

                if (optype2.HasValue) {
                    if (!match.Groups["op2"].Success)
                        Error(lineIndex, $"Operand missing for instruction \"{mnemonic.ToString()}\"");

                    if (optype2.Value == OperandType.REGISTER) {
                        if (Enum.TryParse(match.Groups["op2"].Value.ToUpper(), out Register r)) register2 = r;
                        else
                            Error(
                                $"Invalid register argument \"{match.Groups["op2"].Value}\" for instruction \"{mnemonic.ToString()}\"");
                    } else if (optype2.Value == OperandType.IMM_VALUE) {
                        var result = ParseAsByte(match.Groups["op2"].Value);
                        if (result.HasValue) immValue = result;
                        byteIndex++;
                    } else if (optype2.Value == OperandType.ADDRESS) {
                        if (char.IsDigit(match.Groups["op2"].Value[0])) {
                            var result = ParseAsByte(match.Groups["op2"].Value);
                            if (result.HasValue) immValue = result;
                        } else {
                            label = AddOrReturnLabel(match.Groups["op2"].Value, null);
                        }

                        byteIndex++;
                    }
                } else {
                    if (match.Groups["op2"].Success)
                        Error(lineIndex,
                            $"Operand \"{match.Groups["op2"].Value}\" for instruction \"{mnemonic.ToString()}\" not expected");
                }

                instructions.Add(new Instruction {
                    Mnemonic = mnemonic, Register = register, Register2 = register2, ImmValue = immValue, Label = label,
                    Address = address, LineNumber = lineIndex
                });
            }

            foreach (var instruction in instructions)
                if (instruction.Label != null)
                    if (instruction.Label.Address == null)
                        Error(instruction.LineNumber,
                            $"Label \"{instruction.Label.LabelName}\" was used but was never defined");
            return instructions;
        }

        private byte? ParseAsByte(string s) {
            if (s.StartsWith("0x"))
                try {
                    return Convert.ToByte(s, 16);
                } catch (OverflowException) {
                    Error($"Value \"{s}\" is too large for a byte");
                } catch (FormatException) {
                    Error($"Value \"{s}\" is in the wrong format for a hexadecimal value");
                }
            else if (s.StartsWith("0b"))
                try {
                    return Convert.ToByte(s.Remove(0, 2), 2);
                } catch (OverflowException) {
                    Error($"Value \"{s}\" is too large for a byte");
                } catch (FormatException) {
                    Error($"Value \"{s}\" is in the wrong format for a binary value");
                }
            else if (s.StartsWith('-'))
                try {
                    return unchecked((byte) Convert.ToSByte(s));
                } catch (OverflowException) {
                    Error($"Value \"{s}\" is too small for a byte");
                } catch (FormatException) {
                    Error($"Value \"{s}\" is in the wrong format");
                }
            else
                try {
                    return Convert.ToByte(s);
                } catch (OverflowException) {
                    Error($"Value \"{s}\" is too large for a byte");
                } catch (FormatException) {
                    Error($"Value \"{s}\" is in the wrong format");
                }

            return null;
        }
    }
}