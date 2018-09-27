using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTests38 : CPUUnitTests
    {
        [TestMethod]
        public void Cmp_Eb_Gb()
        {
            WriteByte(0, 100, 40);
            FlagC = true;
            FlagZ = true;
            al = 10;
            emit("cmp byte [100], al");
            run();
            Assert.AreEqual(ReadByte(0, 100), 40);
            Assert.IsFalse(FlagZ);
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void Cmp_Ev_Gv()
        {
            WriteWord(0, 100, 4000);
            FlagC = true;
            FlagZ = true;
            ax = 1000;
            emit("cmp word [100], ax");
            run();
            Assert.AreEqual(ReadWord(0, 100), 4000);
            Assert.IsFalse(FlagZ);
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void Cmp_Gb_Eb()
        {
            WriteByte(0, 100, 10);
            al = 40;
            FlagC = true;
            FlagZ = true;
            emit("cmp al, byte [100]");
            run();
            Assert.AreEqual(al, 40);
            Assert.IsFalse(FlagZ);
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void Cmp_Gv_Ev()
        {
            WriteWord(0, 100, 1000);
            ax = 4000;
            FlagC = true;
            FlagZ = true;
            emit("cmp ax, word [100]");
            run();
            Assert.AreEqual(ax, 4000);
            Assert.IsFalse(FlagZ);
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void Cmp_AL_Ib()
        {
            al = 40;
            FlagC = true;
            FlagZ = true;
            emit("cmp al, 10");
            run();
            Assert.AreEqual(al, 40);
            Assert.IsFalse(FlagZ);
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void Cmp_AX_Iv()
        {
            ax = 4000;
            FlagC = true;
            FlagZ = true;
            emit("cmp ax, 1000");
            run();
            Assert.AreEqual(ax, 4000);
            Assert.IsFalse(FlagZ);
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void Aas()
        {
            al = 0xCC;
            emit("aas");
            run();
            Assert.AreEqual(al, 0);
        }


    }
}
