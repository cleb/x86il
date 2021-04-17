namespace x86il
{
    internal class Emulator
    {
        public ICpu cpu;
        private readonly DosCmd dos;
        public byte[] memory;

        public Emulator()
        {
            memory = new byte[1048576];
            cpu = new InterpretCpu(memory);
            cpu.SetRegister(Segments.cs, 0);
            cpu.SetRegister(Segments.ss, 0);
            cpu.SetRegister(Reg16.sp, 0xfe);
            dos = new DosCmd(cpu);
            cpu.SetInterruptHandler(0x21, dos.Int21h);
        }

        public void Execute()
        {
            cpu.Execute(0x100, 65536);
        }
    }
}