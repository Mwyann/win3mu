using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class RegModWriteTests : CPUUnitTests
    {
        #region Mode 00
        [TestMethod]
        public void Write_ds_bx_si()
        {

            ds = 0x0800;
            bx = 0x0100;
            si = 0x0010;
            ax = 0x1234;

            emit("mov [bx+si], ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8110), 0x1234);
        }

        [TestMethod]
        public void Write_ds_bx_di()
        {
            ds = 0x0800;
            bx = 0x0100;
            di = 0x0010;
            ax = 0x1234;

            emit("mov [bx+di], ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8110), 0x1234);
        }

        [TestMethod]
        public void Write_ss_bp_si()
        {
            ss = 0x0800;
            bp = 0x0100;
            si = 0x0010;
            ax = 0x1234;

            emit("mov [bp+si],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8110), 0x1234);
        }

        [TestMethod]
        public void Write_ss_bp_di()
        {
            ss = 0x0800;
            bp = 0x0100;
            di = 0x0010;
            ax = 0x1234;

            emit("mov [bp+di],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8110), 0x1234);
        }


        [TestMethod]
        public void Write_ds_si()
        {
            ds = 0x0800;
            si = 0x0010;
            ax = 0x1234;

            emit("mov [si],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8010), 0x1234);
        }

        [TestMethod]
        public void Write_ds_di()
        {
            ds = 0x0800;
            di = 0x0010;
            ax = 0x1234;

            emit("mov [di],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8010), 0x1234);
        }

        [TestMethod]
        public void Write_ds_imm()
        {
            ax = 0x1234;
            ds = 0x0800;

            emit("mov [0x10],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8010), 0x1234);
        }

        [TestMethod]
        public void Write_ds_bx()
        {
            ds = 0x0800;
            bx = 0x0010;
            ax = 0x1234;

            emit("mov [bx],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8010), 0x1234);
        }
        #endregion

        #region Mode 01 (8-bit displacement)
        [TestMethod]
        public void Write_ds_bx_si_disp8()
        {
            ds = 0x0800;
            bx = 0x0100;
            si = 0x0010;
            ax = 0x1234;

            emit("mov [bx+si+8],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8118), 0x1234);
        }

        [TestMethod]
        public void Write_ds_bx_di_disp8()
        {
            ds = 0x0800;
            bx = 0x0100;
            di = 0x0010;
            ax = 0x1234;

            emit("mov [bx+di+8],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8118), 0x1234);
        }

        [TestMethod]
        public void Write_ss_bp_si_disp8()
        {
            ax = 0x1234;

            ss = 0x0800;
            bp = 0x0100;
            si = 0x0010;

            emit("mov [bp+si+8],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8118), 0x1234);
        }

        [TestMethod]
        public void Write_ss_bp_di_disp8()
        {
            ax = 0x1234;

            ss = 0x0800;
            bp = 0x0100;
            di = 0x0010;

            emit("mov [bp+di+8],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8118), 0x1234);
        }


        [TestMethod]
        public void Write_ds_si_disp8()
        {
            ax = 0x1234;

            ds = 0x0800;
            si = 0x0010;

            emit("mov [si+8],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8018), 0x1234);
        }

        [TestMethod]
        public void Write_ds_di_disp8()
        {
            ax = 0x1234;

            ds = 0x0800;
            di = 0x0010;

            emit("mov [di+8],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8018), 0x1234);
        }

        [TestMethod]
        public void Write_ss_bp_disp8()
        {
            ax = 0x1234;

            ss = 0x0800;
            bp = 0x0010;

            emit("mov [bp+8],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8018), 0x1234);
        }

        [TestMethod]
        public void Write_ds_bx_disp8()
        {
            ax = 0x1234;

            ds = 0x0800;
            bx = 0x0010;

            emit("mov [bx+8],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8018), 0x1234);
        }

        [TestMethod]
        public void Write_ds_bx_disp8_negative()
        {
            ax = 0x1234;

            ds = 0x0800;
            bx = 0x0020;

            emit("mov [bx-8],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8018), 0x1234);
        }
        #endregion


        #region Mode 10 (16-bit displacement)
        [TestMethod]
        public void Write_ds_bx_si_disp16()
        {
            ax = 0x1234;

            ds = 0x0800;
            bx = 0x0100;
            si = 0x0010;

            emit("mov [bx+si+208h],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8318), 0x1234);
        }

        [TestMethod]
        public void Write_ds_bx_di_disp16()
        {
            ax = 0x1234;

            ds = 0x0800;
            bx = 0x0100;
            di = 0x0010;

            emit("mov [bx+di+208h],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8318), 0x1234);
        }

        [TestMethod]
        public void Write_ss_bp_si_disp16()
        {
            ax = 0x1234;

            ss = 0x0800;
            bp = 0x0100;
            si = 0x0010;

            emit("mov [bp+si+208h],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8318), 0x1234);
        }

        [TestMethod]
        public void Write_ss_bp_di_disp16()
        {
            ax = 0x1234;

            ss = 0x0800;
            bp = 0x0100;
            di = 0x0010;

            emit("mov [bp+di+208h],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8318), 0x1234);
        }


        [TestMethod]
        public void Write_ds_si_disp16()
        {
            ax = 0x1234;

            ds = 0x0800;
            si = 0x0010;

            emit("mov [si+208h],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8218), 0x1234);
        }

        [TestMethod]
        public void Write_ds_di_disp16()
        {
            ax = 0x1234;

            ds = 0x0800;
            di = 0x0010;

            emit("mov [di+208h],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8218), 0x1234);
        }

        [TestMethod]
        public void Write_ss_bp_disp16()
        {
            ax = 0x1234;

            ss = 0x0800;
            bp = 0x0010;

            emit("mov [bp+208h],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8218), 0x1234);
        }

        [TestMethod]
        public void Write_ds_bx_disp16()
        {
            ax = 0x1234;

            ds = 0x0800;
            bx = 0x0010;

            emit("mov [bx+208h],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8218), 0x1234);
        }

        [TestMethod]
        public void Write_ds_bx_disp16_negative()
        {
            ax = 0x1234;

            ds = 0x0800;
            bx = 0x0220;

            emit("mov [bx-208h],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8018), 0x1234);
        }
        #endregion

        #region Mode 11 (register)

        [TestMethod]
        public void Write_ax()
        {
            ax = 0x1234;

            // MOV AX, AX
            emit("db 089h, 0c0h + (0 << 3) + 0");
            run();

            Assert.AreEqual(ax, 0x1234);
        }

        [TestMethod]
        public void Write_cx()
        {
            ax = 0x1234;

            // MOV CX, AX
            emit("db 089h, 0c0h + (0 << 3) + 1");
            run();

            Assert.AreEqual(cx, 0x1234);
        }

        [TestMethod]
        public void Write_dx()
        {
            ax = 0x1234;

            // MOV DX, AX
            emit("db 089h, 0c0h + (0 << 3) + 2");
            run();

            Assert.AreEqual(dx, 0x1234);
        }

        [TestMethod]
        public void Write_bx()
        {
            ax = 0x1234;

            // MOV BX, AX
            emit("db 089h, 0c0h + (0 << 3) + 3");
            run();

            Assert.AreEqual(bx, 0x1234);
        }

        [TestMethod]
        public void Write_sp()
        {
            ax = 0x1234;

            // MOV SP,AX
            emit("db 089h, 0c0h + (0 << 3) + 4");
            run();

            Assert.AreEqual(sp, 0x1234);
        }

        [TestMethod]
        public void Write_bp()
        {
            ax = 0x1234;

            // MOV BP,AX
            emit("db 089h, 0c0h + (0 << 3) + 5");
            run();

            Assert.AreEqual(bp, 0x1234);
        }

        [TestMethod]
        public void Write_si()
        {
            ax = 0x1234;

            // MOV SI, AX
            emit("db 089h, 0c0h + (0 << 3) + 6");
            run();

            Assert.AreEqual(si, 0x1234);
        }

        [TestMethod]
        public void Write_di()
        {
            ax = 0x1234;

            // MOV DI,AX
            emit("db 089h, 0c0h + (0 << 3) + 7");
            run();

            Assert.AreEqual(di, 0x1234);
        }

        #endregion

        #region Segment overrides
        [TestMethod]
        public void Write_es_bx()
        {
            ax = 0x1234;

            es = 0x0800;
            bx = 0x0010;

            emit("mov [es:bx],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8010), 0x1234);
        }


        [TestMethod]
        public void Write_ss_bx()
        {
            ax = 0x1234;

            ss = 0x0800;
            bx = 0x0010;

            emit("mov [ss:bx],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8010), 0x1234);
        }

        [TestMethod]
        public void Write_es_bp_di()
        {
            ax = 0x1234;

            es = 0x0800;
            bp = 0x0100;
            di = 0x0010;

            emit("mov [es:bp+di],ax");
            run();

            Assert.AreEqual(ReadWord(0, 0x8110), 0x1234);
        }
        #endregion
    }
}
