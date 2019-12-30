using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x86il
{
    public class ModRmExecutorSegment32b : ModRmExecutor
    {
        protected override RegisterType RegisterType => RegisterType.reg16;
        protected override RegisterType ComplementRegisterType => RegisterType.segment;
        public ModRmExecutorSegment32b(Registers regs, FlagsRegister flags, byte[] mem, ModRMDecoder modRm) : base(regs, flags, mem, modRm)
        {
        }

        public override void WriteMemoryResult(ushort res)
        {
            BinaryHelper.Write16Bit(memory, decoder.Address, (UInt16)res);
        }

        public override uint ReadMemory()
        {
            return BinaryHelper.Read32Bit(memory, decoder.Address);
        }
    }
}
