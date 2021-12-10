using System;
using System.Text;

namespace LogicWorldAssembler {
    public struct Instruction {
        public Mnemonic Mnemonic { get; init; }
        public Register? Register { get; init; }
        public Register? Register2 { get; init; }
        public byte? ImmValue { get; init; }
        public Label? Label { get; init; }
        public int Address { get; init; }
        public int LineNumber { get; init; }

        public override string ToString() {
            return
                $"{nameof(Mnemonic)}: {Mnemonic}, {nameof(Register)}: {Register}, {nameof(Register2)}: {Register2}, {nameof(ImmValue)}: {ImmValue}, {nameof(Label)}: {Label?.LabelName}, {nameof(Address)}: {Address}, {nameof(LineNumber)}: {LineNumber}";
        }

        public string Reconstruct() {
            StringBuilder builder = new();
            builder.AppendFormat("{0:X2}: ", Address);
            builder.Append(Mnemonic.ToString());
            (var op1, var op2) = Mnemonic.Operands();
            if (op1.HasValue) {
                builder.Append(' ');
                string s = op1.Value switch {
                    OperandType.REGISTER => Register?.ToString() ?? "null",
                    OperandType.IMM_VALUE => ImmValue.HasValue ? "0x" + ImmValue.Value.ToString("X2") : "null",
                    OperandType.ADDRESS => ImmValue?.ToString("X2") ??
                                           (Label != null ? Label.LabelName + $" ({Label.Address:X2})" : "null"),
                    _ => throw new ArgumentOutOfRangeException()
                };
                builder.Append(s);
            }

            if (op2.HasValue) {
                builder.Append(' ');
                string s = op2.Value switch {
                    OperandType.REGISTER => Register2?.ToString() ?? "null",
                    OperandType.IMM_VALUE => ImmValue.HasValue ? "0x" + ImmValue.Value.ToString("X2") : "null",
                    OperandType.ADDRESS => ImmValue?.ToString("X2") ??
                                           (Label != null ? Label.LabelName + $" ({Label.Address:X2})" : "null"),
                    _ => throw new ArgumentOutOfRangeException()
                };
                builder.Append(s);
            }

            return builder.ToString();
        }

