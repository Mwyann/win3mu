using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTests68 : CPUUnitTests
    {
        [TestMethod]
        public void push_Ib()
        {
            sp = 0x1000;
            emit("push -1");
            step();

            Assert.AreEqual(sp, 0x0FFE);
            Assert.AreEqual(ReadWord(ss, sp), 0xFFFF);  // byte will be sign extended
        }

        [TestMethod]
        public void push_Iv()
        {
            sp = 0x1000;
            emit("push 01ffh");
            step();

            Assert.AreEqual(sp, 0x0FFE);
            Assert.AreEqual(ReadWord(ss, sp), 0x01ff);
        }

        [TestMethod]
        public void imul_Gv_Ev_Ib()
        {
            ax = 0;
            bx = 10;
            emit("imul ax,bx,20");
            step();
            Assert.AreEqual(ax, 200);

            bx = 0xFFFF;
            emit("imul ax,bx,10");
            step();
            Assert.AreEqual(ax, 0xFFF6);
        }

        [TestMethod]
        public void imul_Gv_Ev_Iv()
        {
            ax = 0;
            bx = 10;
            emit("imul ax,bx,200");
            step();
            Assert.AreEqual(ax, 2000);

            bx = 0xFFFF;
            emit("imul ax,bx,1000");
            step();
            Assert.AreEqual(ax, unchecked((ushort)(short)-1000));
        }


        [TestMethod]
        public void insb()
        {
            EnqueueReadPortByte(0x5678, 0x12);

            di = 0x1000;
            dx = 0x5678;
            emit("insb");
            step();

            Assert.IsTrue(WasPortAccessed(0x5678));
            Assert.AreEqual(di, 0x1001);
            Assert.AreEqual(ReadByte(ds, 0x1000), 0x12);
        }

        [TestMethod]
        public void rep_insb()
        {
            for (int i=0; i<5; i++)
            {
                EnqueueReadPortByte(0x5678, (byte)(0x12 + i));
            }

            di = 0x1000;
            dx = 0x5678;
            cx = 5;
            emit("rep insb");
            step();

            Assert.IsTrue(WasPortAccessed(0x5678));
            Assert.AreEqual(di, 0x1005);
            Assert.AreEqual(cx, 0);
            for (int i=0; i<5; i++)
            {
                Assert.AreEqual(ReadByte(ds, (ushort)(0x1000 + i)), (byte)(0x12 + i));
            }
        }

        [TestMethod]
        public void rep_insb_cx0()
        {
            di = 0x1000;
            dx = 0x5678;
            cx = 0;
            emit("rep insb");
            step();

            Assert.IsFalse(WasPortAccessed(0x5678));
            Assert.AreEqual(di, 0x1000);
            Assert.AreEqual(cx, 0);
        }

        [TestMethod]
        public void insw()
        {
            EnqueueReadPortByte(0x5678, 0x34);
            EnqueueReadPortByte(0x5679, 0x12);

            di = 0x1000;
            dx = 0x5678;
            emit("insw");
            step();

            Assert.IsTrue(WasPortAccessed(0x5678));
            Assert.IsTrue(WasPortAccessed(0x5679));
            Assert.AreEqual(di, 0x1002);
            Assert.AreEqual(ReadWord(ds, 0x1000), 0x1234);
        }

        [TestMethod]
        public void rep_insw()
        {
            for (int i=0; i<5; i++)
            {
                EnqueueReadPortByte(0x5678, (byte)(0x34 + i));
                EnqueueReadPortByte(0x5679, 0x12);
            }

            di = 0x1000;
            dx = 0x5678;
            cx = 5;
            emit("rep insw");
            step();

            Assert.IsTrue(WasPortAccessed(0x5678));
            Assert.IsTrue(WasPortAccessed(0x5679));
            Assert.AreEqual(di, 0x100A);
            Assert.AreEqual(cx, 0);
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(ReadWord(ds, (ushort)(0x1000 + i * 2)), (ushort)(0x1234 + i));
            }
        }

        [TestMethod]
        public void rep_insw_cx0()
        {
            di = 0x1000;
            dx = 0x5678;
            cx = 0;
            emit("rep insw");
            step();

            Assert.IsFalse(WasPortAccessed(0x5678));
            Assert.IsFalse(WasPortAccessed(0x5679));
            Assert.AreEqual(di, 0x1000);
            Assert.AreEqual(cx, 0);
        }

        [TestMethod]
        public void outsb()
        {
            si = 0x1000;
            dx = 0x5678;
            WriteByte(ds, si, 0x12);
            emit("outsb");
            step();

            Assert.IsTrue(WasPortAccessed(0x5678));
            Assert.AreEqual(si, 0x1001);
            Assert.AreEqual(DequeueWrittenPortByte(0x5678), 0x12);
        }

        [TestMethod]
        public void rep_outsb()
        {
            si = 0x1000;
            dx = 0x5678;
            cx = 5;
            for (int i = 0; i < 5; i++)
            {
                WriteByte(ds, (ushort)(si + i), (byte)(0x12 + i));
            }
            emit("rep outsb");
            step();

            Assert.IsTrue(WasPortAccessed(0x5678));
            Assert.AreEqual(si, 0x1005);
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(DequeueWrittenPortByte(0x5678), (byte)(0x12 + i));
            }
        }

        [TestMethod]
        public void rep_outsb_cx0()
        {
            si = 0x1000;
            dx = 0x5678;
            cx = 0;
            emit("rep outsb");
            step();

            Assert.IsFalse(WasPortAccessed(0x5678));
            Assert.AreEqual(si, 0x1000);
        }

        [TestMethod]
        public void outsw()
        {
            si = 0x1000;
            dx = 0x5678;
            WriteWord(ds, si, 0x1234);
            emit("outsw");
            step();

            Assert.IsTrue(WasPortAccessed(0x5678));
            Assert.IsTrue(WasPortAccessed(0x5679));
            Assert.AreEqual(si, 0x1002);
            Assert.AreEqual(DequeueWrittenPortByte(0x5678), 0x34);
            Assert.AreEqual(DequeueWrittenPortByte(0x5679), 0x12);
        }

        [TestMethod]
        public void rep_outsw()
        {
            si = 0x1000;
            dx = 0x5678;
            cx = 5;
            for (int i = 0; i < 5; i++)
            {
                WriteWord(ds, (ushort)(si + i * 2), (ushort)(0x1234 + i));
            }
            emit("rep outsw");
            step();

            Assert.IsTrue(WasPortAccessed(0x5678));
            Assert.AreEqual(si, 0x100A);
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(DequeueWrittenPortByte(0x5678), (byte)(0x34 + i));
                Assert.AreEqual(DequeueWrittenPortByte(0x5679), 0x12);
            }
        }

        [TestMethod]
        public void rep_outsw_cx0()
        {
            si = 0x1000;
            dx = 0x5678;
            cx = 0;
            emit("rep outsw");
            step();

            Assert.IsFalse(WasPortAccessed(0x5678));
            Assert.AreEqual(si, 0x1000);
        }

    }
}
