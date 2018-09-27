using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTests40 : CPUUnitTests
    {
        [TestMethod]
        public void Inc_ax()
        {
            ax = 4000;
            emit("inc ax");
            run();
            Assert.AreEqual(ax, 4001);
        }

        [TestMethod]
        public void Inc_cx()
        {
            cx = 4000;
            emit("inc cx");
            run();
            Assert.AreEqual(cx, 4001);
        }

        [TestMethod]
        public void Inc_dx()
        {
            dx = 4000;
            emit("inc dx");
            run();
            Assert.AreEqual(dx, 4001);
        }

        [TestMethod]
        public void Inc_bx()
        {
            bx = 4000;
            emit("inc bx");
            run();
            Assert.AreEqual(bx, 4001);
        }

        [TestMethod]
        public void Inc_sp()
        {
            sp = 4000;
            emit("inc sp");
            run();
            Assert.AreEqual(sp, 4001);
        }

        [TestMethod]
        public void Inc_bp()
        {
            bp = 4000;
            emit("inc bp");
            run();
            Assert.AreEqual(bp, 4001);
        }

        [TestMethod]
        public void Inc_si()
        {
            si = 4000;
            emit("inc si");
            run();
            Assert.AreEqual(si, 4001);
        }

        [TestMethod]
        public void Inc_di()
        {
            di = 4000;
            emit("inc di");
            run();
            Assert.AreEqual(di, 4001);
        }

    }
}
