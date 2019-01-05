using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x86il
{
    interface ICpu
    {
        void Execute(int ipStart, int ipEnd);
        Byte GetRegister(Reg8 register);
        UInt16 GetRegister(Reg16 register);
        void SetRegister(Reg8 register, Byte value);
        void SetRegister(Reg16 register, UInt16 value);
        void SetRegister(Segments register, UInt16 value);

        Byte GetInDs(UInt16 offset);
        void SetInterruptHandler(Byte number, Action handler);
        
    }
}
