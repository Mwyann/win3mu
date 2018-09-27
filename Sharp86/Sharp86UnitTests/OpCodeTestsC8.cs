using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTestsC8 : CPUUnitTests
    {
        [TestMethod]
        public void enter_leave()
        {
            sp = 0x1000;
            bp = 0x1010;
            emit("enter 0x100, 0");
            step();

            Assert.AreEqual(ReadWord(ss, 0x0FFE), 0x1010);      // BP on stack
            Assert.AreEqual(bp, 0xFFE);
            Assert.AreEqual(sp, 0xEFE);

            emit("leave");
            step();
            Assert.AreEqual(sp, 0x1000);
            Assert.AreEqual(bp, 0x1010);
        }

        [TestMethod]
        public void enter_leave_nested()
        {
            sp = 0x1000;
            bp = 0x1010;
            emit("enter 0x100, 0");
            step();

            Assert.AreEqual(ReadWord(ss, 0x0FFE), 0x1010);      // BP on stack
            Assert.AreEqual(bp, 0xFFE);
            Assert.AreEqual(sp, 0xEFE);

            emit("enter 0x30, 1");
            step();
            Assert.AreEqual(bp, 0xEFC);
            Assert.AreEqual(sp, 0xECA);

            Assert.AreEqual(ReadWord(ss, bp), 0xFFE);
            Assert.AreEqual(ReadWord(ss, (ushort)(bp-2)), 0xEFC);

            emit("enter 0x20, 2");
            step();
            Assert.AreEqual(bp, 0xEC8);
            Assert.AreEqual(sp, 0xEA4);

            Assert.AreEqual(ReadWord(ss, bp), 0xEFC);
            Assert.AreEqual(ReadWord(ss, (ushort)(bp - 2)), 0xEFC);
            Assert.AreEqual(ReadWord(ss, (ushort)(bp - 4)), 0xEC8);

            emit("leave");
            step();
            Assert.AreEqual(bp, 0xEFC);
            Assert.AreEqual(sp, 0xECA);

            emit("leave");
            step();
            Assert.AreEqual(bp, 0xFFE);
            Assert.AreEqual(sp, 0xEFE);

            emit("leave");
            step();
            Assert.AreEqual(sp, 0x1000);
            Assert.AreEqual(bp, 0x1010);
        }

        [TestMethod]
        public void retf_Iv()
        {
            sp = 0xFFC;
            WriteWord(ss, sp, 0x8000);
            WriteWord(ss, (ushort)(sp + 2), 0x1234);

            emit("retf 0x1000");
            step();
            Assert.AreEqual(ip, 0x8000);
            Assert.AreEqual(cs, 0x1234);
            Assert.AreEqual(sp, 0x2000);
        }


        [TestMethod]
        public void retf()
        {
            sp = 0xFFC;
            WriteWord(ss, sp, 0x8000);
            WriteWord(ss, (ushort)(sp + 2), 0x1234);
            emit("retf");
            step();
            Assert.AreEqual(ip, 0x8000);
            Assert.AreEqual(cs, 0x1234);
            Assert.AreEqual(sp, 0x1000);
        }

        byte _raisedInterrupt;
        public override void RaiseInterrupt(byte interruptNumber)
        {
            _raisedInterrupt = interruptNumber;
        }

        [TestMethod]
        public void int3()
        {
            _raisedInterrupt = 0;
            emit("db 0xCC");        // int 3
            step();
            Assert.AreEqual(_raisedInterrupt, 3);
        }

        [TestMethod]
        public void int_Ib()
        {
            _raisedInterrupt = 0;
            emit("int 21h");        // int 3
            step();
            Assert.AreEqual(_raisedInterrupt, 0x21);
        }

        [TestMethod]
        public void into()
        {
            FlagO = false;
            _raisedInterrupt = 0;
            emit("into");
            step();
            Assert.AreEqual(_raisedInterrupt, 0x00);

            FlagO = true;
            emit("into");
            step();
            Assert.AreEqual(_raisedInterrupt, 0x04);
        }

        [TestMethod]
        public void iret()
        {
            sp = 0xFFA;
            WriteWord(ss, sp, 0x8000);
            WriteWord(ss, (ushort)(sp + 2), 0x1234);
            WriteWord(ss, (ushort)(sp + 4), 0xAAAA);
            emit("iret");
            step();
            Assert.AreEqual(ip, 0x8000);
            Assert.AreEqual(cs, 0x1234);
            Assert.AreEqual(EFlags, (0xAAAA & EFlag.SupportedBits) | EFlag.FixedBits);
            Assert.AreEqual(sp, 0x1000);
        }


    }

}
