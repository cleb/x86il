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
    public class RegistersTests
    {
        [TestMethod()]
        public void GetSetAlTest()
        {
            var registers = new Registers();
            registers.Set(Reg8.al, 4);
            Assert.AreEqual(4, registers.Get(Reg8.al));
        }

        [TestMethod()]
        public void GetSetAhTest()
        {
            var registers = new Registers();
            registers.Set(Reg8.ah, 8);
            Assert.AreEqual(8, registers.Get(Reg8.ah));
        }

        [TestMethod()]
        public void GetSetBlBhTest()
        {
            var registers = new Registers();
            registers.Set(Reg8.bl, 15);
            registers.Set(Reg8.bh, 16);
            Assert.AreEqual(15, registers.Get(Reg8.bl));
            Assert.AreEqual(16, registers.Get(Reg8.bh));
        }

        [TestMethod()]
        public void GetSetClChCxTest()
        {
            var registers = new Registers();
            registers.Set(Reg8.ch, 0x23);
            registers.Set(Reg8.cl, 0x42);
            Assert.AreEqual(0x2342, registers.Get(Reg16.cx));
        }
    }
}