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
            var filePath = TestContext.CurrentContext.TestDirectory + $"../../../TestAsm/{filename}";
            System.Diagnostics.Process.Start("nasm", $"{filePath}.asm -o {filePath}.o");
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
    }
}