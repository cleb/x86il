using System;

namespace x86il
{
    class Program
    {
        static void Main(string[] args)
        {
            Emulator emulator = new Emulator();
            Byte[] Code = { 0x0E, 0x1F, 0xBA, 0x0D, 0x01, 0xB4, 0x09, 0xCD, 0x21, 0xB4, 0x4C, 0xCD, 0x21, 0x74, 0x65, 0x73, 0x74, 0x0D, 0x0A, 0x24 };
            for (int i = 0; i <  Code.Length; i++)
            {
                emulator.memory[i + 0x100] = Code[i];
            }
            emulator.Execute();
        }
    }
}
