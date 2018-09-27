using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTests50 : CPUUnitTests
    {
        [TestMethod]
        public void Push_ax()
        {
            ax = 4000;
            sp = 0x8008;
            emit("push ax");
            run();
            Assert.AreEqual(ReadWord(ss, sp), 4000);
            Assert.AreEqual(sp, 0x8006);
        }

        [TestMethod]
        public void Push_cx()
        {
            cx = 4000;
            sp = 0x8008;
            emit("push cx");
            run();
            Assert.AreEqual(ReadWord(ss, sp), 4000);
            Assert.AreEqual(sp, 0x8006);
        }

        [TestMethod]
        public void Push_dx()
        {
            dx = 4000;
            sp = 0x8008;
            emit("push dx");
            run();
            Assert.AreEqual(ReadWord(ss, sp), 4000);
            Assert.AreEqual(sp, 0x8006);
        }

        [TestMethod]
        public void Push_bx()
        {
            bx = 4000;
            sp = 0x8008;
            emit("push bx");
            run();
            Assert.AreEqual(ReadWord(ss, sp), 4000);
            Assert.AreEqual(sp, 0x8006);
        }

        [TestMethod]
        public void Push_sp()
        {
            sp = 4000;
            emit("push sp");
            run();
            Assert.AreEqual(ReadWord(ss, sp), 4000);
            Assert.AreEqual(sp, 3998);
        }

        [TestMethod]
        public void Push_bp()
        {
            bp = 4000;
            sp = 0x8008;
            emit("push bp");
            run();
            Assert.AreEqual(ReadWord(ss, sp), 4000);
            Assert.AreEqual(sp, 0x8006);
        }

        [TestMethod]
        public void Push_si()
        {
            si = 4000;
            sp = 0x8008;
            emit("push si");
            run();
            Assert.AreEqual(ReadWord(ss, sp), 4000);
            Assert.AreEqual(sp, 0x8006);
        }

        [TestMethod]
        public void Push_di()
        {
            di = 4000;
            sp = 0x8008;
            emit("push di");
            run();
            Assert.AreEqual(ReadWord(ss, sp), 4000);
            Assert.AreEqual(sp, 0x8006);
        }

    }
}
