namespace x86il
{
    public class ModRmExecutorSegment32b : ModRmExecutor
    {
        public ModRmExecutorSegment32b(Registers regs, FlagsRegister flags, byte[] mem, ModRMDecoder modRm) : base(regs,
            flags, mem, modRm)
        {
        }

        protected override RegisterType RegisterType => RegisterType.reg16;
        protected override RegisterType ComplementRegisterType => RegisterType.segment;

        public override void WriteMemoryResult(ushort res)
        {
            BinaryHelper.Write16Bit(memory, decoder.Address, res);
        }

        public override uint ReadMemory()
        {
            return BinaryHelper.Read32Bit(memory, decoder.Address);
        }
    }
}