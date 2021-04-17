using System;
using System.Text;

namespace x86il
{
    internal class DosCmd
    {
        private readonly ICpu cpu;

        public DosCmd(ICpu c)
        {
            cpu = c;
        }

        private void TextOutput()
        {
            var sb = new StringBuilder();
            var i = cpu.GetRegister(Reg16.dx);
            byte value = 0;
            do
            {
                value = cpu.GetInDs(i);
                if (value != '$') sb.Append((char) value);
                i++;
            } while (value != '$');

            Console.Write(sb);
        }

        public void Int21h()
        {
            switch (cpu.GetRegister(Reg8.ah))
            {
                case 0x9:
                    TextOutput();
                    break;
                case 0x4c:
                    Environment.Exit(0);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}