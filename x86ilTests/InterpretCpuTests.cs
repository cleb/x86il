using Microsoft.VisualStudio.TestTools.UnitTesting;
using x86il;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x86il.Tests
{
    [TestClass()]
    public class InterpretCpuTests
    {
        [TestMethod()]
        public void ExecuteTest()
        {
            var memory = new Byte[] { 0xb4, 0x04 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0,1);
            Assert.AreEqual(4, cpu.GetRegister(Reg8.ah));
        }

        [TestMethod()]
        public void ExecuteTestXor()
        {
            var memory = new Byte[] { 0xb0, 0x04, 0x30, 0xc0 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 1);
            Assert.AreEqual(4, cpu.GetRegister(Reg8.al));
            cpu.Execute(2, 3);
            Assert.AreEqual(0, cpu.GetRegister(Reg8.al));
        }
        [TestMethod()]
        public void ExecuteTestMov16()
        {
            var memory = new Byte[] { 0xbb, 0x42, 0x00 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 2);
            Assert.AreEqual(0x42, cpu.GetRegister(Reg16.bx));
        }
        [TestMethod()]
        public void ExecuteTestMovEsAx()
        {
            var memory = new Byte[] { 0xb8, 0x42, 0x00, 0x8e, 0xc0 };
            var cpu = new InterpretCpu(memory);
            cpu.Execute(0, 5);
            Assert.AreEqual(0x42, cpu.GetRegister(Segments.es));
        }
    }
}