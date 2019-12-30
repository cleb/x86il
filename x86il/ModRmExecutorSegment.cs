using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x86il
{
    public class ModRmExecutorSegment : ModRmExecutor
    {
        protected override RegisterType RegisterType => RegisterType.segment;
        protected override RegisterType ComplementRegisterType => RegisterType.reg16;
        public ModRmExecutorSegment(Registers regs, FlagsRegister flags, byte[] mem, ModRMDecoder modRm) : base(regs, flags, mem, modRm)
        {
        }

        public override void WriteMemoryResult(ushort res)
        {
            BinaryHelper.Write16Bit(memory, decoder.Address, (UInt16)res);
        }

        public override uint ReadMemory()
        {
            return BinaryHelper.Read16Bit(memory, decoder.Address);
        }
    }
}
