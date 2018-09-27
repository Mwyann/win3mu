using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTestsF8 : CPUUnitTests
    {
        [TestMethod]
        public void clc()
        {
            FlagC = true;
            emit("clc");
            step();
            Assert.IsFalse(FlagC);
        }

        [TestMethod]
        public void stc()
        {
            FlagC = false;
            emit("stc");
            step();
            Assert.IsTrue(FlagC);
        }

        [TestMethod]
        public void cli()
        {
            FlagI = true;
            emit("cli");
            step();
            Assert.IsFalse(FlagI);
        }

        [TestMethod]
        public void sti()
        {
            FlagI = false;
            emit("sti");
            step();
            Assert.IsTrue(FlagI);
        }

        [TestMethod]
        public void cld()
        {
            FlagD = true;
            emit("cld");
            step();
            Assert.IsFalse(FlagD);
        }

        [TestMethod]
        public void std()
        {
            FlagD = false;
            emit("std");
            step();
            Assert.IsTrue(FlagD);
        }

        [TestMethod]
        public void inc_Eb()
        {
            bx = 0x1000;
            WriteByte(ds, bx, 0x12);
            emit("inc byte [bx]");
            step();
            Assert.AreEqual(ReadByte(ds, bx), 0x13);
        }

        [TestMethod]
        public void dec_Eb()
        {
            bx = 0x1000;
            WriteByte(ds, bx, 0x12);
            emit("dec byte [bx]");
            step();
            Assert.AreEqual(ReadByte(ds, bx), 0x11);
        }

        [TestMethod]
        public void inc_Ev()
        {
            bx = 0x1000;
            WriteWord(ds, bx, 0x1234);
            emit("inc word [bx]");
            step();
            Assert.AreEqual(ReadWord(ds, bx), 0x1235);
        }

        [TestMethod]
        public void dec_Ev()
        {
            bx = 0x1000;
            WriteWord(ds, bx, 0x1234);
            emit("dec word [bx]");
            step();
            Assert.AreEqual(ReadWord(ds, bx), 0x1233);
        }

        [TestMethod]
        public void call_Ev()
        {
            sp = 0x2000;
            ax = 0x1000;
            emit("call ax");
            step();
            Assert.AreEqual(ip, 0x1000);
            Assert.AreEqual(sp, 0x1ffe);
            ushort retaddr = ReadWord(ss, sp);
            Assert.AreEqual(retaddr, 0x0102);
        }

        [TestMethod]
        public void call_far_Mp()
        {
            sp = 0x2000;
            bx = 0x1000;
            WriteWord(ds, bx, 0x2000);
            WriteWord(ds, (ushort)(bx+2), 0x4000);

            emit("call word far [bx]");
            step();

            Assert.AreEqual(ip, 0x2000);
            Assert.AreEqual(cs, 0x4000);
            Assert.AreEqual(sp, 0x1ffc);
            Assert.AreEqual(ReadWord(ss, sp), 0x102);
            Assert.AreEqual(ReadWord(ss, (ushort)(sp + 2)), 0);
        }

        [TestMethod]
        public void jmp_Ev()
        {
            ax = 0x1000;
            emit("jmp ax");
            step();
            Assert.AreEqual(ip, 0x1000);
        }

        [TestMethod]
        public void jmp_far_Mp()
        {
            bx = 0x1000;
            WriteWord(ds, bx, 0x2000);
            WriteWord(ds, (ushort)(bx + 2), 0x4000);

            emit("jmp word far [bx]");
            step();

            Assert.AreEqual(ip, 0x2000);
            Assert.AreEqual(cs, 0x4000);
        }

        [TestMethod]
        public void push_Ev()
        {
            sp = 0x1000;
            bx = 0x1000;
            WriteWord(ds, bx, 0x1234);

            emit("push word [bx]");
            step();

            Assert.AreEqual(sp, 0xFFE);
            Assert.AreEqual(ReadWord(ss, sp), 0x1234);
        }


    }
}
