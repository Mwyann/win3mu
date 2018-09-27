using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTests48 : CPUUnitTests
    {
        [TestMethod]
        public void Dec_ax()
        {
            ax = 4000;
            emit("dec ax");
            run();
            Assert.AreEqual(ax, 3999);
        }

        [TestMethod]
        public void Dec_cx()
        {
            cx = 4000;
            emit("dec cx");
            run();
            Assert.AreEqual(cx, 3999);
        }

        [TestMethod]
        public void Dec_dx()
        {
            dx = 4000;
            emit("dec dx");
            run();
            Assert.AreEqual(dx, 3999);
        }

        [TestMethod]
        public void Dec_bx()
        {
            bx = 4000;
            emit("dec bx");
            run();
            Assert.AreEqual(bx, 3999);
        }

        [TestMethod]
        public void Dec_sp()
        {
            sp = 4000;
            emit("dec sp");
            run();
            Assert.AreEqual(sp, 3999);
        }

        [TestMethod]
        public void Dec_bp()
        {
            bp = 4000;
            emit("dec bp");
            run();
            Assert.AreEqual(bp, 3999);
        }

        [TestMethod]
        public void Dec_si()
        {
            si = 4000;
            emit("dec si");
            run();
            Assert.AreEqual(si, 3999);
        }

        [TestMethod]
        public void Dec_di()
        {
            di = 4000;
            emit("dec di");
            run();
            Assert.AreEqual(di, 3999);
        }

    }
}
