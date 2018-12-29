using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x86il
{
    interface ICpu
    {
        void Execute(Byte[] code);
    }
}
