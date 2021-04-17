namespace x86il
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var emulator = new Emulator();
            byte[] Code =
            {
                0x0E, 0x1F, 0xBA, 0x0D, 0x01, 0xB4, 0x09, 0xCD, 0x21, 0xB4, 0x4C, 0xCD, 0x21, 0x74, 0x65, 0x73, 0x74,
                0x0D, 0x0A, 0x24
            };
            for (var i = 0; i < Code.Length; i++) emulator.memory[i + 0x100] = Code[i];
            emulator.Execute();
        }
    }
}