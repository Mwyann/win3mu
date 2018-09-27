using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTests58 : CPUUnitTests
    {
        [TestMethod]
        public void Pop_ax()
        {
            sp = 0x8006;
            WriteWord(ss, sp, 4000);
            emit("pop ax");
            run();
            Assert.AreEqual(ax, 4000);
            Assert.AreEqual(sp, 0x8008);
        }

        [TestMethod]
        public void Pop_cx()
        {
            sp = 0x8006;
            WriteWord(ss, sp, 4000);
            emit("pop cx");
            run();
            Assert.AreEqual(cx, 4000);
            Assert.AreEqual(sp, 0x8008);
        }

        [TestMethod]
        public void Pop_dx()
        {
            sp = 0x8006;
            WriteWord(ss, sp, 4000);
            emit("pop dx");
            run();
            Assert.AreEqual(dx, 4000);
            Assert.AreEqual(sp, 0x8008);
        }

        [TestMethod]
        public void Pop_bx()
        {
            sp = 0x8006;
            WriteWord(ss, sp, 4000);
            emit("pop bx");
            run();
            Assert.AreEqual(bx, 4000);
            Assert.AreEqual(sp, 0x8008);
        }

        [TestMethod]
        public void Pop_sp()
        {
            sp = 0x8006;
            WriteWord(ss, sp, 4000);
            emit("pop sp");
            run();
            Assert.AreEqual(sp, 4000);
        }

        [TestMethod]
        public void Pop_bp()
        {
            sp = 0x8006;
            WriteWord(ss, sp, 4000);
            emit("pop bp");
            run();
            Assert.AreEqual(bp, 4000);
            Assert.AreEqual(sp, 0x8008);
        }

        [TestMethod]
        public void Pop_si()
        {
            sp = 0x8006;
            WriteWord(ss, sp, 4000);
            emit("pop si");
            run();
            Assert.AreEqual(si, 4000);
            Assert.AreEqual(sp, 0x8008);
        }

        [TestMethod]
        public void Pop_di()
        {
            sp = 0x8006;
            WriteWord(ss, sp, 4000);
            emit("pop di");
            run();
            Assert.AreEqual(di, 4000);
            Assert.AreEqual(sp, 0x8008);
        }

    }
}
