using NUnit.Framework;

namespace x86il.Tests
{
    [TestFixture]
    public class RegistersTests
    {
        [Test]
        public void GetSetAlTest()
        {
            var registers = new Registers();
            registers.Set(Reg8.al, 4);
            Assert.AreEqual(4, registers.Get(Reg8.al));
        }

        [Test]
        public void GetSetAhTest()
        {
            var registers = new Registers();
            registers.Set(Reg8.ah, 8);
            Assert.AreEqual(8, registers.Get(Reg8.ah));
        }

        [Test]
        public void GetSetBlBhTest()
        {
            var registers = new Registers();
            registers.Set(Reg8.bl, 15);
            registers.Set(Reg8.bh, 16);
            Assert.AreEqual(15, registers.Get(Reg8.bl));
            Assert.AreEqual(16, registers.Get(Reg8.bh));
        }

        [Test]
        public void GetSetClChCxTest()
        {
            var registers = new Registers();
            registers.Set(Reg8.ch, 0x23);
            registers.Set(Reg8.cl, 0x42);
            Assert.AreEqual(0x2342, registers.Get(Reg16.cx));
        }
    }
}