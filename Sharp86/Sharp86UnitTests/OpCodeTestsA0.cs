using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTestsA0 : CPUUnitTests
    {
        [TestMethod]
        public void mov_al_Ob()
        {
            es = 0x0020;
            WriteByte(es, 0x1000, 0x12);
            emit("mov al,[es:0x1000]");
            run();
            Assert.AreEqual(al, 0x12);
        }

        [TestMethod]
        public void mov_ax_Ob()
        {
            es = 0x0020;
            WriteWord(es, 0x1000, 0x1234);
            emit("mov ax,[es:0x1000]");
            run();
            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void mov_Ob_al()
        {
            es = 0x0020;
            al = 0x12;
            emit("mov [es:0x1000],al");
            run();
            Assert.AreEqual(ReadByte(es, 0x1000), 0x12);
        }

        [TestMethod]
        public void mov_Ob_ax()
        {
            es = 0x0020;
            ax = 0x1234;
            emit("mov [es:0x1000],ax");
            run();
            Assert.AreEqual(ReadWord(es, 0x1000), 0x1234);
        }

        [TestMethod]
        public void movsb()
        {
            es = 0x0020;
            ds = 0x0120;
            si = 0x100;
            di = 0x200;

            WriteByte(ds, si, 0x12);
            emit("movsb");
            run();
            Assert.AreEqual(ReadByte(es, (ushort)(di - 1)), 0x12);
            Assert.AreEqual(si, 0x101);
            Assert.AreEqual(di, 0x201);
        }

        [TestMethod]
        public void rep_movsb()
        {
            es = 0x0020;
            ds = 0x0120;
            si = 0x100;
            di = 0x200;
            cx = 16;

            for (int i = 0; i < 16; i++)
            {
                WriteByte(ds, (ushort)(si + i), (byte)(0x10 + i));
            }

            emit("rep movsb");
            run();

            Assert.AreEqual(si, 0x110);
            Assert.AreEqual(di, 0x210);
            Assert.AreEqual(cx, 0);
            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(ReadByte(es, (ushort)(di - 16 + i)), (byte)(0x10 + i));
            }
        }

        [TestMethod]
        public void rep_movsb_cx0()
        {
            es = 0xFFFF;
            ds = 0x0120;
            si = 0x100;
            di = 0x200;
            cx = 0;

            emit("rep movsb");
            run();

            Assert.AreEqual(si, 0x100);
            Assert.AreEqual(di, 0x200);
            Assert.AreEqual(cx, 0);
        }

        [TestMethod]
        public void rep_movsb_overwrite()
        {
            es = 0x0020;
            ds = 0x0020;
            si = 0x100;
            di = 0x101;
            cx = 15;

            for (int i = 0; i < 16; i++)
            {
                WriteByte(ds, (ushort)(si + i), (byte)(0x10 + i));
            }

            emit("rep movsb");
            run();

            Assert.AreEqual(si, 0x10f);
            Assert.AreEqual(di, 0x110);
            Assert.AreEqual(cx, 0);
            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(ReadByte(es, (ushort)(di - 16 + i)), (byte)(0x10));
            }
        }

        [TestMethod]
        public void movsw()
        {
            es = 0x0020;
            ds = 0x0120;
            si = 0x100;
            di = 0x200;

            WriteWord(ds, si, 0x1234);
            emit("movsw");
            run();
            Assert.AreEqual(ReadWord(es, (ushort)(di - 2)), 0x1234);
            Assert.AreEqual(si, 0x102);
            Assert.AreEqual(di, 0x202);
        }

        [TestMethod]
        public void rep_movsw()
        {
            es = 0x0020;
            ds = 0x0120;
            si = 0x100;
            di = 0x200;
            cx = 16;

            for (int i = 0; i < 16; i++)
            {
                WriteWord(ds, (ushort)(si + i * 2), (ushort)(0x1234 + i));
            }

            emit("rep movsw");
            run();

            Assert.AreEqual(si, 0x120);
            Assert.AreEqual(di, 0x220);
            Assert.AreEqual(cx, 0);
            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(ReadWord(es, (ushort)(di - 32 + i * 2)), (ushort)(0x1234 + i));
            }
        }

        [TestMethod]
        public void rep_movsw_cx0()
        {
            es = 0xFFFF;
            ds = 0x0120;
            si = 0x100;
            di = 0x200;
            cx = 0;

            emit("rep movsw");
            run();

            Assert.AreEqual(si, 0x100);
            Assert.AreEqual(di, 0x200);
            Assert.AreEqual(cx, 0);
        }

        [TestMethod]
        public void rep_movsb_d()
        {
            es = 0x0020;
            ds = 0x0120;
            si = 0x100;
            di = 0x200;
            cx = 16;
            FlagD = true;

            for (int i = 0; i < 16; i++)
            {
                WriteByte(ds, (ushort)(si - i), (byte)(0x10 + i));
            }

            emit("rep movsb");
            run();

            Assert.AreEqual(si, 0xF0);
            Assert.AreEqual(di, 0x1F0);
            Assert.AreEqual(cx, 0);
            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(ReadByte(es, (ushort)(di + 16 - i)), (byte)(0x10 + i));
            }
        }

        [TestMethod]
        public void rep_movsw_d()
        {
            es = 0x0020;
            ds = 0x0120;
            si = 0x100;
            di = 0x200;
            cx = 16;
            FlagD = true;

            for (int i = 0; i < 16; i++)
            {
                WriteWord(ds, (ushort)(si - i *2), (ushort)(0x1234 + i));
            }

            emit("rep movsw");
            run();

            Assert.AreEqual(si, 0xE0);
            Assert.AreEqual(di, 0x1E0);
            Assert.AreEqual(cx, 0);
            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(ReadWord(es, (ushort)(di + 32 - i * 2)), (ushort)(0x1234 + i));
            }
        }


        [TestMethod]
        public void cmpsb()
        {
            es = 0x0020;
            ds = 0x0120;
            si = 0x100;
            di = 0x200;

            WriteByte(ds, si, 0x12);
            WriteByte(es, di, 0x56);
            emit("cmpsb");
            run();
            Assert.AreEqual(si, 0x101);
            Assert.AreEqual(di, 0x201);
            Assert.IsFalse(FlagZ);
            Assert.IsTrue(FlagC);
        }

        [TestMethod]
        public void rep_cmpsb()
        {
            es = 0x0020;
            ds = 0x0120;
            si = 0x100;
            di = 0x200;
            cx = 16;

            for (int i = 0; i < 16; i++)
            {
                WriteByte(ds, (ushort)(si + i), (byte)(0x10 + i));
                WriteByte(es, (ushort)(di + i), (byte)(0x10 + i));
            }

            WriteByte(es, (ushort)(di + 5), (byte)(0xFF));

            emit("repe cmpsb");
            run();

            Assert.AreEqual(si, 0x106);
            Assert.AreEqual(di, 0x206);
            Assert.AreEqual(cx, 10);
            Assert.IsFalse(FlagZ);
        }

        [TestMethod]
        public void rep_cmpsb_cx0()
        {
            es = 0xFFFF;
            ds = 0x0120;
            si = 0x100;
            di = 0x200;
            cx = 0;

            emit("repe cmpsb");
            run();

            Assert.AreEqual(si, 0x100);
            Assert.AreEqual(di, 0x200);
            Assert.AreEqual(cx, 0);
        }

        [TestMethod]
        public void rep_cmpsw()
        {
            es = 0x0020;
            ds = 0x0120;
            si = 0x100;
            di = 0x200;
            cx = 16;

            for (int i = 0; i < 16; i++)
            {
                WriteWord(ds, (ushort)(si + i * 2), (ushort)(0x1234 + i));
                WriteWord(es, (ushort)(di + i * 2), (ushort)(0x1234 + i));
            }

            WriteWord(es, (ushort)(di + 5 * 2), (ushort)(0xFFFF));

            emit("repe cmpsw");
            run();

            Assert.AreEqual(si, 0x10c);
            Assert.AreEqual(di, 0x20c);
            Assert.AreEqual(cx, 10);
            Assert.IsFalse(FlagZ);
        }

        [TestMethod]
        public void rep_cmpsw_cx0()
        {
            es = 0xFFFF;
            ds = 0x0120;
            si = 0x100;
            di = 0x200;
            cx = 0;

            emit("repe cmpsw");
            run();

            Assert.AreEqual(si, 0x100);
            Assert.AreEqual(di, 0x200);
            Assert.AreEqual(cx, 0);
        }

    }

}
