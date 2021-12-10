using System;

namespace LogicWorldAssembler {
    public enum OperandType {
        REGISTER,
        IMM_VALUE,
        ADDRESS
    }

    public enum Mnemonic {
        NOP,
        HLT,
        MVI,
        PSH,
        POP,
        LDR,
        STR,
        LDAX,
        STAX,
        MOV,
        ADD,
        ADC,
        SUB,
        SBB,
        ANA,
        XRA,
        ORA,
        CMP,
        INC,
        DEC,
        ADI,
        ACI,
        SUI,
        SBI,
        ANI,
        XRI,
        ORI,
        CPI,
        SHL,
        SHR,
        RTL,
        RTR,
        NOT,
        NEG,
        JMP,
        STSP,
        LDPC,
        LDSP,
        RET,
        CAL,
        PSSW,
        POSW,
        JC,
        JNC,
        JZ,
        JNZ,
        JO,
        JNO,
        JM,
        JP,
        RC,
        RNC,
        RZ,
        RNZ,
        RO,
        RNO,
        RM,
        RP,
        CC,
        CNC,
        CZ,
        CNZ,
        CO,
        CNO,
        CM,
        CP
    }

    public enum Register {
        A = 0,
        B,
        C,
        D,
        E,
        F,
        G,
        M
    }

    public static class MnemonicExtensions {
        public static (OperandType?, OperandType?) Operands(this Mnemonic m) {
            return m switch {
                Mnemonic.NOP or Mnemonic.HLT => (null, null),
                Mnemonic.MVI => (OperandType.REGISTER, OperandType.IMM_VALUE),
                Mnemonic.LDR or Mnemonic.STR => (OperandType.REGISTER, OperandType.ADDRESS),
                Mnemonic.PSH or Mnemonic.POP or Mnemonic.LDAX or Mnemonic.STAX => (OperandType.REGISTER, null),
                Mnemonic.MOV => (OperandType.REGISTER, OperandType.REGISTER),
                Mnemonic.ADD or Mnemonic.ADC or Mnemonic.SUB or Mnemonic.SBB => (OperandType.REGISTER, null),
                Mnemonic.ANA or Mnemonic.XRA or Mnemonic.ORA or Mnemonic.CMP => (OperandType.REGISTER, null),
                Mnemonic.INC or Mnemonic.DEC => (OperandType.REGISTER, null),
                Mnemonic.ADI or Mnemonic.ACI or Mnemonic.SUI or Mnemonic.SBI => (OperandType.IMM_VALUE, null),
                Mnemonic.ANI or Mnemonic.XRI or Mnemonic.ORI or Mnemonic.CPI => (OperandType.IMM_VALUE, null),
                Mnemonic.SHL or Mnemonic.SHR or Mnemonic.RTL or Mnemonic.RTR
                    or Mnemonic.NOT or Mnemonic.NEG => (null, null),
                Mnemonic.JMP or Mnemonic.STSP or Mnemonic.CAL => (OperandType.ADDRESS, null),
                Mnemonic.LDPC or Mnemonic.LDSP or Mnemonic.RET or Mnemonic.PSSW or Mnemonic.POSW => (null, null),
                Mnemonic.JC or Mnemonic.JNC or Mnemonic.JZ or Mnemonic.JNZ or
                    Mnemonic.JO or Mnemonic.JNO or Mnemonic.JM or Mnemonic.JP => (OperandType.ADDRESS, null),
                Mnemonic.RC or Mnemonic.RNC or Mnemonic.RZ or Mnemonic.RNZ or
                    Mnemonic.RO or Mnemonic.RNO or Mnemonic.RM or Mnemonic.RP => (null, null),
                Mnemonic.CC or Mnemonic.CNC or Mnemonic.CZ or Mnemonic.CNZ or
                    Mnemonic.CO or Mnemonic.CNO or Mnemonic.CM or Mnemonic.CP => (OperandType.ADDRESS, null),
                _ => throw new ArgumentOutOfRangeException(nameof(m), $"Unexpected mnemonic value: {m}")
            };
        }
    }
}