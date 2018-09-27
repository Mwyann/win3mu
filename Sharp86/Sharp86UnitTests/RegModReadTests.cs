using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class RegModReadTests : CPUUnitTests
    {
        #region Mode 00
        [TestMethod]
        public void Read_ds_bx_si()
        {
            WriteWord(0, 0x8110, 0x1234);

            ds = 0x0800;
            bx = 0x0100;
            si = 0x0010;

            emit("mov ax,[bx+si]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ds_bx_di()
        {
            WriteWord(0, 0x8110, 0x1234);

            ds = 0x0800;
            bx = 0x0100;
            di = 0x0010;

            emit("mov ax,[bx+di]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ss_bp_si()
        {
            WriteWord(0, 0x8110, 0x1234);

            ss = 0x0800;
            bp = 0x0100;
            si = 0x0010;

            emit("mov ax,[bp+si]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ss_bp_di()
        {
            WriteWord(0, 0x8110, 0x1234);

            ss = 0x0800;
            bp = 0x0100;
            di = 0x0010;

            emit("mov ax,[bp+di]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }


        [TestMethod]
        public void Read_ds_si()
        {
            WriteWord(0, 0x8010, 0x1234);

            ds = 0x0800;
            si = 0x0010;

            emit("mov ax,[si]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ds_di()
        {
            WriteWord(0, 0x8010, 0x1234);

            ds = 0x0800;
            di = 0x0010;

            emit("mov ax,[di]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ds_imm()
        {
            WriteWord(0, 0x8010, 0x1234);

            ds = 0x0800;

            emit("mov ax,[0x10]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ds_bx()
        {
            WriteWord(0, 0x8010, 0x1234);

            ds = 0x0800;
            bx = 0x0010;

            emit("mov ax,[bx]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }
        #endregion

        #region Mode 01 (8-bit displacement)
        [TestMethod]
        public void Read_ds_bx_si_disp8()
        {
            WriteWord(0, 0x8118, 0x1234);

            ds = 0x0800;
            bx = 0x0100;
            si = 0x0010;

            emit("mov ax,[bx+si+8]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ds_bx_di_disp8()
        {
            WriteWord(0, 0x8118, 0x1234);

            ds = 0x0800;
            bx = 0x0100;
            di = 0x0010;

            emit("mov ax,[bx+di+8]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ss_bp_si_disp8()
        {
            WriteWord(0, 0x8118, 0x1234);

            ss = 0x0800;
            bp = 0x0100;
            si = 0x0010;

            emit("mov ax,[bp+si+8]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ss_bp_di_disp8()
        {
            WriteWord(0, 0x8118, 0x1234);

            ss = 0x0800;
            bp = 0x0100;
            di = 0x0010;

            emit("mov ax,[bp+di+8]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }


        [TestMethod]
        public void Read_ds_si_disp8()
        {
            WriteWord(0, 0x8018, 0x1234);

            ds = 0x0800;
            si = 0x0010;

            emit("mov ax,[si+8]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ds_di_disp8()
        {
            WriteWord(0, 0x8018, 0x1234);

            ds = 0x0800;
            di = 0x0010;

            emit("mov ax,[di+8]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ss_bp_disp8()
        {
            WriteWord(0, 0x8018, 0x1234);

            ss = 0x0800;
            bp = 0x0010;

            emit("mov ax,[bp+8]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ds_bx_disp8()
        {
            WriteWord(0, 0x8018, 0x1234);

            ds = 0x0800;
            bx = 0x0010;

            emit("mov ax,[bx+8]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ds_bx_disp8_negative()
        {
            WriteWord(0, 0x8018, 0x1234);

            ds = 0x0800;
            bx = 0x0020;

            emit("mov ax,[bx-8]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }
        #endregion


        #region Mode 10 (16-bit displacement)
        [TestMethod]
        public void Read_ds_bx_si_disp16()
        {
            WriteWord(0, 0x8318, 0x1234);

            ds = 0x0800;
            bx = 0x0100;
            si = 0x0010;

            emit("mov ax,[bx+si+208h]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ds_bx_di_disp16()
        {
            WriteWord(0, 0x8318, 0x1234);

            ds = 0x0800;
            bx = 0x0100;
            di = 0x0010;

            emit("mov ax,[bx+di+208h]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ss_bp_si_disp16()
        {
            WriteWord(0, 0x8318, 0x1234);

            ss = 0x0800;
            bp = 0x0100;
            si = 0x0010;

            emit("mov ax,[bp+si+208h]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ss_bp_di_disp16()
        {
            WriteWord(0, 0x8318, 0x1234);

            ss = 0x0800;
            bp = 0x0100;
            di = 0x0010;

            emit("mov ax,[bp+di+208h]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }


        [TestMethod]
        public void Read_ds_si_disp16()
        {
            WriteWord(0, 0x8218, 0x1234);

            ds = 0x0800;
            si = 0x0010;

            emit("mov ax,[si+208h]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ds_di_disp16()
        {
            WriteWord(0, 0x8218, 0x1234);

            ds = 0x0800;
            di = 0x0010;

            emit("mov ax,[di+208h]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ss_bp_disp16()
        {
            WriteWord(0, 0x8218, 0x1234);

            ss = 0x0800;
            bp = 0x0010;

            emit("mov ax,[bp+208h]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ds_bx_disp16()
        {
            WriteWord(0, 0x8218, 0x1234);

            ds = 0x0800;
            bx = 0x0010;

            emit("mov ax,[bx+208h]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_ds_bx_disp16_negative()
        {
            WriteWord(0, 0x8018, 0x1234);

            ds = 0x0800;
            bx = 0x0220;

            emit("mov ax,[bx-208h]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }
        #endregion

        #region Mode 11 (register)

        [TestMethod]
        public void Read_ax()
        {
            ax = 0x1234;

            // MOV AX, AX
            emit("db 08bh, 0c0h + (0 << 3) + 0");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_cx()
        {
            cx = 0x1234;

            // MOV AX, CX
            emit("db 08bh, 0c0h + (0 << 3) + 1");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_dx()
        {
            dx = 0x1234;

            // MOV AX, DX
            emit("db 08bh, 0c0h + (0 << 3) + 2");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_bx()
        {
            bx = 0x1234;

            // MOV AX, BX
            emit("db 08bh, 0c0h + (0 << 3) + 3");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_sp()
        {
            sp = 0x1234;

            // MOV AX, SP
            emit("db 08bh, 0c0h + (0 << 3) + 4");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_bp()
        {
            bp = 0x1234;

            // MOV AX, BP
            emit("db 08bh, 0c0h + (0 << 3) + 5");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_si()
        {
            si = 0x1234;

            // MOV AX, BP
            emit("db 08bh, 0c0h + (0 << 3) + 6");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_di()
        {
            di = 0x1234;

            // MOV AX, BP
            emit("db 08bh, 0c0h + (0 << 3) + 7");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        #endregion

        #region Segment overrides
        [TestMethod]
        public void Read_es_bx()
        {
            WriteWord(0, 0x8010, 0x1234);

            es = 0x0800;
            bx = 0x0010;

            emit("mov ax,[es:bx]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }


        [TestMethod]
        public void Read_ss_bx()
        {
            WriteWord(0, 0x8010, 0x1234);

            ss = 0x0800;
            bx = 0x0010;

            emit("mov ax,[ss:bx]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Read_es_bp_di()
        {
            WriteWord(0, 0x8110, 0x1234);

            es = 0x0800;
            bp = 0x0100;
            di = 0x0010;

            emit("mov ax,[es:bp+di]");
            run();

            Assert.AreEqual(ax, 0x1234);
        }
        #endregion
    }
}
