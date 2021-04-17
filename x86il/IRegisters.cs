using System;

namespace x86il
{
    internal interface IRegisters
    {
        byte Get(Enum register);
        void Set(Enum register, byte value);
        void Set(Enum register, ushort value);
    }
}