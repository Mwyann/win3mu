using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTests80 : CPUUnitTests
    {
        [TestMethod]
        public void Add_Eb_Ib()
        {
            WriteByte(0, 100, 40);
            emit("add byte [100], 20");
            run();
            Assert.AreEqual(ReadByte(0, 100), 60);
        }

        [TestMethod]
        public void Or_Eb_Ib()
        {
            WriteByte(0, 100, 0x41);
            emit("or byte [100], 21h");
            run();
            Assert.AreEqual(ReadByte(0, 100), 0x61);
        }

        [TestMethod]
        public void Adc_Eb_Ib()
        {
            WriteByte(0, 100, 40);
            FlagC = true;
            emit("adc byte [100], 20");
            run();
            Assert.AreEqual(ReadByte(0, 100), 61);
        }

        [TestMethod]
        public void Sbb_Eb_Ib()
        {
            WriteByte(0, 100, 40);
            FlagC = true;
            emit("sbb byte [100], 10");
            run();
            Assert.AreEqual(ReadByte(0, 100), 29);
        }

        [TestMethod]
        public void And_Eb_Ib()
        {
            WriteByte(0, 100, 0x60);
            emit("and byte [100], 20h");
            run();
            Assert.AreEqual(ReadByte(0, 100), 0x20);
        }

        [TestMethod]
        public void Sub_Eb_Ib()
        {
            WriteByte(0, 100, 40);
            FlagC = true;
            emit("sub byte [100], 10");
            run();
            Assert.AreEqual(ReadByte(0, 100), 30);
        }

        [TestMethod]
        public void Xor_Eb_Ib()
        {
            WriteByte(0, 100, 0x60);
            emit("xor byte [100], 0x20");
            run();
            Assert.AreEqual(ReadByte(0, 100), 0x40);
        }

        [TestMethod]
        public void Cmp_Eb_Ib()
        {
            WriteByte(0, 100, 40);
            FlagC = true;
            FlagZ = true;
            emit("cmp byte [100], 10");
            run();
            Assert.AreEqual(ReadByte(0, 100), 40);
            Assert.IsFalse(FlagZ);
            Assert.IsFalse(FlagC);
        }





        [TestMethod]
        public void Add_Ev_Iv()
        {
            WriteWord(0, 100, 4000);
            emit("add word [100], 2000");
            run();
            Assert.AreEqual(ReadWord(0, 100), 6000);
        }

        [TestMethod]
        public void Or_Ev_Iv()
        {
            WriteWord(0, 100, 0x4001);
            emit("or word [100], 2001h");
            run();
            Assert.AreEqual(ReadWord(0, 100), 0x6001);
        }


        [TestMethod]
        public void Adc_Ev_Iv()
        {
            WriteWord(0, 100, 4000);
            FlagC = true;
            emit("adc word [100], 2000");
            run();
            Assert.AreEqual(ReadWord(0, 100), 6001);
        }

        [TestMethod]
        public void Sbb_Ev_Iv()
        {
            WriteWord(0, 100, 4000);
            FlagC = true;
            emit("sbb word [100], 1000");
            run();
            Assert.AreEqual(ReadWord(0, 100), 2999);
        }

        [TestMethod]
        public void And_Ev_Iv()
        {
            WriteWord(0, 100, 0x6000);
            emit("and word [100], 2000h");
            run();
            Assert.AreEqual(ReadWord(0, 100), 0x2000);
        }

        [TestMethod]
        public void Sub_Ev_Iv()
        {
            WriteWord(0, 100, 4000);
            FlagC = true;
            emit("sub word [100], 1000");
            run();
            Assert.AreEqual(ReadWord(0, 100), 3000);
        }

        [TestMethod]
        public void Xor_Ev_Iv()
        {
            WriteWord(0, 100, 0x6000);
            emit("xor word [100], 2000h");
            run();
            Assert.AreEqual(ReadWord(0, 100), 0x4000);
        }

        [TestMethod]
        public void Cmp_Ev_Iv()
        {
            WriteWord(0, 100, 4000);
            FlagC = true;
            FlagZ = true;
            emit("cmp word [100], 1000");
            run();
            Assert.AreEqual(ReadWord(0, 100), 4000);
            Assert.IsFalse(FlagZ);
            Assert.IsFalse(FlagC);
        }





        [TestMethod]
        public void Add_Ev_Ib()
        {
            WriteWord(0, 100, 4000);
            emit("add word [100], byte 0xFF");
            run();
            Assert.AreEqual(ReadWord(0, 100), 3999);
        }

        [TestMethod]
        public void Or_Ev_Ib()
        {
            WriteWord(0, 100, 0x4001);
            emit("or word [100], byte 0xFE");
            run();
            Assert.AreEqual(ReadWord(0, 100), 0xFFFF);
        }


        [TestMethod]
        public void Adc_Ev_Ib()
        {
            WriteWord(0, 100, 4000);
            FlagC = true;
            emit("adc word [100], byte 0xFE");
            run();
            Assert.AreEqual(ReadWord(0, 100), 3999);
        }

        [TestMethod]
        public void Sbb_Ev_Ib()
        {
            WriteWord(0, 100, 4000);
            FlagC = true;
            emit("sbb word [100], byte 0xFE");
            run();
            Assert.AreEqual(ReadWord(0, 100), 4001);
        }

        [TestMethod]
        public void And_Ev_Ib()
        {
            WriteWord(0, 100, 0x6000);
            emit("and word [100], byte 0xFE");
            run();
            Assert.AreEqual(ReadWord(0, 100), 0x6000);
        }

        [TestMethod]
        public void Sub_Ev_Ib()
        {
            WriteWord(0, 100, 4000);
            FlagC = true;
            emit("sub word [100], byte 0xFF");
            run();
            Assert.AreEqual(ReadWord(0, 100), 4001);
        }

        [TestMethod]
        public void Xor_Ev_Ib()
        {
            WriteWord(0, 100, 0x6000);
            emit("xor word [100], byte 0xFF");
            run();
            Assert.AreEqual(ReadWord(0, 100), 0x9FFF);
        }

        [TestMethod]
        public void Cmp_Ev_Ib()
        {
            WriteWord(0, 100, 4000);
            FlagC = true;
            FlagZ = true;
            emit("cmp word [100], byte 0xFE");
            run();
            Assert.AreEqual(ReadWord(0, 100), 4000);
            Assert.IsFalse(FlagZ);
            Assert.IsTrue(FlagC);
        }

        [TestMethod]
        public void Test_Eb_Gb()
        {
            WriteByte(0, 100, 0x60);
            al = 0x20;
            emit("test byte [100], al");
            run();
            Assert.AreEqual(ReadByte(0, 100), 0x60);
            Assert.IsFalse(FlagZ);
        }

        [TestMethod]
        public void Test_Ev_Gv()
        {
            WriteWord(0, 100, 0x6000);
            ax = 0x2000;
            emit("test word [100], ax");
            run();
            Assert.AreEqual(ReadWord(0, 100), 0x6000);
            Assert.IsFalse(FlagZ);
        }

        [TestMethod]
        public void Xchg_Eb_Gb()
        {
            WriteByte(0, 100, 0x60);
            al = 0x20;
            emit("xchg byte [100], al");
            run();
            Assert.AreEqual(ReadByte(0, 100), 0x20);
            Assert.AreEqual(al, 0x60);
        }

        [TestMethod]
        public void Xchg_Ev_Gv()
        {
            WriteWord(0, 100, 0x6000);
            ax = 0x2000;
            emit("xchg word [100], ax");
            run();
            Assert.AreEqual(ReadWord(0, 100), 0x2000);
            Assert.AreEqual(ax, 0x6000);
        }


    }
}
