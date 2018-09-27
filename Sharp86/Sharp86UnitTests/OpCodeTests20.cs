using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTests20 : CPUUnitTests
    {
        [TestMethod]
        public void And_Eb_Gb()
        {
            WriteByte(0, 100, 0x60);
            al = 0x20;
            emit("and byte [100], al");
            run();
            Assert.AreEqual(ReadByte(0, 100), 0x20);
        }

        [TestMethod]
        public void And_Ev_Gv()
        {
            WriteWord(0, 100, 0x6000);
            ax = 0x2000;
            emit("and word [100], ax");
            run();
            Assert.AreEqual(ReadWord(0, 100), 0x2000);
        }

        [TestMethod]
        public void And_Gb_Eb()
        {
            WriteByte(0, 100, 0x60);
            al = 0x20;
            emit("and al, byte [100]");
            run();
            Assert.AreEqual(al, 0x20);
        }

        [TestMethod]
        public void And_Gv_Ev()
        {
            WriteWord(0, 100, 0x6000);
            ax = 0x2000;
            emit("and ax, word [100]");
            run();
            Assert.AreEqual(ax, 0x2000);
        }

        [TestMethod]
        public void And_AL_Ib()
        {
            al = 0x60;
            emit("and al, 0x20");
            run();
            Assert.AreEqual(al, 0x20);
        }

        [TestMethod]
        public void And_AX_Iv()
        {
            ax = 0x6000;
            emit("and ax, 0x2000");
            run();
            Assert.AreEqual(ax, 0x2000);
        }

        [TestMethod]
        public void DAA()
        {
            al = 0xCC;
            emit("daa");
            run();
            Assert.AreEqual(al, 0x32);
        }

    }
}
