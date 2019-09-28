using System;
using System.IO;
using NUnit.Framework;

namespace x86il.Tests
{
    [TestFixture]
    public class InterpretCpuTests
    {
        private InterpretCpu RunAsmTest(string filename)
        {
            var filePath = TestContext.CurrentContext.TestDirectory + $"/TestAsm/{filename}";
            
            if(!File.Exists($"{filePath}.o"))
            {
                var compile = System.Diagnostics.Process.Start("nasm", $"{filePath}.asm -o {filePath}.o");
                compile.WaitForExit();
            }
            var memory = File.ReadAllBytes($"{filePath}.o");
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, memory.Length);
            return cpu;
        }
        [Test]
        public void ExecuteTest()
        {
            var memory = new Byte[] { 0xb4, 0x04 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0,1);
            Assert.AreEqual(4, cpu.GetRegister(Reg8.ah));
        }

        [Test]
        public void ExecuteTestXor()
        {
            var memory = new Byte[] { 0xb0, 0x04, 0x30, 0xc0 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 1);
            Assert.AreEqual(4, cpu.GetRegister(Reg8.al));
            cpu.Execute(2, 3);
            Assert.AreEqual(0, cpu.GetRegister(Reg8.al));
        }
        [Test]
        public void ExecuteTestMov16()
        {
            var memory = new Byte[] { 0xbb, 0x42, 0x00 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 2);
            Assert.AreEqual(0x42, cpu.GetRegister(Reg16.bx));
        }
        [Test]
        public void ExecuteTestMovEsAx()
        {
            var memory = new Byte[] { 0xb8, 0x42, 0x00, 0x8e, 0xc0 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 5);
            Assert.AreEqual(0x42, cpu.GetRegister(Segments.es));
        }
        [Test]
        public void ExecuteTestAddAhAl()
        {
            var memory = new Byte[] { 0xB4, 0x04, 0xB0, 0x08, 0x00, 0xC4 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 6);
            Assert.AreEqual(12, cpu.GetRegister(Reg8.ah));
        }
        [Test]
        public void ExecuteTestAddBxSiAl()
        {
            var memory = new Byte[] { 0x00,0x00,0x4 };
            var cpu = new InterpretCpu(memory);
            cpu.SetRegister(Reg8.al, 8);
            cpu.SetRegister(Reg16.si, 1);
            cpu.SetRegister(Reg16.bx, 1);
            cpu.Execute(0, 2);
            Assert.AreEqual(12, memory[2]);
        }
        [Test]
        public void ExecuteTestAddToImm()
        {
            var memory = new Byte[] { 0xB4, 0x04, 0x00, 0x26, 0x06, 0x00, 0x08 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 6);
            Assert.AreEqual(12, memory[6]);
        }
        [Test]
        public void ExecuteTestAddAlImmAddr8()
        {
            var memory = new Byte[] { 0xB0, 0x04, 0x02, 0x06, 0x06, 0x00, 0x08 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 6);
            Assert.AreEqual(12, cpu.GetRegister(Reg8.al));
        }
        [Test]
        public void ExecuteTestAddAxImmAddr16()
        {
            
            var memory = new Byte[] { 0xB8, 0xCF, 0x12, 0x03, 0x06, 0x07, 0x00, 0x57, 0x06 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 6);
            Assert.AreEqual(6438, cpu.GetRegister(Reg16.ax));
        }
        [Test]
        public void ExecuteTestAddAlImm8()
        {
            var memory = new Byte[] { 0xB0, 0x04, 0x04, 0x08 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 3);
            Assert.AreEqual(12, cpu.GetRegister(Reg8.al));
        }
        [Test]
        public void ExecuteTestAddAxImm16()
        {
            var memory = new Byte[] { 0xB8, 0xCF, 0x12, 0x05, 0x57, 0x06 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 5);
            Assert.AreEqual(6438, cpu.GetRegister(Reg16.ax));
        }
        [Test]
        public void ExecuteTestPushEs()
        {
            var memory = new Byte[] { 0xB8, 0x2A, 0x00, 0x8E, 0xC0, 0x06, 0x1F,0x00,0x00 };
            var cpu = new InterpretCpu(memory);
            cpu.SetRegister(Reg16.sp, 8);
            cpu.Execute(0, 7);
            Assert.AreEqual(42, cpu.GetRegister(Segments.ds));
        }
        [Test]
        public void ExecuteTestOrMem8()
        {
            var memory = new Byte[] { 0xB3, 0x17, 0x08, 0x1E, 0x06, 0x00, 0x2A };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 5);
            Assert.AreEqual(0x3f, memory[6]);
        }
        [Test]
        public void ExecuteTestOrMem16()
        {
            var memory = new Byte[] { 0xBB, 0xCF, 0x12, 0x09, 0x1E, 0x07, 0x00, 0x57, 0x06 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 6);
            Assert.AreEqual(0x16df, BinaryHelper.Read16Bit(memory,7));
        }
        [Test]
        public void ExecuteTestOrAlImm8()
        {
            var memory = new Byte[] { 0xB0, 0x17, 0x0C, 0x2A };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 3);
            Assert.AreEqual(0x3f, cpu.GetRegister(Reg8.al));
        }
        [Test]
        public void ExecuteTestOrAxImm16()
        {
            var memory = new Byte[] { 0xB8, 0xEC, 0x05, 0x0D, 0x26, 0x09 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 5);
            Assert.AreEqual(0xdee, cpu.GetRegister(Reg16.ax));
        }
        [Test]
        public void ExecuteTestAdc()
        {
            var memory = new Byte[] { 0xB4, 0x04, 0xB7, 0xFF, 0x00, 0xFC, 0xB3, 0x04, 0xB0, 0x08, 0x10, 0xC3 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 11);
            Assert.AreEqual(13, cpu.GetRegister(Reg8.bl));
        }
        [Test]
        public void ExecuteTestAdc16()
        {
            var memory = new Byte[] { 0xb8, 0x04, 0x00, 0xbb, 0xff, 0xff, 0x01, 0xd8, 0xbb, 0x04, 0x00, 0xb8, 0x08, 0x00, 0x11, 0xc3 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 0x10);
            Assert.AreEqual(13, cpu.GetRegister(Reg8.bl));
        }
        [Test]
        public void ExecuteTestAdcr8rm8()
        {
            var memory = new Byte[] { 0xB4, 0x04, 0xB7, 0xFF, 0x00, 0xFC, 0xB3, 0x04, 0x12, 0x1E, 0x0C, 0x00, 0x08 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 11);
            Assert.AreEqual(13, cpu.GetRegister(Reg8.bl));
        }
        [Test]
        public void ExecuteTestAdc8Imm8()
        {
            var memory = new Byte[] { 0xB4, 0x04, 0xB7, 0xFF, 0x00, 0xFC, 0xB0, 0x04, 0x14, 0x08 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 9);
            Assert.AreEqual(13, cpu.GetRegister(Reg8.al));
        }
        [Test]
        public void ExecuteTestAdc16Imm16()
        {
            var memory = new Byte[] { 0xB4, 0x04, 0xB7, 0xFF, 0x00, 0xFC, 0xB8, 0x30, 0x00, 0x15, 0xEC, 0x05 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 12);
            Assert.AreEqual(1565, cpu.GetRegister(Reg16.ax));
        }
        [Test]
        public void ExecuteTestSbb816Rm8()
        {
            var memory = new Byte[] { 0xB4, 0x04, 0xB7, 0xFF, 0x00, 0xFC, 0xB0, 0x04, 0xB3, 0x08, 0x18, 0xC3 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 11);
            Assert.AreEqual(3, cpu.GetRegister(Reg8.bl));
        }
        [Test]
        public void ExecuteTestSbb1616Rm16()
        {
            var memory = new Byte[] { 0xB4, 0x04, 0xB7, 0xFF, 0x00, 0xFC, 0xB8, 0x30, 0x00, 0xBB, 0xEC, 0x05, 0x19, 0xC3 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 13);
            Assert.AreEqual(1467, cpu.GetRegister(Reg16.bx));
        }
        [Test]
        public void ExecuteTestSbb8Imm8()
        {
            var memory = new Byte[] { 0xB4, 0x04, 0xB7, 0xFF, 0x00, 0xFC, 0xB0, 0x08, 0x1C, 0x04 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 9);
            Assert.AreEqual(3, cpu.GetRegister(Reg8.al));
        }
        [Test]
        public void ExecuteTestSbb16Imm16()
        {
            var memory = new Byte[] { 0xB4, 0x04, 0xB7, 0xFF, 0x00, 0xFC, 0xB8, 0xCF, 0x12, 0x1D, 0x57, 0x06 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 11);
            Assert.AreEqual(3191, cpu.GetRegister(Reg16.ax));
        }
        [Test]
        public void ExecuteTestAnd8Madrm8()
        {
            var cpu = RunAsmTest("And8Rm8");
            Assert.AreEqual(2, cpu.GetRegister(Reg8.bl));
        }
        [Test]
        public void ExecuteTestAnd16Mrm16()
        {
            var cpu = RunAsmTest("And16Rm16");
            Assert.AreEqual(583, cpu.GetRegister(Reg16.bx));
        }
        [Test]
        public void ExecuteTestAnd8Imm8()
        {
            var cpu = RunAsmTest("And8Imm8");
            Assert.AreEqual(2, cpu.GetRegister(Reg8.al));
        }
        [Test]
        public void ExecuteTestAnd16Imm16()
        {
            var cpu = RunAsmTest("And16Imm16");
            Assert.AreEqual(583, cpu.GetRegister(Reg16.ax));
        }
        [Test]
        public void ExecuteTestSub8Modrm8()
        {
            var cpu = RunAsmTest("Sub8Rm8");
            Assert.AreEqual(4, cpu.GetRegister(Reg8.bl));
        }
        [Test]
        public void ExecuteTestSub16Rm16()
        {
            var cpu = RunAsmTest("Sub16Rm16");
            Assert.AreEqual(1468, cpu.GetRegister(Reg16.bx));
        }
        [Test]
        public void ExecuteTestSub8Imm8()
        {
            var cpu = RunAsmTest("Sub8Imm8");
            Assert.AreEqual(4, cpu.GetRegister(Reg8.al));
        }
        [Test]
        public void ExecuteTestSub16Imm16()
        {
            var cpu = RunAsmTest("Sub16Imm16");
            Assert.AreEqual(3192, cpu.GetRegister(Reg16.ax));
        }
        [Test]
        public void ExecuteTestXor16Rm16()
        {
            var cpu = RunAsmTest("Xor16Rm16");
            Assert.AreEqual(5272, cpu.GetRegister(Reg16.bx));
        }
        [Test]
        public void ExecuteTestXor16Imm16()
        {
            var cpu = RunAsmTest("Xor16Imm16");
            Assert.AreEqual(5272, cpu.GetRegister(Reg16.ax));
        }
        [Test]
        public void ExecuteTestCmp8Rm8False()
        {
            var cpu = RunAsmTest("Cmp8Rm8");
            Assert.AreEqual(false, cpu.CpuFlags.HasFlag(Flags.Zero));
        }
        [Test]
        public void ExecuteTestCmp8Rm8True()
        {
            var cpu = RunAsmTest("Cmp8Rm82");
            Assert.AreEqual(true, cpu.CpuFlags.HasFlag(Flags.Zero));
        }
        [Test]
        public void ExecuteTestCmp16Rm16False()
        {
            var cpu = RunAsmTest("Cmp16Rm16");
            Assert.AreEqual(false, cpu.CpuFlags.HasFlag(Flags.Zero));
        }
        [Test]
        public void ExecuteTestCmp16Rm16True()
        {
            var cpu = RunAsmTest("Cmp16Rm162");
            Assert.AreEqual(true, cpu.CpuFlags.HasFlag(Flags.Zero));
        }
        [Test]
        public void ExecuteTestCmp8Imm8False()
        {
            var cpu = RunAsmTest("Cmp8Imm8");
            Assert.AreEqual(false, cpu.CpuFlags.HasFlag(Flags.Zero));
        }
        [Test]
        public void ExecuteTestCmp8Imm8True()
        {
            var cpu = RunAsmTest("Cmp8Imm82");
            Assert.AreEqual(true, cpu.CpuFlags.HasFlag(Flags.Zero));
        }
        [Test]
        public void ExecuteTestCmp16Imm16False()
        {
            var cpu = RunAsmTest("Cmp16Imm16");
            Assert.AreEqual(false, cpu.CpuFlags.HasFlag(Flags.Zero));
        }
        [Test]
        public void ExecuteTestCmp16Imm16True()
        {
            var cpu = RunAsmTest("Cmp16Imm162");
            Assert.AreEqual(true, cpu.CpuFlags.HasFlag(Flags.Zero));
        }
        [Test]
        public void ExecuteTestInc8()
        {
            var cpu = RunAsmTest("Inc8");
            Assert.AreEqual(5, cpu.GetRegister(Reg8.al));
            Assert.AreEqual(10, cpu.GetRegister(Reg8.bl));
            Assert.AreEqual(18, cpu.GetRegister(Reg8.cl));
            Assert.AreEqual(20, cpu.GetRegister(Reg8.dl));
            Assert.AreEqual(25, cpu.GetRegister(Reg8.ah));
            Assert.AreEqual(46, cpu.GetRegister(Reg8.bh));
            Assert.AreEqual(9, cpu.GetRegister(Reg8.ch));
            Assert.AreEqual(22, cpu.GetRegister(Reg8.dh));

        }
        [Test]
        public void ExecuteTestInc16()
        {
            var cpu = RunAsmTest("Inc16");
            Assert.AreEqual(5, cpu.GetRegister(Reg16.ax));
            Assert.AreEqual(10, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(18, cpu.GetRegister(Reg16.cx));
            Assert.AreEqual(20, cpu.GetRegister(Reg16.dx));
            Assert.AreEqual(25, cpu.GetRegister(Reg16.sp));
            Assert.AreEqual(46, cpu.GetRegister(Reg16.bp));
            Assert.AreEqual(9, cpu.GetRegister(Reg16.si));
            Assert.AreEqual(22, cpu.GetRegister(Reg16.di));

        }
        [Test]
        public void ExecuteTestDec16()
        {
            var cpu = RunAsmTest("Dec16");
            Assert.AreEqual(3, cpu.GetRegister(Reg16.ax));
            Assert.AreEqual(6, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(12, cpu.GetRegister(Reg16.cx));
            Assert.AreEqual(12, cpu.GetRegister(Reg16.dx));
            Assert.AreEqual(21, cpu.GetRegister(Reg16.sp));
            Assert.AreEqual(38, cpu.GetRegister(Reg16.bp));
            Assert.AreEqual(-3, (Int16)cpu.GetRegister(Reg16.si));
            Assert.AreEqual(6, cpu.GetRegister(Reg16.di));

        }
        [Test]
        public void ExecuteTestPushPop16()
        {
            var cpu = RunAsmTest("PushPop16");
            Assert.AreEqual(3, cpu.GetRegister(Reg16.ax));
            Assert.AreEqual(42, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(23, cpu.GetRegister(Reg16.cx));
            Assert.AreEqual(16, cpu.GetRegister(Reg16.dx));
            Assert.AreEqual(15, cpu.GetRegister(Reg16.bp));
            Assert.AreEqual(8, (Int16)cpu.GetRegister(Reg16.si));
            Assert.AreEqual(4, cpu.GetRegister(Reg16.di));

        }
        [Test]
        public void ExecuteTestPushaPopa16()
        {
            var cpu = RunAsmTest("PushaPopa16");
            Assert.AreEqual(4, cpu.GetRegister(Reg16.ax));
            Assert.AreEqual(8, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(15, cpu.GetRegister(Reg16.cx));
            Assert.AreEqual(16, cpu.GetRegister(Reg16.dx));
            Assert.AreEqual(23, cpu.GetRegister(Reg16.si));
            Assert.AreEqual(42, cpu.GetRegister(Reg16.di));
            Assert.AreEqual(3, cpu.GetRegister(Reg16.bp));
        }

        [Test]
        public void ExecuteTestJo()
        {
            var cpu = RunAsmTest("Jo");
            Assert.AreEqual(4, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(15, cpu.GetRegister(Reg16.cx));
        }

        [Test]
        public void ExecuteTestJno()
        {
            var cpu = RunAsmTest("Jno");
            Assert.AreEqual(8, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(15, cpu.GetRegister(Reg16.cx));
        }

        [Test]
        public void ExecuteTestJc()
        {
            var cpu = RunAsmTest("Jc");
            Assert.AreEqual(4, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(15, cpu.GetRegister(Reg16.cx));
        }

        [Test]
        public void ExecuteTestJnc()
        {
            var cpu = RunAsmTest("Jnc");
            Assert.AreEqual(8, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(15, cpu.GetRegister(Reg16.cx));
        }
        [Test]
        public void ExecuteTestJz()
        {
            var cpu = RunAsmTest("Jz");
            Assert.AreEqual(4, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(15, cpu.GetRegister(Reg16.cx));
        }

        [Test]
        public void ExecuteTestJnz()
        {
            var cpu = RunAsmTest("Jnz");
            Assert.AreEqual(8, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(15, cpu.GetRegister(Reg16.cx));
        }

        [Test]
        public void ExecuteTestJbe()
        {
            var cpu = RunAsmTest("Jbe");
            Assert.AreEqual(4, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(15, cpu.GetRegister(Reg16.cx));
        }

        [Test]
        public void ExecuteTestJs()
        {
            var cpu = RunAsmTest("Js");
            Assert.AreEqual(4, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(15, cpu.GetRegister(Reg16.cx));
        }

        [Test]
        public void ExecuteTestJns()
        {
            var cpu = RunAsmTest("Jns");
            Assert.AreEqual(8, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(15, cpu.GetRegister(Reg16.cx));
        }

        [Test]
        public void ExecuteTestJp()
        {
            var cpu = RunAsmTest("Jp");
            Assert.AreEqual(4, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(15, cpu.GetRegister(Reg16.cx));
        }

        [Test]
        public void ExecuteTestJnp()
        {
            var cpu = RunAsmTest("Jnp");
            Assert.AreEqual(8, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(15, cpu.GetRegister(Reg16.cx));
        }

        [Test]
        public void ExecuteTestJl()
        {
            var cpu = RunAsmTest("Jl");
            Assert.AreEqual(4, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(15, cpu.GetRegister(Reg16.cx));
        }

        [Test]
        public void ExecuteTestJnl()
        {
            var cpu = RunAsmTest("Jnl");
            Assert.AreEqual(8, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(15, cpu.GetRegister(Reg16.cx));
        }

        [Test]
        public void ExecuteTestJle()
        {
            var cpu = RunAsmTest("Jle");
            Assert.AreEqual(4, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(15, cpu.GetRegister(Reg16.cx));
        }

        [Test]
        public void ExecuteTestJg()
        {
            var cpu = RunAsmTest("Jg");
            Assert.AreEqual(4, cpu.GetRegister(Reg16.bx));
            Assert.AreEqual(15, cpu.GetRegister(Reg16.cx));
        }

        [Test]
        public void ExecuteTestAdd8Imm8()
        {
            var cpu = RunAsmTest("Add8Imm8");
            Assert.AreEqual(12, cpu.GetRegister(Reg8.ah));
        }

        [Test]
        public void ExecuteTestOrBlImm8()
        {
            var cpu = RunAsmTest("OrBlImm8");
            Assert.AreEqual(0x3f, cpu.GetRegister(Reg8.bl));
        }

        [Test]
        public void ExecuteTestAdcBlImm8()
        {
            var cpu = RunAsmTest("AdcBlImm8");
            Assert.AreEqual(13, cpu.GetRegister(Reg8.bl));
        }

        [Test]
        public void ExecuteTestSbbBlImm8()
        {
            var cpu = RunAsmTest("SbbBlImm8");
            Assert.AreEqual(3, cpu.GetRegister(Reg8.bl));
        }

        [Test]
        public void ExecuteTestAndBlImm8()
        {
            var cpu = RunAsmTest("AndBlImm8");
            Assert.AreEqual(2, cpu.GetRegister(Reg8.bl));
        }

        [Test]
        public void ExecuteTestSubBlImm8()
        {
            var cpu = RunAsmTest("SubBlImm8");
            Assert.AreEqual(4, cpu.GetRegister(Reg8.bl));
        }
        [Test]
        public void ExecuteTestXorBlImm8()
        {
            var cpu = RunAsmTest("XorBlImm8");
            Assert.AreEqual(63, cpu.GetRegister(Reg8.bl));
        }
    }
}