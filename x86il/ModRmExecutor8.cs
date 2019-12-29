using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x86il
{
    public class ModRmExecutor8 : ModRmExecutor
    {
        public ModRmExecutor8(Registers regs, FlagsRegister flags, byte[] mem, ModRMDecoder modRm) : base(regs, flags, mem, modRm)
        {
        }

        protected override RegisterType RegisterType => RegisterType.reg8;
        protected override RegisterType ComplementRegisterType => RegisterType.reg8;

        public override ushort ReadMemory()
        {
            return memory[decoder.Address];
        }

        public override void WriteMemoryResult(ushort res)
        {
            memory[decoder.Address] = (byte)res;
        }
    }
}
