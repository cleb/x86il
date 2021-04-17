namespace x86il
{
    public class FlagsRegister
    {
        public Flags CpuFlags { get; private set; }

        public void SetFlagBasedOnResult(Flags flag, bool state)
        {
            if (state)
                CpuFlags |= flag;
            else
                CpuFlags &= ~flag;
        }

        public void CheckZero(int result, uint input1, uint input2)
        {
            SetFlagBasedOnResult(Flags.Zero, result == 0);
        }

        public void CheckCarry(int result, uint input1, uint input2, int bytes = 1)
        {
            SetFlagBasedOnResult(Flags.Carry, result >= bytes << 8 || result < 0);
        }

        public void CheckOverflow(int result, uint input1, uint input2, int bytes = 1)
        {
            var adjusted1 = bytes == 1 ? (sbyte) input1 : (short) input1;
            var adjusted2 = bytes == 1 ? (sbyte) input2 : (short) input2;
            var adjustedResult = bytes == 1 ? (sbyte) result : (short) result;
            SetFlagBasedOnResult(Flags.Overflow, adjusted1 > 0 && adjusted2 > 0 && adjustedResult < 0
                                                 || adjusted1 < 0 && adjusted2 < 0 && adjustedResult > 0);
        }

        public void CheckParity(int result)
        {
            var lowest = result & 0xff;
            var isEven = true;
            for (var i = 0; i < 8; i++)
            {
                if ((lowest & 1) != 0) isEven = !isEven;
                lowest >>= 1;
            }

            SetFlagBasedOnResult(Flags.Parity, isEven);
        }

        public void CheckSign(uint result, int bytes = 1)
        {
            var adjusted = bytes == 1 ? (sbyte) result : (short) result;
            SetFlagBasedOnResult(Flags.Sign, adjusted < 0);
        }

        public bool HasFlag(Flags flag)
        {
            return CpuFlags.HasFlag(flag);
        }

        public void SetFlagsFromInputAndResult(int result, uint input1, uint input2, int bytes = 1)
        {
            CheckZero(result, input1, input2);
            CheckCarry(result, input1, input2, bytes);
            CheckOverflow(result, input1, input2, bytes);
            CheckSign((uint) result, bytes);
            CheckParity(result);
        }
    }
}