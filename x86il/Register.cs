using System;

namespace x86il
{
    public enum Reg8
    {
        al = 0,
        cl = 1,
        dl = 2,
        bl = 3,
        ah = 4,
        ch = 5,
        dh = 6,
        bh = 7
    }

    public enum Reg16
    {
        ax = 0,
        cx = 1,
        dx = 2,
        bx = 3,
        sp = 4,
        bp = 5,
        si = 6,
        di = 7
    }

    public enum Segments
    {
        es = 0,
        cs = 1,
        ss = 2,
        ds = 3
    }

    public enum RegisterType
    {
        reg8,
        reg16,
        segment
    }

    public class Registers
    {
        private readonly uint[] registers =
        {
            0, 0, 0, 0, 0, 0, 0, 0
        };

        private readonly uint[] segments =
        {
            0, 0, 0, 0
        };


        public byte Get(Reg8 register)
        {
            if (register < Reg8.ah)
                return (byte) registers[(int) register];
            return (byte) (registers[register - Reg8.ah] >> 8);
        }

        public void Set(Reg8 register, byte value)
        {
            if (register < Reg8.ah)
            {
                registers[(int) register] &= 0xff00;
                registers[(int) register] |= value;
            }
            else
            {
                registers[(int) register - (int) Reg8.ah] &= 0xff;
                registers[register - Reg8.ah] |= (uint) (value << 8);
            }
        }

        public ushort Get(Reg16 register)
        {
            return (ushort) registers[(int) register];
        }

        public void Set(Reg16 register, ushort value)
        {
            registers[(int) register] = value;
        }

        public ushort Get(Segments segment)
        {
            return (ushort) segments[(int) segment];
        }

        public void Set(Segments segment, ushort value)
        {
            segments[(int) segment] = value;
        }

        public ushort Get(ushort register, RegisterType type)
        {
            switch (type)
            {
                case RegisterType.reg8:
                    return Get((Reg8) register);
                case RegisterType.reg16:
                    return Get((Reg16) register);
                case RegisterType.segment:
                    return Get((Segments) register);
                default:
                    throw new InvalidOperationException();
            }
        }

        public void Set(ushort register, ushort value, RegisterType type)
        {
            switch (type)
            {
                case RegisterType.reg8:
                    Set((Reg8) register, (byte) value);
                    break;
                case RegisterType.reg16:
                    Set((Reg16) register, value);
                    break;
                case RegisterType.segment:
                    Set((Segments) register, value);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}