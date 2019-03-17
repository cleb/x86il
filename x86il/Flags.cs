using System;

namespace x86il
{
    [FlagsAttribute]
    public enum Flags : UInt16
    {
        Carry = 1,
        Parity = 4,
        Adjust = 0x10,
        Zero = 0x40,
        Sign = 0x80,
        Trap = 0x100,
        Interrupt = 0x200,
        Direction = 0x400,
        Overflow = 0x800
    }
}
