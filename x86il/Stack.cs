namespace x86il
{
    public class Stack
    {
        private readonly byte[] memory;
        private readonly Registers registers;

        public Stack(Registers registers, byte[] memory)
        {
            this.registers = registers;
            this.memory = memory;
        }

        public void PushValue(ushort Value)
        {
            var sp = registers.Get(Reg16.sp) - 2;
            BinaryHelper.Write16Bit(memory, (registers.Get(Segments.ss) << 4) + sp, Value);
            registers.Set(Reg16.sp, (ushort) sp);
        }

        public ushort PopValue16()
        {
            var sp = registers.Get(Reg16.sp);
            var ret = BinaryHelper.Read16Bit(memory, (registers.Get(Segments.ss) << 4) + sp);
            registers.Set(Reg16.sp, (ushort) (sp + 2));
            return ret;
        }

        public void Push(Segments seg)
        {
            var value = registers.Get(seg);
            PushValue(value);
        }

        public void Push(Reg16 reg)
        {
            var value = registers.Get(reg);
            PushValue(value);
        }

        public void Pop(Segments seg)
        {
            var segment = PopValue16();
            registers.Set(seg, segment);
        }

        public void Pop(Reg16 reg)
        {
            var register = PopValue16();
            registers.Set(reg, register);
        }

        public void Push(params Reg16[] regs)
        {
            foreach (var reg in regs)
                Push(reg);
        }

        public void Pop(params Reg16[] regs)
        {
            foreach (var reg in regs)
                Pop(reg);
        }
    }
}