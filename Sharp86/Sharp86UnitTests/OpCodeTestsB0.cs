using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTestsB0 : CPUUnitTests
    {
        [TestMethod]
        public void mov_al_ib()
        {
            emit("mov al,12h");
            run();
            Assert.AreEqual(al, 0x12);
        }

        [TestMethod]
        public void mov_cl_ib()
        {
            emit("mov cl,12h");
            run();
            Assert.AreEqual(cl, 0x12);
        }

        [TestMethod]
        public void mov_dl_ib()
        {
            emit("mov dl,12h");
            run();
            Assert.AreEqual(dl, 0x12);
        }

        [TestMethod]
        public void mov_bl_ib()
        {
            emit("mov bl,12h");
            run();
            Assert.AreEqual(bl, 0x12);
        }

        [TestMethod]
        public void mov_ah_ib()
        {
            emit("mov ah,12h");
            run();
            Assert.AreEqual(ah, 0x12);
        }

        [TestMethod]
        public void mov_ch_ib()
        {
            emit("mov ch,12h");
            run();
            Assert.AreEqual(ch, 0x12);
        }

        [TestMethod]
        public void mov_dh_ib()
        {
            emit("mov dh,12h");
            run();
            Assert.AreEqual(dh, 0x12);
        }

        [TestMethod]
        public void mov_bh_ib()
        {
            emit("mov bh,12h");
            run();
            Assert.AreEqual(bh, 0x12);
        }

    }

}
