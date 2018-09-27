using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTests00 : CPUUnitTests
    {
        [TestMethod]
        public void Add_Eb_Gb()
        {
            WriteByte(0, 100, 40);
            al = 20;
            emit("add byte [100], al");
            run();
            Assert.AreEqual(ReadByte(0, 100), 60);
        }

        [TestMethod]
        public void Add_Ev_Gv()
        {
            WriteWord(0, 100, 4000);
            ax = 2000;
            emit("add word [100], ax");
            run();
            Assert.AreEqual(this.ReadWord(0, 100), 6000);
        }

        [TestMethod]
        public void Add_Gb_Eb()
        {
            WriteByte(0, 100, 40);
            al = 20;
            emit("add al, byte [100]");
            run();
            Assert.AreEqual(al, 60);
        }

        [TestMethod]
        public void Add_Gv_Ev()
        {
            WriteWord(0, 100, 4000);
            ax = 2000;
            emit("add ax, word [100]");
            run();
            Assert.AreEqual(ax, 6000);
        }

        [TestMethod]
        public void Add_AL_Ib()
        {
            al = 40;
            emit("add al, 20");
            run();
            Assert.AreEqual(al, 60);
        }

        [TestMethod]
        public void Add_AX_Iv()
        {
            ax = 4000;
            emit("add ax, 2000");
            run();
            Assert.AreEqual(ax, 6000);
        }

        [TestMethod]
        public void PUSH_ES()
        {
            sp = 0x8008;
            es = 0x1234;
            emit("push es");
            run();
            Assert.AreEqual(ReadWord(ss, sp), 0x1234);
            Assert.AreEqual(sp, 0x8006);
        }

        [TestMethod]
        public void POP_ES()
        {
            sp = 0x8006;
            WriteWord(ss, sp, 0x1234);
            emit("pop es");
            run();
            Assert.AreEqual(es, 0x1234);
            Assert.AreEqual(sp, 0x8008);
        }

    }
}
