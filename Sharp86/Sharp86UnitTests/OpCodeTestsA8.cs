using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTestsA8 : CPUUnitTests
    {
        [TestMethod]
        public void test_al_Ib()
        {
            al = 0x30;
            emit("test al,0x10");
            run();
            Assert.AreEqual(al, 0x30);
            Assert.IsFalse(FlagZ);
        }

        [TestMethod]
        public void test_ax_Iv()
        {
            ax = 0x3000;
            emit("test ax,0x1000");
            run();
            Assert.AreEqual(ax, 0x3000);
            Assert.IsFalse(FlagZ);
        }

        [TestMethod]
        public void stosb()
        {
            es = 0x0020;
            di = 0x200;
            al = 0x12;
            emit("stosb");
            run();
            Assert.AreEqual(ReadByte(es, (ushort)(di - 1)), 0x12);
            Assert.AreEqual(di, 0x201);
        }

        [TestMethod]
        public void rep_stosb()
        {
            es = 0x0020;
            di = 0x200;
            al = 0x12;
            cx = 16;

            emit("rep stosb");
            run();

            Assert.AreEqual(di, 0x210);
            Assert.AreEqual(cx, 0);
            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(ReadByte(es, (ushort)(di - 16 + i)), al);
            }
        }

        [TestMethod]
        public void rep_stosb_cx0()
        {
            es = 0xFFFF;
            di = 0x200;
            al = 0x12;
            cx = 0;

            emit("rep stosb");
            run();

            Assert.AreEqual(di, 0x200);
            Assert.AreEqual(cx, 0);
        }


        [TestMethod]
        public void stosw()
        {
            es = 0x0020;
            di = 0x200;
            ax = 0x1234;
            emit("stosw");
            run();
            Assert.AreEqual(ReadWord(es, (ushort)(di - 2)), 0x1234);
            Assert.AreEqual(di, 0x202);
        }

        [TestMethod]
        public void rep_stosw()
        {
            es = 0x0020;
            di = 0x200;
            ax = 0x1234;
            cx = 16;

            emit("rep stosw");
            run();

            Assert.AreEqual(di, 0x220);
            Assert.AreEqual(cx, 0);
            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(ReadWord(es, (ushort)(di - 32 + i*2)), ax);
            }
        }

        [TestMethod]
        public void rep_stosw_cx0()
        {
            es = 0xFFFF;
            di = 0x200;
            ax = 0x1234;
            cx = 0;

            emit("rep stosw");
            run();

            Assert.AreEqual(di, 0x200);
            Assert.AreEqual(cx, 0);
        }

        [TestMethod]
        public void lodsb()
        {
            ds = 0x0020;
            si = 0x200;
            WriteByte(ds, si, 0x12);
            emit("lodsb");
            run();
            Assert.AreEqual(al, 0x12);
            Assert.AreEqual(si, 0x201);
        }

        [TestMethod]
        public void rep_lodsb()
        {
            ds = 0x0020;
            si = 0x200;
            cx = 16;

            for (int i=0; i<16; i++)
            {
                WriteByte(ds, (ushort)(si + i), (byte)(0x10 + i));
            }

            emit("rep lodsb");
            run();

            Assert.AreEqual(si, 0x210);
            Assert.AreEqual(cx, 0);
            Assert.AreEqual(al, 0x1f);
        }

        [TestMethod]
        public void rep_lodsb_cx0()
        {
            ds = 0xFFFF;
            si = 0x200;
            cx = 0;

            emit("rep lodsb");
            run();

            Assert.AreEqual(si, 0x200);
            Assert.AreEqual(cx, 0);
        }

        [TestMethod]
        public void lodsw()
        {
            ds = 0x0020;
            si = 0x200;
            WriteWord(ds, si, 0x1234);
            emit("lodsw");
            run();
            Assert.AreEqual(ax, 0x1234);
            Assert.AreEqual(si, 0x202);
        }

        [TestMethod]
        public void rep_lodsw()
        {
            ds = 0x0020;
            si = 0x200;
            cx = 16;

            for (int i = 0; i < 16; i++)
            {
                WriteWord(ds, (ushort)(si + i * 2), (ushort)(0x1234 + i));
            }

            emit("rep lodsw");
            run();

            Assert.AreEqual(si, 0x220);
            Assert.AreEqual(cx, 0);
            Assert.AreEqual(ax, 0x1234 + 15);
        }

        [TestMethod]
        public void rep_lodsw_cx0()
        {
            ds = 0xFFFF;
            si = 0x200;
            cx = 0;

            emit("rep lodsw");
            run();

            Assert.AreEqual(si, 0x200);
            Assert.AreEqual(cx, 0);
        }


        [TestMethod]
        public void scasb()
        {
            es = 0x0020;
            di = 0x200;
            WriteByte(es, di, 0x12);
            al = 0x11;
            emit("scasb");
            run();
            Assert.AreEqual(al, 0x11);
            Assert.AreEqual(di, 0x201);
            Assert.IsTrue(FlagC);
        }


        [TestMethod]
        public void rep_scasb()
        {
            es = 0x0020;
            di = 0x200;

            for (int i = 0; i < 16; i++)
            {
                WriteByte(es, (ushort)(di + i), 0x11);
            }

            al = 0x11;
            cx = 16;
            emit("repz scasb");
            run();
            Assert.AreEqual(al, 0x11);
            Assert.AreEqual(di, 0x210);
            Assert.IsTrue(FlagZ);
        }

        [TestMethod]
        public void rep_scasb_cx0()
        {
            es = 0xFFFF;
            di = 0x200;

            al = 0x11;
            cx = 0;
            emit("repz scasb");
            run();
            Assert.AreEqual(al, 0x11);
            Assert.AreEqual(di, 0x200);
        }

        [TestMethod]
        public void rep_scasb_2()
        {
            es = 0x0020;
            di = 0x200;

            for (int i = 0; i < 16; i++)
            {
                WriteByte(es, (ushort)(di + i), 0x11);
            }
            WriteByte(es, (ushort)(di + 2), 0x12);

            al = 0x11;
            cx = 16;
            emit("repz scasb");
            run();
            Assert.AreEqual(al, 0x11);
            Assert.AreEqual(di, 0x203);
            Assert.IsFalse(FlagZ);
        }

        [TestMethod]
        public void rep_scasb_3()
        {
            es = 0x0020;
            di = 0x200;

            for (int i = 0; i < 16; i++)
            {
                WriteByte(es, (ushort)(di + i), 0x11);
            }
            WriteByte(es, (ushort)(di + 2), 0x12);

            al = 0x12;
            cx = 16;
            emit("repnz scasb");
            run();
            Assert.AreEqual(al, 0x12);
            Assert.AreEqual(di, 0x203);
            Assert.IsTrue(FlagZ);
        }

    }

}
