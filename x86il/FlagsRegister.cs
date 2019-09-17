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

        public void SetFlagBasedOnResult(Flags flag, bool state)
        {
            if (state)
            {
                CpuFlags |= flag;
            } else
            {
                CpuFlags &= ~flag;
            }
        }

        public void CheckZero(Int32 result, UInt16 input1, UInt16 input2)
        {
            SetFlagBasedOnResult(Flags.Zero, result == 0);
        }
        public void CheckCarry(Int32 result, UInt16 input1, UInt16 input2, int bytes = 1)
        {
            SetFlagBasedOnResult(Flags.Carry, result >= (bytes << 8) || result < 0);
        }
        public void CheckOverflow(Int32 result, UInt16 input1, UInt16 input2, int bytes = 1)
        {
            Int16 adjusted1 = (Int16)(bytes == 1 ? (sbyte)input1 : (Int16)input1);
            Int16 adjusted2 = (Int16)(bytes == 1 ? (sbyte)input2 : (Int16)input2);
            Int16 adjustedResult = (Int16)(bytes == 1 ? (sbyte)result : (Int16)result);
            SetFlagBasedOnResult(Flags.Overflow, (adjusted1 > 0 && adjusted2 > 0 && adjustedResult < 0)
                || (adjusted1 < 0 && adjusted2 < 0 && adjustedResult > 0));
        }

        public void CheckParity(Int32 result)
        {
            var lowest = result & 0xff;
            bool isEven = true;
            for(var i = 0; i < 8; i++)
            {
                if((lowest & 1) != 0)
                {
                    isEven = !isEven;
                }
                lowest >>= 1;
            }
            SetFlagBasedOnResult(Flags.Parity, isEven);
        }

        public void CheckSign(UInt32 result, int bytes = 1)
        {
            Int16 adjusted = (Int16)(bytes == 1 ? (sbyte)result : (Int16)result);
            SetFlagBasedOnResult(Flags.Sign, adjusted < 0);
        }

        public bool HasFlag(Flags flag)
        {
            return CpuFlags.HasFlag(flag);
        }
    }
}
