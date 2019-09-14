using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x86il
{
    public class FlagsRegister
    {
        public Flags CpuFlags { get; private set; }

        public void CheckZero(Int32 result, UInt16 input1, UInt16 input2)
        {
            if (result == 0)
            {
                CpuFlags |= Flags.Zero;
            }
        }
        public void CheckCarry(Int32 result, UInt16 input1, UInt16 input2, int bytes = 1)
        {
            if (result >= (bytes << 8) || result < 0)
            {
                CpuFlags |= Flags.Carry;
            }
        }
        public void CheckOverflow(Int32 result, UInt16 input1, UInt16 input2, int bytes = 1)
        {
            Int16 adjusted1 = (Int16)(bytes == 1 ? (sbyte)input1 : (Int16)input1);
            Int16 adjusted2 = (Int16)(bytes == 1 ? (sbyte)input2 : (Int16)input2);
            Int16 adjustedResult = (Int16)(bytes == 1 ? (sbyte)result : (Int16)result);

            if ((adjusted1 > 0 && adjusted2 > 0 && adjustedResult < 0)
                || (adjusted1 < 0 && adjusted2 < 0 && adjustedResult > 0))
            {
                CpuFlags |= Flags.Overflow;
            }
        }

        public bool HasFlag(Flags flag)
        {
            return CpuFlags.HasFlag(flag);
        }
    }
}
