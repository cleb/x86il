using System;

namespace x86il
{
    class Emulator
    {
        public ICpu cpu;
        public Byte[] memory;
        DosCmd dos;
        public Emulator()
        {
            memory = new Byte[1048576];
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
