using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTests60 : CPUUnitTests
    {
        bool _boundsExceeded = false;

        [TestInitialize]
        public override void Reset()
        {
            _boundsExceeded = false;
            base.Reset();
        }

        public override void RaiseInterrupt(byte interruptNumber)
        {
            if (interruptNumber==5)
            {
                _boundsExceeded = true;
                return;
            }

            base.RaiseInterrupt(interruptNumber);
        }

        [TestMethod]
        public void pusha_popa()
        {
            sp = 0x1000;
            ax = 1;
            bx = 2;
            cx = 3;
            dx = 4;
            bp = 5;
            si = 6;
            di = 7;

            emit("pusha");
            step();

            Assert.AreEqual(sp, 0x0FF0);
            Assert.AreEqual(this.ReadWord(ss, (ushort)(sp + 0)), di);
            Assert.AreEqual(this.ReadWord(ss, (ushort)(sp + 2)), si);
            Assert.AreEqual(this.ReadWord(ss, (ushort)(sp + 4)), bp);
            Assert.AreEqual(this.ReadWord(ss, (ushort)(sp + 6)), 0x1000);
            Assert.AreEqual(this.ReadWord(ss, (ushort)(sp + 8)), bx);
            Assert.AreEqual(this.ReadWord(ss, (ushort)(sp + 10)), dx);
            Assert.AreEqual(this.ReadWord(ss, (ushort)(sp + 12)), cx);
            Assert.AreEqual(this.ReadWord(ss, (ushort)(sp + 14)), ax);

            ax = 0;
            bx = 0;
            cx = 0;
            dx = 0;
            bp = 0;
            si = 0;
            di = 0;

            emit("popa");
            step();

            Assert.AreEqual(sp, 0x1000);
            Assert.AreEqual(ax, 1);
            Assert.AreEqual(bx, 2);
            Assert.AreEqual(cx, 3);
            Assert.AreEqual(dx, 4);
            Assert.AreEqual(bp, 5);
            Assert.AreEqual(si, 6);
            Assert.AreEqual(di, 7);
        }


        [TestMethod]
        public void bound_r16_m16()
        {
            di = 0x1000;
            WriteWord(ds, di, unchecked((ushort)(short)-10));
            WriteWord(ds, (ushort)(di + 2), unchecked((ushort)(short)20));

            _boundsExceeded = false;
            ax = 0;
            emit("bound ax,word [di]");
            step();
            Assert.IsFalse(_boundsExceeded);

            _boundsExceeded = false;
            ax = unchecked((ushort)(short)-11);
            emit("bound ax,word [di]");
            step();
            Assert.IsTrue(_boundsExceeded);

            _boundsExceeded = false;
            ax = unchecked((ushort)(short)-10);
            emit("bound ax,word [di]");
            step();
            Assert.IsFalse(_boundsExceeded);

            _boundsExceeded = false;
            ax = unchecked((ushort)(short)21);
            emit("bound ax,word [di]");
            step();
            Assert.IsTrue(_boundsExceeded);

            _boundsExceeded = false;
            ax = unchecked((ushort)(short)20);
            emit("bound ax,word [di]");
            step();
            Assert.IsFalse(_boundsExceeded);
        }
    }
}
