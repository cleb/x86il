using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x86il
{
    interface IRegisters
    {
        byte Get(Enum register);
        void Set(Enum register, byte value);
        void Set(Enum register, UInt16 value);
    }
}
