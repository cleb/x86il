namespace x86il
{
    public class ModRmExecutor16 : ModRmExecutor
    {
        public ModRmExecutor16(Registers regs, FlagsRegister flags, byte[] mem, ModRMDecoder modRm) : base(regs, flags,
            mem, modRm)
        {
        }

        protected override RegisterType RegisterType => RegisterType.reg16;
        protected override RegisterType ComplementRegisterType => RegisterType.reg16;

        public override void WriteMemoryResult(ushort res)
        {
            BinaryHelper.Write16Bit(memory, decoder.Address, res);
        }

        public override uint ReadMemory()
        {
            return BinaryHelper.Read16Bit(memory, decoder.Address);
        }
    }
}