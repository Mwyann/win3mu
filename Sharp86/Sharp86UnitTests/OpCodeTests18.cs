using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTests18 : CPUUnitTests
    {
        [TestMethod]
        public void Sbb_Eb_Gb()
        {
            WriteByte(0, 100, 40);
            FlagC = true;
            al = 10;
            emit("sbb byte [100], al");
            run();
            Assert.AreEqual(ReadByte(0, 100), 29);
        }

        [TestMethod]
        public void Sbb_Ev_Gv()
        {
            WriteWord(0, 100, 4000);
            FlagC = true;
            ax = 1000;
            emit("sbb word [100], ax");
            run();
            Assert.AreEqual(ReadWord(0, 100), 2999);
        }

        [TestMethod]
        public void Sbb_Gb_Eb()
        {
            WriteByte(0, 100, 10);
            al = 40;
            FlagC = true;
            emit("sbb al, byte [100]");
            run();
            Assert.AreEqual(al, 29);
        }

        [TestMethod]
        public void Sbb_Gv_Ev()
        {
            WriteWord(0, 100, 1000);
            ax = 4000;
            FlagC = true;
            emit("sbb ax, word [100]");
            run();
            Assert.AreEqual(ax, 2999);
        }

        [TestMethod]
        public void Sbb_AL_Ib()
        {
            al = 40;
            FlagC = true;
            emit("sbb al, 10");
            run();
            Assert.AreEqual(al, 29);
        }

        [TestMethod]
        public void Sbb_AX_Iv()
        {
            ax = 4000;
            FlagC = true;
            emit("sbb ax, 1000");
            run();
            Assert.AreEqual(ax, 2999);
        }

        [TestMethod]
        public void PUSH_DS()
        {
            sp = 0x8008;
            ds = 0x1234;
            emit("push ds");
            run();
            Assert.AreEqual(ReadWord(ss, sp), 0x1234);
            Assert.AreEqual(sp, 0x8006);
        }

        [TestMethod]
        public void POP_DS()
        {
            sp = 0x8006;
            WriteWord(ss, sp, 0x1234);
            emit("pop ds");
            run();
            Assert.AreEqual(ds, 0x1234);
            Assert.AreEqual(sp, 0x8008);
        }

    }
}
