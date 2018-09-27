using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTestsB8 : CPUUnitTests
    {
        [TestMethod]
        public void mov_ax_iv()
        {
            emit("mov ax,1234h");
            run();
            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void mov_cx_iv()
        {
            emit("mov cx,1234h");
            run();
            Assert.AreEqual(cx, 0x1234);
        }

        [TestMethod]
        public void mov_dx_iv()
        {
            emit("mov dx,1234h");
            run();
            Assert.AreEqual(dx, 0x1234);
        }

        [TestMethod]
        public void mov_bx_iv()
        {
            emit("mov bx,1234h");
            run();
            Assert.AreEqual(bx, 0x1234);
        }

        [TestMethod]
        public void mov_sp_iv()
        {
            emit("mov sp,1234h");
            run();
            Assert.AreEqual(sp, 0x1234);
        }

        [TestMethod]
        public void mov_bp_iv()
        {
            emit("mov bp,1234h");
            run();
            Assert.AreEqual(bp, 0x1234);
        }

        [TestMethod]
        public void mov_si_iv()
        {
            emit("mov si,1234h");
            run();
            Assert.AreEqual(si, 0x1234);
        }

        [TestMethod]
        public void mov_di_iv()
        {
            emit("mov di,1234h");
            run();
            Assert.AreEqual(di, 0x1234);
        }

    }

}
