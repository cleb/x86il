using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private UInt32[] registers = {
            0, 0, 0, 0, 0, 0, 0, 0
        };

        private UInt32[] segments = {
            0, 0, 0, 0
        };


        public byte Get(Reg8 register)
        {
            if(register < Reg8.ah)
            {
                return (byte) registers[(int)register];
            } else
            {
                return (byte)(registers[(register - Reg8.ah)]>>8);
            }
        }
        public void Set(Reg8 register, byte value)
        {
            if (register < Reg8.ah)
            {
                registers[(int)register] &= 0xffffff00;
                registers[(int)register] |= value;
            }
            else
            {
                registers[(int)register] &= 0xffff00ff;
                registers[(register - Reg8.ah)] |= (uint)(value<<8);
            }
        }
        public UInt16 Get(Reg16 register)
        {
            return (UInt16)registers[(int)register];
        }
        public void Set(Reg16 register, UInt16 value)
        {
            registers[(int)register] |= (0xffff0000 | value);
        }
        public UInt16 Get(Segments segment)
        {
            return (UInt16)segments[(int)segment];
        }
        public void Set(Segments segment, UInt16 value)
        {
            segments[(int)segment] = value;
        }
        public UInt16 Get(UInt16 register, RegisterType type)
        {
            switch (type)
            {
                case RegisterType.reg8:
                    return Get((Reg8)register);
                case RegisterType.reg16:
                    return Get((Reg16)register);
                case RegisterType.segment:
                    return Get((Segments)register);
                default:
                    throw new InvalidOperationException();
            }
        }
        public void Set(UInt16 register, UInt16 value, RegisterType type)
        {
            switch (type)
            {
                case RegisterType.reg8:
                    Set((Reg8)register,(Byte)value);
                    break;
                case RegisterType.reg16:
                    Set((Reg16)register,value);
                    break;
                case RegisterType.segment:
                    Set((Segments)register,value);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }

}