        public bool Verify() {
            (var op1, var op2) = Mnemonic.Operands();
            switch (op1) {
                case OperandType.REGISTER:
                    if (!Register.HasValue) throw new Exception("Register value is still null");
                    break;
                case OperandType.IMM_VALUE:
                    if (!ImmValue.HasValue) throw new Exception("Immediate value is still null");
                    break;
                case OperandType.ADDRESS:
                    if (!ImmValue.HasValue) {
                        if (Label == null)
                            throw new Exception("No address is available (immediate address and label are null)");
                        if (!Label.Address.HasValue) throw new Exception("Label address is still null");
                    }

                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (op2) {
                case OperandType.REGISTER:
                    if (!Register2.HasValue) throw new Exception("Second register value is still null");
                    break;
                case OperandType.IMM_VALUE:
                    if (!ImmValue.HasValue) throw new Exception("Immediate value is still null");
                    break;
                case OperandType.ADDRESS:
                    if (!ImmValue.HasValue) {
                        if (Label == null)
                            throw new Exception("No address is available (immediate address and label are null)");
                        if (!Label.Address.HasValue) throw new Exception("Label address is still null");
                    }

                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        public byte[] ToMachineCode() {
            Verify();
            byte b = 0;
            byte? immediate = null;

            if (Mnemonic == Mnemonic.NOP) return new byte[] {0x00};
            if (Mnemonic == Mnemonic.HLT) return new byte[] {0x07};

            if (Mnemonic is Mnemonic.MVI or Mnemonic.PSH or Mnemonic.POP or Mnemonic.LDR or Mnemonic.STR or
                Mnemonic.LDAX or Mnemonic.STAX or Mnemonic.ADD or Mnemonic.ADC or Mnemonic.SUB or Mnemonic.SBB or
                Mnemonic.ANA or Mnemonic.XRA or Mnemonic.ORA or Mnemonic.CMP or Mnemonic.INC or Mnemonic.DEC) {
                b |= Mnemonic switch {
                    Mnemonic.MVI => 0b00001000,
                    Mnemonic.PSH => 0b00010000,
                    Mnemonic.POP => 0b00011000,
                    Mnemonic.LDR => 0b00100000,
                    Mnemonic.STR => 0b00101000,
                    Mnemonic.LDAX => 0b00110000,
                    Mnemonic.STAX => 0b00111000,
                    Mnemonic.ADD => 0b10000000,
                    Mnemonic.ADC => 0b10001000,
                    Mnemonic.SUB => 0b10010000,
                    Mnemonic.SBB => 0b10011000,
                    Mnemonic.ANA => 0b10100000,
                    Mnemonic.XRA => 0b10101000,
                    Mnemonic.ORA => 0b10110000,
                    Mnemonic.CMP => 0b10111000,
                    Mnemonic.INC => 0b11000000,
                    Mnemonic.DEC => 0b11001000,
                    _ => 0
                };
                b |= (byte) Register!;
            }

            b |= Mnemonic switch {
                Mnemonic.ADI => 0b11010000,
                Mnemonic.ACI => 0b11010001,
                Mnemonic.SUI => 0b11010010,
                Mnemonic.SBI => 0b11010011,
                Mnemonic.ANI => 0b11010100,
                Mnemonic.XRI => 0b11010101,
                Mnemonic.ORI => 0b11010110,
                Mnemonic.CPI => 0b11010111,
                Mnemonic.SHL => 0b11011000,
                Mnemonic.SHR => 0b11011001,
                Mnemonic.RTL => 0b11011010,
                Mnemonic.RTR => 0b11011011,
                Mnemonic.NOT => 0b11011100,
                Mnemonic.NEG => 0b11011101,
                _ => 0
            };

            if (Mnemonic == Mnemonic.MOV) {
                b |= 0b01000000;

                b |= (byte) ((byte) Register! << 3);
                b |= (byte) Register2!;
            }

            b |= Mnemonic switch {
                Mnemonic.JMP => 0b11100000,
                Mnemonic.STSP => 0b11100001,
                Mnemonic.LDPC => 0b11100010,
                Mnemonic.LDSP => 0b11100011,
                Mnemonic.RET => 0b11100100,
                Mnemonic.CAL => 0b11100101,
                Mnemonic.PSSW => 0b11100110,
                Mnemonic.POSW => 0b11100111,
                _ => 0
            };

            b |= Mnemonic switch {
                Mnemonic.JC => 0b11101000,
                Mnemonic.JNC => 0b11101001,
                Mnemonic.JZ => 0b11101010,
                Mnemonic.JNZ => 0b11101011,
                Mnemonic.JO => 0b11101100,
                Mnemonic.JNO => 0b11101101,
                Mnemonic.JM => 0b11101110,
                Mnemonic.JP => 0b11101111,
                _ => 0
            };

            b |= Mnemonic switch {
                Mnemonic.RC => 0b11110000,
                Mnemonic.RNC => 0b11110001,
                Mnemonic.RZ => 0b11110010,
                Mnemonic.RNZ => 0b11110011,
                Mnemonic.RO => 0b11110100,
                Mnemonic.RNO => 0b11110101,
                Mnemonic.RM => 0b11110110,
                Mnemonic.RP => 0b11110111,
                _ => 0
            };

            b |= Mnemonic switch {
                Mnemonic.CC => 0b11111000,
                Mnemonic.CNC => 0b11111001,
                Mnemonic.CZ => 0b11111010,
                Mnemonic.CNZ => 0b11111011,
                Mnemonic.CO => 0b11111100,
                Mnemonic.CNO => 0b11111101,
                Mnemonic.CM => 0b11111110,
                Mnemonic.CP => 0b11111111,
                _ => 0
            };

            if (Mnemonic is Mnemonic.MVI or Mnemonic.ADI or Mnemonic.ACI or Mnemonic.SUI or Mnemonic.SBI or Mnemonic.ANI
                or Mnemonic.XRI or Mnemonic.ORI or Mnemonic.CPI)
                immediate = ImmValue;

            if (Mnemonic is Mnemonic.LDR or Mnemonic.STR or Mnemonic.JMP or Mnemonic.STSP or Mnemonic.CAL
                or Mnemonic.JC or Mnemonic.JNC or Mnemonic.JZ or Mnemonic.JNZ or Mnemonic.JO or Mnemonic.JNO or
                Mnemonic.JM or Mnemonic.JP
                or Mnemonic.CC or Mnemonic.CNC or Mnemonic.CZ or Mnemonic.CNZ or Mnemonic.CO or Mnemonic.CNO or
                Mnemonic.CM or Mnemonic.CP)
                immediate = ImmValue ?? (byte) Label!.Address!;

            if (immediate.HasValue) return new[] {b, immediate.Value};
            return new[] {b};
        }
    }
}