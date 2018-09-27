using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTests10 : CPUUnitTests
    {
        [TestMethod]
        public void Adc_Eb_Gb()
        {
            WriteByte(0, 100, 40);
            FlagC = true;
            al = 20;
            emit("adc byte [100], al");
            run();
            Assert.AreEqual(ReadByte(0, 100), 61);
        }

        [TestMethod]
        public void Adc_Ev_Gv()
        {
            WriteWord(0, 100, 4000);
            FlagC = true;
            ax = 2000;
            emit("adc word [100], ax");
            run();
            Assert.AreEqual(ReadWord(0, 100), 6001);
        }

        [TestMethod]
        public void Adc_Gb_Eb()
        {
            WriteByte(0, 100, 40);
            al = 20;
            FlagC = true;
            emit("adc al, byte [100]");
            run();
            Assert.AreEqual(al, 61);
        }

        [TestMethod]
        public void Adc_Gv_Ev()
        {
            WriteWord(0, 100, 4000);
            ax = 2000;
            FlagC = true;
            emit("adc ax, word [100]");
            run();
            Assert.AreEqual(ax, 6001);
        }

        [TestMethod]
        public void Adc_AL_Ib()
        {
            al = 40;
            FlagC = true;
            emit("adc al, 20");
            run();
            Assert.AreEqual(al, 61);
        }

        [TestMethod]
        public void Adc_AX_Iv()
        {
            ax = 4000;
            FlagC = true;
            emit("adc ax, 2000");
            run();
            Assert.AreEqual(ax, 6001);
        }

        [TestMethod]
        public void PUSH_SS()
        {
            sp = 0x8008;
            ss = 0x0001;
            emit("push ss");
            run();
            Assert.AreEqual(ReadWord(ss, sp), 0x0001);
            Assert.AreEqual(sp, 0x8006);
        }

        [TestMethod]
        public void POP_SS()
        {
            sp = 0x8006;
            WriteWord(ss, sp, 0x1234);
            emit("pop ss");
            run();
            Assert.AreEqual(ss, 0x1234);
            Assert.AreEqual(sp, 0x8008);
        }

    }
}
