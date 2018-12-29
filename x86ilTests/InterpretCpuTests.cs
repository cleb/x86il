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
    }
}