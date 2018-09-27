using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTests98 : CPUUnitTests
    {
        [TestMethod]
        public void cbw()
        {
            ax = 0xFF;
            emit("cbw");
            run();
            Assert.AreEqual(ax, 0xFFFF);
        }

        [TestMethod]
        public void cwd()
        {
            ax = 0xFFFF;
            emit("cwd");
            run();
            Assert.AreEqual(dxax, 0xFFFFFFFF);
        }

        [TestMethod]
        public void call_Ap()
        {
            sp = 0x8008;
            emit("call 01:1234");
            step();
            Assert.AreEqual(sp, 0x8004);
            Assert.AreEqual(ReadWord(ss, sp), 0x105);
            Assert.AreEqual(ReadWord(ss, (ushort)(sp + 2)), 0);
            Assert.AreEqual(cs, 1);
            Assert.AreEqual(ip, 1234);
        }

        [TestMethod]
        public void pushf()
        {
            sp = 0x8008;
            EFlags = 0x1234;
            emit("pushf");
            run();
            Assert.AreEqual(sp, 0x8006);
            Assert.AreEqual(ReadWord(ss, sp), EFlags);
        }

        [TestMethod]
        public void popf()
        {
            sp = 0x8006;
            WriteWord(ss, sp, 0x1234);
            EFlags = 0;
            emit("popf");
            run();
            Assert.AreEqual(sp, 0x8008);
            Assert.AreEqual(EFlags, (0x1234 & EFlag.SupportedBits) | EFlag.FixedBits);
        }

        [TestMethod]
        public void sahf()
        {
            Flags8 = 0;
            ah = 0xFF;
            emit("sahf");
            run();
            Assert.AreEqual(Flags8, (byte)(((0xFF & EFlag.SupportedBits) | EFlag.FixedBits) & 0xFF));
        }

        [TestMethod]
        public void lahf()
        {
            Flags8 = 0xFF;
            ah = 0;
            emit("lahf");
            run();
            Assert.AreEqual(ah, (byte)(((0xFF & EFlag.SupportedBits) | EFlag.FixedBits) & 0xFF));
        }

    }
}
