using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTests88 : CPUUnitTests
    {
        [TestMethod]
        public void Mov_Eb_Gb()
        {
            WriteByte(0, 100, 40);
            al = 20;
            bx = 100;
            emit("mov byte [bx], al");
            run();
            Assert.AreEqual(ReadByte(0, 100), 20);
        }

        [TestMethod]
        public void Mov_Ev_Gv()
        {
            WriteWord(0, 100, 4000);
            ax = 2000;
            bx = 100;
            emit("mov word [bx], ax");
            run();
            Assert.AreEqual(ReadWord(0, 100), 2000);
        }

        [TestMethod]
        public void Add_Gb_Eb()
        {
            WriteByte(0, 100, 40);
            al = 20;
            bx = 100;
            emit("mov al, byte [bx]");
            run();
            Assert.AreEqual(al, 40);
        }

        [TestMethod]
        public void Add_Gv_Ev()
        {
            WriteWord(0, 100, 4000);
            ax = 2000;
            bx = 100;
            emit("mov ax, word [bx]");
            run();
            Assert.AreEqual(ax, 4000);
        }

        [TestMethod]
        public void Mov_Ev_Sw()
        {
            WriteWord(0, 100, 4000);
            es = 2000;
            bx = 100;
            emit("mov word [bx], es");
            run();
            Assert.AreEqual(ReadWord(0, 100), 2000);
        }

        [TestMethod]
        public void Lea_Gv_M()
        {
            WriteWord(0, 100, 4000);
            bx = 0x8000;
            si = 0x0400;
            emit("lea ax,word [bx+si+20h]");
            run();
            Assert.AreEqual(ax, 0x8420);
        }

        [TestMethod]
        public void Add_Sw_Ev()
        {
            WriteWord(0, 100, 4000);
            es = 2000;
            bx = 100;
            emit("mov word [bx], es");
            run();
            Assert.AreEqual(ReadWord(0, 100), 2000);
        }

        [TestMethod]
        public void Pop_Ev()
        {
            sp = 0x8006;
            WriteWord(ss, sp, 4000);
            bx = 0x1000;
            si = 0x0100;
            emit("pop word [bx+si]");
            run();
            Assert.AreEqual(ReadWord(ds, 0x1100), 4000);
            Assert.AreEqual(sp, 0x8008);
        }




    }
}
