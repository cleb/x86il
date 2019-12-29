using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x86il
{
    static class EnumExtensions
    {
        public static int ToNumBytes(this RegisterType type)
        {
            return type == RegisterType.reg8 ? 1 : 2;
        }
    }
}
