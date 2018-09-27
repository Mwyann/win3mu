using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTestsE0 : CPUUnitTests
    {
        [TestMethod]
        public void loopnz_1()
        {
            cx = 1;
            emit("label1:");
            emit("nop");
            emit("nop");
            emit("loopnz label1");
            step();
            step();
            step();

            Assert.AreEqual(ip, 0x104);
            Assert.AreEqual(cx, 0);
        }

        [TestMethod]
        public void loopnz_2()
        {
            cx = 0;
            emit("label1:");
            emit("nop");
            emit("nop");
            emit("loopnz label1");
            step();
            step();
            step();

            Assert.AreEqual(ip, 0x100);
            Assert.AreEqual(cx, 0xFFFF);
        }

        [TestMethod]
        public void loopnz_3()
        {
            cx = 1;
            FlagZ = true;
            emit("label1:");
            emit("nop");
            emit("nop");
            emit("loopnz label1");
            step();
            step();
            step();

            Assert.AreEqual(ip, 0x104);
            Assert.AreEqual(cx, 0);
        }



        [TestMethod]
        public void loopz_1()
        {
            FlagZ = true;
            cx = 1;
            emit("label1:");
            emit("nop");
            emit("nop");
            emit("loopz label1");
            step();
            step();
            step();

            Assert.AreEqual(ip, 0x104);
            Assert.AreEqual(cx, 0);
        }

        [TestMethod]
        public void loopz_2()
        {
            FlagZ = true;
            cx = 0;
            emit("label1:");
            emit("nop");
            emit("nop");
            emit("loopz label1");
            step();
            step();
            step();

            Assert.AreEqual(ip, 0x100);
            Assert.AreEqual(cx, 0xFFFF);
        }

        [TestMethod]
        public void loopz_3()
        {
            cx = 1;
            FlagZ = false;
            emit("label1:");
            emit("nop");
            emit("nop");
            emit("loopz label1");
            step();
            step();
            step();

            Assert.AreEqual(ip, 0x104);
            Assert.AreEqual(cx, 0);
        }


        [TestMethod]
        public void loop_1()
        {
            FlagZ = true;
            cx = 1;
            emit("label1:");
            emit("nop");
            emit("nop");
            emit("loop label1");
            step();
            step();
            step();

            Assert.AreEqual(ip, 0x104);
            Assert.AreEqual(cx, 0);
        }

        [TestMethod]
        public void loop_2()
        {
            FlagZ = true;
            cx = 1;
            emit("label1:");
            emit("nop");
            emit("nop");
            emit("loop label1");
            step();
            step();
            step();

            Assert.AreEqual(ip, 0x104);
            Assert.AreEqual(cx, 0);
        }

        [TestMethod]
        public void jcxz_1()
        {
            emit("mov cx,0");
            emit("jcxz $+0x10");
            step();
            step();

            Assert.AreEqual(ip, 0x113);
        }

        [TestMethod]
        public void jcxz_2()
        {
            emit("mov cx,1");
            emit("jcxz $+0x10");
            step();
            step();

            Assert.AreEqual(ip, 0x105);
        }

        [TestMethod]
        public void in_al_dx()
        {
            EnqueueReadPortByte(0x1234, 0x78);

            ax = 0;
            dx = 0x1234;
            emit("in al,dx");
            step();

            Assert.AreEqual(ax, 0x78);
            Assert.IsTrue(WasPortAccessed(0x1234));
        }

        [TestMethod]
        public void in_ax_dx()
        {
            EnqueueReadPortByte(0x1234, 0x78);
            EnqueueReadPortByte(0x1235, 0x56);

            dx = 0x1234;
            emit("in ax,dx");
            step();

            Assert.AreEqual(ax, 0x5678);
            Assert.IsTrue(WasPortAccessed(0x1234));
            Assert.IsTrue(WasPortAccessed(0x1235));
        }

        [TestMethod]
        public void out_dx_al()
        {
            ax = 0x5678;
            dx = 0x1234;

            emit("out dx,al");
            step();

            Assert.AreEqual(DequeueWrittenPortByte(0x1234), 0x78);
            Assert.IsTrue(WasPortAccessed(0x1234));
            Assert.IsFalse(WasPortAccessed(0x1235));
        }

        [TestMethod]
        public void out_dx_ax()
        {
            ax = 0x5678;
            dx = 0x1234;

            emit("out dx,ax");
            step();

            Assert.AreEqual(DequeueWrittenPortByte(0x1234), 0x78);
            Assert.AreEqual(DequeueWrittenPortByte(0x1235), 0x56);
            Assert.IsTrue(WasPortAccessed(0x1234));
            Assert.IsTrue(WasPortAccessed(0x1235));
        }
    }
}
