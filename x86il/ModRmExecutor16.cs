using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x86il
{
    public class ModRmExecutor16 : ModRmExecutor
    {
        protected override RegisterType RegisterType => RegisterType.reg16;
        protected override RegisterType ComplementRegisterType => RegisterType.reg16;
        public ModRmExecutor16(Registers regs, FlagsRegister flags, byte[] mem, ModRMDecoder modRm) : base(regs, flags, mem, modRm)
        {
        }

        public override void WriteMemoryResult(ushort res)
        {
            BinaryHelper.Write16Bit(memory, decoder.Address, (UInt16)res);
        }

        public override ushort ReadMemory()
        {
            return BinaryHelper.Read16Bit(memory, decoder.Address);
        }
    }
}
