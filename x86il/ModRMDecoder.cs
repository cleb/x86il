﻿
using System;

namespace x86il
{
    public class ModRMDecoder
    {
        byte[] memory;
        Registers registers;
        
        public ushort Address { get; private set; }
        public ushort R1 { get; private set; }
        public ushort R2 { get; private set; }
        public ModRMType Type { get; private set; }

        public int IpShift { get; private set; }

        public ModRMDecoder(byte[] mem, Registers regs)
        {
            memory = mem;
            registers = regs;
        }

        public void Decode(int ip)
        {
            byte modrm = memory[ip + 1];
            R1 = GetRegSegmentFromModRm(modrm);
            switch (modrm >> 6)
            {
                case 0x00:
                case 0x01:
                case 0x02:
                    Address = getEffectiveAddressFromModRm(modrm, ip);
                    Type = ModRMType.RM;
                    break;
                case 0x03:
                    R2 = (UInt16)((modrm) & 7);
                    IpShift = 2;
                    Type = ModRMType.Reg;
                    break;
            }
        }

        private UInt16 getEffectiveAddressFromModRm(UInt16 modrm, int ip)
        {
            switch (modrm >> 6)
            {
                case 0x00:
                    if ((modrm & 7) == 0x6)
                    {
                        var imm16 = BinaryHelper.Read16Bit(memory, ip + 2);
                        IpShift = 4;
                        return imm16;
                    }
                    else
                    {
                        IpShift = 2;
                        return getEffectiveAddress(modrm);
                    }
                case 0x01:
                    byte displacement = memory[ip + 2];
                    IpShift = 3;
                    return (UInt16)(getEffectiveAddress(modrm) + displacement);
                case 0x02:
                    var disp16 = BinaryHelper.Read16Bit(memory, ip + 2); ;
                    IpShift = 4;
                    return (UInt16)(getEffectiveAddress(modrm) + disp16);
                default:
                    throw new InvalidOperationException();
            }
        }

        private UInt16 getEffectiveAddress(UInt16 modrm)
        {
            switch (modrm & 7)
            {
                case 0x0:
                    return (UInt16)(registers.Get(Reg16.bx) + registers.Get(Reg16.si));
                case 0x1:
                    return (UInt16)(registers.Get(Reg16.bx) + registers.Get(Reg16.di));
                case 0x2:
                    return (UInt16)(registers.Get(Reg16.bp) + registers.Get(Reg16.si));
                case 0x3:
                    return (UInt16)(registers.Get(Reg16.bp) + registers.Get(Reg16.di));
                case 0x4:
                    return registers.Get(Reg16.si);
                case 0x5:
                    return registers.Get(Reg16.di);
                case 0x6:
                    return registers.Get(Reg16.bp);
                case 0x7:
                    return registers.Get(Reg16.bx);
                default:
                    throw new NotImplementedException();
            }
        }

        public static ushort GetRegSegmentFromModRm(byte modrm)
        {
            return (UInt16)((modrm >> 3) & 7);
        }
    }
}
