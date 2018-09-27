using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTestsF0 : CPUUnitTests
    {
        [TestMethod]
        public void hlt()
        {
            cx = 1;
            emit("hlt");
            step();
            Assert.IsTrue(Halted);
        }

        [TestMethod]
        public void cmc()
        {
            FlagC = false;
            emit("cmc");
            step();
            Assert.IsTrue(FlagC);

            emit("cmc");
            step();
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void test_Eb_Ib()
        {
            si = 0x8000;
            WriteByte(ds, si, 0x88);
            emit("test byte [si],0x80");
            step();
            Assert.IsFalse(FlagZ);

            emit("test byte [si],0x1");
            step();
            Assert.IsTrue(FlagZ);
        }

        [TestMethod]
        public void not_Eb_Ib()
        {
            si = 0x8000;
            WriteByte(ds, si, 0x88);
            emit("not byte [si]");
            step();
            Assert.AreEqual(ReadByte(ds, si), 0x77);
        }

        [TestMethod]
        public void neg_Eb_Ib()
        {
            si = 0x8000;
            WriteByte(ds, si, 1);
            emit("neg byte [si]");
            step();
            Assert.AreEqual(ReadByte(ds, si), 0xFF);
        }

        [TestMethod]
        public void mul_Eb_Ib()
        {
            al = 100;
            WriteByte(ds, si, 200);
            emit("mul byte [si]");
            step();
            Assert.AreEqual(ax, 20000);
        }

        [TestMethod]
        public void imul_Eb_Ib()
        {
            al = 0xFF;
            WriteByte(ds, si, 0xFF);
            emit("imul byte [si]");
            step();
            Assert.AreEqual(ax, 1);
        }

        [TestMethod]
        public void div_Eb_Ib()
        {
            ax = 20001;
            WriteByte(ds, si, 200);
            emit("div byte [si]");
            step();
            Assert.AreEqual(al, 100);
            Assert.AreEqual(ah, 1);
        }

        [TestMethod]
        public void idiv_Eb_Ib()
        {
            ax = 10001;
            WriteByte(ds, si, 100);
            emit("idiv byte [si]");
            step();
            Assert.AreEqual(al, 100);
            Assert.AreEqual(ah, 1);
        }



        [TestMethod]
        public void test_Ev_Iv()
        {
            si = 0x8000;
            WriteWord(ds, si, 0x8888);
            emit("test word [si],0x8000");
            step();
            Assert.IsFalse(FlagZ);

            emit("test word [si],0x1");
            step();
            Assert.IsTrue(FlagZ);
        }

        [TestMethod]
        public void not_Ev_Iv()
        {
            si = 0x8000;
            WriteWord(ds, si, 0x8888);
            emit("not word [si]");
            step();
            Assert.AreEqual(ReadWord(ds, si), 0x7777);
        }

        [TestMethod]
        public void neg_Ev_Iv()
        {
            si = 0x8000;
            WriteWord(ds, si, 1);
            emit("neg word [si]");
            step();
            Assert.AreEqual(ReadWord(ds, si), 0xFFFF);
        }

        [TestMethod]
        public void mul_Ev_Iv()
        {
            ax = 100;
            dx = 0xFFFF;
            WriteWord(ds, si, 200);
            emit("mul word [si]");
            step();
            Assert.AreEqual(ax, 20000);
            Assert.AreEqual(dx, 0);
        }

        [TestMethod]
        public void imul_Ev_Iv()
        {
            ax = 0xFFFF;
            dx = 0xFFFF;
            WriteWord(ds, si, 0xFFFF);
            emit("imul word [si]");
            step();
            Assert.AreEqual(ax, 1);
            Assert.AreEqual(dx, 0);
        }

        [TestMethod]
        public void div_Ev_Iv()
        {
            ax = 20001;
            dx = 0;
            WriteWord(ds, si, 200);
            emit("div word [si]");
            step();
            Assert.AreEqual(ax, 100);
            Assert.AreEqual(dx, 1);
        }

        [TestMethod]
        public void idiv_Ev_Iv()
        {
            ax = 20001;
            dx = 0;
            WriteWord(ds, si, 100);
            emit("idiv word [si]");
            step();
            Assert.AreEqual(ax, 200);
            Assert.AreEqual(dx, 1);
        }

    }
}
