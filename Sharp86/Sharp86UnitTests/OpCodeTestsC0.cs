using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTestsC0 : CPUUnitTests
    {
        [TestMethod]
        public void rol_eb_Ib()
        {
            al = 0x81;
            emit("rol al,2");
            step();
            Assert.AreEqual(al, 0x06);
        }

        [TestMethod]
        public void ror_eb_Ib()
        {
            al = 0x81;
            emit("ror al,2");
            step();
            Assert.AreEqual(al, 0x60);
        }

        [TestMethod]
        public void rcl_eb_Ib()
        {
            FlagC = true;
            al = 0x01;
            emit("rcl al,2");
            step();
            Assert.AreEqual(al, 0x06);
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void rcr_eb_Ib()
        {
            FlagC = true;
            al = 0x80;
            emit("rcr al,2");
            step();
            Assert.AreEqual(al, 0x60);
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void shl_eb_Ib()
        {
            al = 0x81;
            emit("shl al,2");
            step();
            Assert.AreEqual(al, 0x04);
        }

        [TestMethod]
        public void shr_eb_Ib()
        {
            al = 0x81;
            emit("shr al,2");
            step();
            Assert.AreEqual(al, 0x20);
        }

        [TestMethod]
        public void sar_eb_Ib()
        {
            al = 0x84;
            emit("sar al,2");
            step();
            Assert.AreEqual(al, 0xE1);
        }



        [TestMethod]
        public void rol_ev_Ib()
        {
            ax = 0x8001;
            emit("rol ax,2");
            step();
            Assert.AreEqual(al, 0x06);
        }

        [TestMethod]
        public void ror_ev_Ib()
        {
            ax = 0x8001;
            emit("ror ax,2");
            step();
            Assert.AreEqual(ax, 0x6000);
        }

        [TestMethod]
        public void rcl_Ev_Ib()
        {
            FlagC = true;
            ax = 0x01;
            emit("rcl ax,2");
            step();
            Assert.AreEqual(ax, 0x06);
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void rcr_ev_Ib()
        {
            FlagC = true;
            ax = 0x8000;
            emit("rcr ax,2");
            step();
            Assert.AreEqual(ax, 0x6000);
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void shl_ev_Ib()
        {
            ax = 0x8001;
            emit("shl ax,2");
            step();
            Assert.AreEqual(ax, 0x04);
        }

        [TestMethod]
        public void shr_ev_Ib()
        {
            ax = 0x8001;
            emit("shr ax,2");
            step();
            Assert.AreEqual(ax, 0x2000);
        }

        [TestMethod]
        public void sar_ev_Ib()
        {
            ax = 0x8004;
            emit("sar ax,2");
            step();
            Assert.AreEqual(ax, 0xE001);
        }



        [TestMethod]
        public void ret_Iv()
        {
            sp = 0xFFE;
            WriteWord(ss, sp, 0x8000);
            emit("ret 0x1000");
            step();
            Assert.AreEqual(ip, 0x8000);
            Assert.AreEqual(sp, 0x2000);
        }

        [TestMethod]
        public void ret()
        {
            sp = 0xFFE;
            WriteWord(ss, sp, 0x8000);
            emit("ret");
            step();
            Assert.AreEqual(ip, 0x8000);
            Assert.AreEqual(sp, 0x1000);
        }

        [TestMethod]
        public void les()
        {
            bx = 0x1000;
            WriteWord(ds, bx, 0x1234);
            WriteWord(ds, (ushort)(bx + 2), 0x5678);

            emit("les si,[bx]");
            step();

            Assert.AreEqual(si, 0x1234);
            Assert.AreEqual(es, 0x5678);
        }

        [TestMethod]
        public void lds()
        {
            bx = 0x1000;
            WriteWord(ds, bx, 0x1234);
            WriteWord(ds, (ushort)(bx + 2), 0x5678);

            emit("lds si,[bx]");
            step();

            Assert.AreEqual(si, 0x1234);
            Assert.AreEqual(ds, 0x5678);
        }

        [TestMethod]
        public void mov_Eb_Ib()
        {
            bx = 0x1000;

            emit("mov byte [bx],012h");
            step();

            Assert.AreEqual(ReadByte(ds, bx), 0x12);
        }

        [TestMethod]
        public void mov_Ev_Iv()
        {
            bx = 0x1000;

            emit("mov word [bx],01234h");
            step();

            Assert.AreEqual(ReadWord(ds, bx), 0x1234);
        }

    }

}
