using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTests28 : CPUUnitTests
    {
        [TestMethod]
        public void Sub_Eb_Gb()
        {
            WriteByte(0, 100, 40);
            FlagC = true;
            al = 10;
            emit("sub byte [100], al");
            run();
            Assert.AreEqual(ReadByte(0, 100), 30);
        }

        [TestMethod]
        public void Sub_Ev_Gv()
        {
            WriteWord(0, 100, 4000);
            FlagC = true;
            ax = 1000;
            emit("sub word [100], ax");
            run();
            Assert.AreEqual(ReadWord(0, 100), 3000);
        }

        [TestMethod]
        public void Sub_Gb_Eb()
        {
            WriteByte(0, 100, 10);
            al = 40;
            FlagC = true;
            emit("sub al, byte [100]");
            run();
            Assert.AreEqual(al, 30);
        }

        [TestMethod]
        public void Sub_Gv_Ev()
        {
            WriteWord(0, 100, 1000);
            ax = 4000;
            FlagC = true;
            emit("sub ax, word [100]");
            run();
            Assert.AreEqual(ax, 3000);
        }

        [TestMethod]
        public void Sub_AL_Ib()
        {
            al = 40;
            FlagC = true;
            emit("sub al, 10");
            run();
            Assert.AreEqual(al, 30);
        }

        [TestMethod]
        public void Sub_AX_Iv()
        {
            ax = 4000;
            FlagC = true;
            emit("sub ax, 1000");
            run();
            Assert.AreEqual(ax, 3000);
        }

        [TestMethod]
        public void DAS()
        {
            al = 0xCC;
            emit("das");
            run();
            Assert.AreEqual(al, 0x66);
        }


    }
}
