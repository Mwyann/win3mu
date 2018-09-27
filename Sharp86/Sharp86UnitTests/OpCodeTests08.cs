using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTests08 : CPUUnitTests
    {
        [TestMethod]
        public void Or_Eb_Gb()
        {
            WriteByte(0, 100, 0x41);
            al = 0x21;
            emit("or byte [100], al");
            run();
            Assert.AreEqual(ReadByte(0, 100), 0x61);
        }

        [TestMethod]
        public void Or_Ev_Gv()
        {
            WriteWord(0, 100, 0x4001);
            ax = 0x2001;
            emit("or word [100], ax");
            run();
            Assert.AreEqual(ReadWord(0, 100), 0x6001);
        }

        [TestMethod]
        public void Or_Gb_Eb()
        {
            WriteByte(0, 100, 0x41);
            al = 0x21;
            emit("or al, byte [100]");
            run();
            Assert.AreEqual(al, 0x61);
        }

        [TestMethod]
        public void Or_Gv_Ev()
        {
            WriteWord(0, 100, 0x4001);
            ax = 0x2001;
            emit("or ax, word [100]");
            run();
            Assert.AreEqual(ax, 0x6001);
        }

        [TestMethod]
        public void Or_AL_Ib()
        {
            al = 0x41;
            emit("or al, 21h");
            run();
            Assert.AreEqual(al, 0x61);
        }

        [TestMethod]
        public void Or_AX_Iv()
        {
            ax = 0x4001;
            emit("or ax, 0x2001");
            run();
            Assert.AreEqual(ax, 0x6001);
        }

        [TestMethod]
        public void PUSH_CS()
        {
            WriteWord(ss, (ushort)(sp - 2), 0x1234);
            sp = 0x8008;
            emit("push cs");
            run();
            Assert.AreEqual(ReadWord(ss, sp), 0);
            Assert.AreEqual(sp, 0x8006);
        }

    }
}
