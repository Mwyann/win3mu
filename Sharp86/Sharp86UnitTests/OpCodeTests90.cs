using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTests90 : CPUUnitTests
    {
        [TestMethod]
        public void Xchg_cx_ax()
        {
            ax = 1000;
            cx = 2000;
            emit("xchg ax,cx");
            run();
            Assert.AreEqual(ax, 2000);
            Assert.AreEqual(cx, 1000);
        }

        [TestMethod]
        public void Xchg_dx_ax()
        {
            ax = 1000;
            dx = 2000;
            emit("xchg dx,ax");
            run();
            Assert.AreEqual(ax, 2000);
            Assert.AreEqual(dx, 1000);
        }

        [TestMethod]
        public void Xchg_bx_ax()
        {
            ax = 1000;
            bx = 2000;
            emit("xchg ax,bx");
            run();
            Assert.AreEqual(ax, 2000);
            Assert.AreEqual(bx, 1000);
        }

        [TestMethod]
        public void Xchg_sp_ax()
        {
            ax = 1000;
            sp = 2000;
            emit("xchg ax,sp");
            run();
            Assert.AreEqual(ax, 2000);
            Assert.AreEqual(sp, 1000);
        }

        [TestMethod]
        public void Xchg_bp_ax()
        {
            ax = 1000;
            sp = 2000;
            emit("xchg ax,sp");
            run();
            Assert.AreEqual(ax, 2000);
            Assert.AreEqual(sp, 1000);
        }

        [TestMethod]
        public void Xchg_si_ax()
        {
            ax = 1000;
            si = 2000;
            emit("xchg ax,si");
            run();
            Assert.AreEqual(ax, 2000);
            Assert.AreEqual(si, 1000);
        }

        [TestMethod]
        public void Xchg_di_ax()
        {
            ax = 1000;
            di = 2000;
            emit("xchg ax,di");
            run();
            Assert.AreEqual(ax, 2000);
            Assert.AreEqual(di, 1000);
        }

    }
}
