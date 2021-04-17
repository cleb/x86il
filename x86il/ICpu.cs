using System;

namespace x86il
{
    internal interface ICpu
    {
        void Execute(int ipStart, int ipEnd);
        byte GetRegister(Reg8 register);
        ushort GetRegister(Reg16 register);
        void SetRegister(Reg8 register, byte value);
        void SetRegister(Reg16 register, ushort value);
        void SetRegister(Segments register, ushort value);

        byte GetInDs(ushort offset);
        void SetInterruptHandler(byte number, Action handler);
    }
}