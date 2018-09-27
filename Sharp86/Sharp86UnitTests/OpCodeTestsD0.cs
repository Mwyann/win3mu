using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTestsD0 : CPUUnitTests
    {
        [TestMethod]
        public void rol_eb_1()
        {
            al = 0x81;
            emit("rol al,1");
            step();
            Assert.AreEqual(al, 0x03);
        }

        [TestMethod]
        public void ror_eb_1()
        {
            al = 0x81;
            emit("ror al,1");
            step();
            Assert.AreEqual(al, 0xc0);
        }

        [TestMethod]
        public void rcl_eb_1()
        {
            FlagC = true;
            al = 0x01;
            emit("rcl al,1");
            step();
            Assert.AreEqual(al, 0x03);
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void rcr_eb_1()
        {
            FlagC = true;
            al = 0x80;
            emit("rcr al,1");
            step();
            Assert.AreEqual(al, 0xC0);
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void shl_eb_1()
        {
            al = 0x81;
            emit("shl al,1");
            step();
            Assert.AreEqual(al, 0x02);
        }

        [TestMethod]
        public void shr_eb_1()
        {
            al = 0x81;
            emit("shr al,1");
            step();
            Assert.AreEqual(al, 0x40);
        }

        [TestMethod]
        public void sar_eb_1()
        {
            al = 0x82;
            emit("sar al,1");
            step();
            Assert.AreEqual(al, 0xC1);
        }



        [TestMethod]
        public void rol_ev_1()
        {
            ax = 0x8001;
            emit("rol ax,1");
            step();
            Assert.AreEqual(al, 0x03);
        }

        [TestMethod]
        public void ror_ev_1()
        {
            ax = 0x8001;
            emit("ror ax,1");
            step();
            Assert.AreEqual(ax, 0xc000);
        }

        [TestMethod]
        public void rcl_Ev_1()
        {
            FlagC = true;
            ax = 0x01;
            emit("rcl ax,1");
            step();
            Assert.AreEqual(ax, 0x03);
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void rcr_ev_1()
        {
            FlagC = true;
            ax = 0x8000;
            emit("rcr ax,1");
            step();
            Assert.AreEqual(ax, 0xC000);
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void shl_ev_1()
        {
            ax = 0x8001;
            emit("shl ax,1");
            step();
            Assert.AreEqual(ax, 0x02);
        }

        [TestMethod]
        public void shr_ev_1()
        {
            ax = 0x8001;
            emit("shr ax,1");
            step();
            Assert.AreEqual(ax, 0x4000);
        }

        [TestMethod]
        public void sar_ev_1()
        {
            ax = 0x8002;
            emit("sar ax,1");
            step();
            Assert.AreEqual(ax, 0xC001);
        }




        [TestMethod]
        public void rol_eb_cl()
        {
            al = 0x81;
            cl = 1;
            emit("rol al,cl");
            step();
            Assert.AreEqual(al, 0x03);
        }

        [TestMethod]
        public void ror_eb_cl()
        {
            al = 0x81;
            cl = 1;
            emit("ror al,cl");
            step();
            Assert.AreEqual(al, 0xc0);
        }

        [TestMethod]
        public void rcl_eb_cl()
        {
            FlagC = true;
            al = 0x01;
            cl = 1;
            emit("rcl al,cl");
            step();
            Assert.AreEqual(al, 0x03);
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void rcr_eb_cl()
        {
            FlagC = true;
            al = 0x80;
            cl = 1;
            emit("rcr al,cl");
            step();
            Assert.AreEqual(al, 0xC0);
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void shl_eb_cl()
        {
            al = 0x81;
            cl = 1;
            emit("shl al,cl");
            step();
            Assert.AreEqual(al, 0x02);
        }

        [TestMethod]
        public void shr_eb_cl()
        {
            al = 0x81;
            cl = 1;
            emit("shr al,cl");
            step();
            Assert.AreEqual(al, 0x40);
        }

        [TestMethod]
        public void sar_eb_cl()
        {
            al = 0x82;
            cl = 1;
            emit("sar al,cl");
            step();
            Assert.AreEqual(al, 0xC1);
        }



        [TestMethod]
        public void rol_ev_cl()
        {
            ax = 0x8001;
            cl = 1;
            emit("rol ax,cl");
            step();
            Assert.AreEqual(al, 0x03);
        }

        [TestMethod]
        public void ror_ev_cl()
        {
            ax = 0x8001;
            cl = 1;
            emit("ror ax,cl");
            step();
            Assert.AreEqual(ax, 0xc000);
        }

        [TestMethod]
        public void rcl_Ev_cl()
        {
            FlagC = true;
            ax = 0x01;
            cl = 1;
            emit("rcl ax,cl");
            step();
            Assert.AreEqual(ax, 0x03);
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void rcr_ev_cl()
        {
            FlagC = true;
            ax = 0x8000;
            cl = 1;
            emit("rcr ax,cl");
            step();
            Assert.AreEqual(ax, 0xC000);
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void shl_ev_cl()
        {
            ax = 0x8001;
            cl = 1;
            emit("shl ax,cl");
            step();
            Assert.AreEqual(ax, 0x02);
        }

        [TestMethod]
        public void shr_ev_cl()
        {
            ax = 0x8001;
            cl = 1;
            emit("shr ax,cl");
            step();
            Assert.AreEqual(ax, 0x4000);
        }

        [TestMethod]
        public void sar_ev_cl()
        {
            ax = 0x8002;
            cl = 1;
            emit("sar ax,cl");
            step();
            Assert.AreEqual(ax, 0xC001);
        }

        [TestMethod]
        public void Aam()
        {
            ax = 0xFF0E;
            emit("aam");
            step();
            Assert.AreEqual(ax, 0x104);
        }

        [TestMethod]
        public void Aam_Im()
        {
            ax = 0xFF0E;
            emit("aam 8");
            step();
            Assert.AreEqual(ax, 0x106);
        }

        [TestMethod]
        public void Aad()
        {
            ax = 0xFF0E;
            emit("aad");
            step();
            Assert.AreEqual(ax, 0x0004);
        }

        [TestMethod]
        public void Aad_Im()
        {
            ax = 0xFF0E;
            emit("aad 8");
            step();
            Assert.AreEqual(ax, 0x006);
        }

        [TestMethod]
        public void xlat()
        {
            bx = 0x1000;
            for (int i=0; i<256; i++)
            {
                WriteByte(ds, (ushort)(bx + i), (byte)i);
            }

            al = 0x20;
            emit("xlat");
            step();
            Assert.AreEqual(al, 0x20);

            al = 0x90;      // al is signed
            emit("xlat");
            step();
            Assert.AreEqual(al, 0x90);
        }
    }

}
