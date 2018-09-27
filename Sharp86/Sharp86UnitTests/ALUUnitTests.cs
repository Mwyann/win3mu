using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class ALUUnitTests
    {
        [TestInitialize]
        public void Init()
        {
            _alu = new ALU();
        }

        ALU _alu;

        uint Signed32(int s)
        {
            return unchecked((uint)s);
        }

        ushort Signed16(int s)
        {
            return unchecked((ushort)s);
        }

        byte Signed8(int s)
        {
            return unchecked((byte)s);
        }


        [TestMethod]
        public void alu_Add16_c()
        {
            Assert.AreEqual(_alu.Add16(10, 20), 30);
            Assert.IsFalse(_alu.FlagC);

            Assert.AreEqual(_alu.Add16(0x8010, 0x8020), 0x30);
            Assert.IsTrue(_alu.FlagC);
        }

        [TestMethod]
        public void alu_Add8_c()
        {
            Assert.AreEqual(_alu.Add8(10, 20), 30);
            Assert.IsFalse(_alu.FlagC);

            Assert.AreEqual(_alu.Add8(0x90, 0x90), 0x20);
            Assert.IsTrue(_alu.FlagC);
        }

        [TestMethod]
        public void alu_Add16_z()
        {
            Assert.AreEqual(_alu.Add16(10, 20), 30);
            Assert.IsFalse(_alu.FlagZ);

            Assert.AreEqual(_alu.Add16(0x8000, 0x8000), 0);
            Assert.IsTrue(_alu.FlagZ);
        }

        [TestMethod]
        public void alu_Add8_z()
        {
            Assert.AreEqual(_alu.Add8(10, 20), 30);
            Assert.IsFalse(_alu.FlagZ);

            Assert.AreEqual(_alu.Add8(0x80, 0x80), 0);
            Assert.IsTrue(_alu.FlagZ);
        }

        [TestMethod]
        public void alu_Add16_s()
        {
            Assert.AreEqual(_alu.Add16(10, 20), 30);
            Assert.IsFalse(_alu.FlagS);

            Assert.AreEqual(_alu.Add16(0x7000, 0x2000), 0x9000);
            Assert.IsTrue(_alu.FlagS);
        }

        [TestMethod]
        public void alu_Add8_s()
        {
            Assert.AreEqual(_alu.Add8(10, 20), 30);
            Assert.IsFalse(_alu.FlagS);

            Assert.AreEqual(_alu.Add8(0x70, 0x20), 0x90);
            Assert.IsTrue(_alu.FlagS);
        }

        [TestMethod]
        public void alu_Add16_o()
        {
            // Positive + positive
            Assert.AreEqual(_alu.Add16(0x7000, 0x7000), 0xE000);
            Assert.IsTrue(_alu.FlagO);

            // Negative + negative
            Assert.AreEqual(_alu.Add16(0x8000, 0xFFFF), 0x7FFF);
            Assert.IsTrue(_alu.FlagO);

            // Positive + positive
            Assert.AreEqual(_alu.Add16(0x7000, 0x50), 0x7050);
            Assert.IsFalse(_alu.FlagO);

            // Negative + negative
            Assert.AreEqual(_alu.Add16(0xFFFF, 0xFFFF), 0xFFFE);
            Assert.IsFalse(_alu.FlagO);

            // Negative + negative
            Assert.AreEqual(_alu.Add16(0x8001, 0xFFFF), 0x8000);
            Assert.IsFalse(_alu.FlagO);
        }

        [TestMethod]
        public void alu_Add8_o()
        {
            // Positive + positive
            Assert.AreEqual(_alu.Add8(0x70, 0x70), 0xE0);
            Assert.IsTrue(_alu.FlagO);

            // Negative + negative
            Assert.AreEqual(_alu.Add8(0x80, 0xFF), 0x7F);
            Assert.IsTrue(_alu.FlagO);

            // Positive + positive
            Assert.AreEqual(_alu.Add8(0x70, 0x5), 0x75);
            Assert.IsFalse(_alu.FlagO);

            // Negative + negative
            Assert.AreEqual(_alu.Add8(0xFF, 0xFF), 0xFE);
            Assert.IsFalse(_alu.FlagO);

            // Negative + negative
            Assert.AreEqual(_alu.Add8(0x81, 0xFF), 0x80);
            Assert.IsFalse(_alu.FlagO);
        }

        [TestMethod]
        public void alu_Sub16_c()
        {
            Assert.AreEqual(_alu.Sub16(10, 5), 5);
            Assert.IsFalse(_alu.FlagC);

            Assert.AreEqual(_alu.Sub16(5, 10), 0x10000 + 5 - 10);
            Assert.IsTrue(_alu.FlagC);
        }

        [TestMethod]
        public void alu_Sub16_o()
        {
            Assert.AreEqual(_alu.Sub16(10, 5), 5);
            Assert.IsFalse(_alu.FlagO);

            Assert.AreEqual(_alu.Sub16(10, 25), Signed16(-15));
            Assert.IsFalse(_alu.FlagO);

            Assert.AreEqual(_alu.Sub16(32766, Signed16(-1)), 32767);
            Assert.IsFalse(_alu.FlagO);

            Assert.AreEqual(_alu.Sub16(Signed16(-32767), 1), Signed16(-32768));
            Assert.IsFalse(_alu.FlagO);

            Assert.AreEqual(_alu.Sub16(32766, Signed16(-2)), 32768);
            Assert.IsTrue(_alu.FlagO);

            Assert.AreEqual(_alu.Sub16(Signed16(-32767), 2), Signed16(-32769));
            Assert.IsTrue(_alu.FlagO);
        }

        [TestMethod]
        public void alu_Sub8_o()
        {
            Assert.AreEqual(_alu.Sub8(10, 5), 5);
            Assert.IsFalse(_alu.FlagO);

            Assert.AreEqual(_alu.Sub8(10, 25), Signed8(-15));
            Assert.IsFalse(_alu.FlagO);

            Assert.AreEqual(_alu.Sub8(126, Signed8(-1)), 127);
            Assert.IsFalse(_alu.FlagO);

            Assert.AreEqual(_alu.Sub8(Signed8(-127), 1), Signed8(-128));
            Assert.IsFalse(_alu.FlagO);

            Assert.AreEqual(_alu.Sub8(126, Signed8(-2)), 128);
            Assert.IsTrue(_alu.FlagO);

            Assert.AreEqual(_alu.Sub8(Signed8(-127), 2), Signed8(-129));
            Assert.IsTrue(_alu.FlagO);
        }

        [TestMethod]
        public void alu_Inc16_z()
        {
            Assert.AreEqual(_alu.Inc16(0xFFFF), 0);
            Assert.IsTrue(_alu.FlagZ);

            Assert.AreEqual(_alu.Inc16(0), 1);
            Assert.IsFalse(_alu.FlagZ);
        }

        [TestMethod]
        public void alu_Inc16_c()
        {
            _alu.FlagC = true;
            Assert.AreEqual(_alu.Inc16(0xFFFF), 0);
            Assert.IsTrue(_alu.FlagC);

            _alu.FlagC = false;
            Assert.AreEqual(_alu.Inc16(0xFFFF), 0);
            Assert.IsFalse(_alu.FlagC);
        }

        [TestMethod]
        public void alu_Dec16_z()
        {
            Assert.AreEqual(_alu.Dec16(1), 0);
            Assert.IsTrue(_alu.FlagZ);

            Assert.AreEqual(_alu.Dec16(2), 1);
            Assert.IsFalse(_alu.FlagZ);
        }

        [TestMethod]
        public void alu_Dec16_c()
        {
            _alu.FlagC = true;
            Assert.AreEqual(_alu.Dec16(0), 0xFFFF);
            Assert.IsTrue(_alu.FlagC);

            _alu.FlagC = false;
            Assert.AreEqual(_alu.Dec16(0), 0xFFFF);
            Assert.IsFalse(_alu.FlagC);
        }


        [TestMethod]
        public void alu_Inc8_z()
        {
            Assert.AreEqual(_alu.Inc8(0xFF), 0);
            Assert.IsTrue(_alu.FlagZ);

            Assert.AreEqual(_alu.Inc8(0), 1);
            Assert.IsFalse(_alu.FlagZ);
        }

        [TestMethod]
        public void alu_Inc8_c()
        {
            _alu.FlagC = true;
            Assert.AreEqual(_alu.Inc8(0xFF), 0);
            Assert.IsTrue(_alu.FlagC);

            _alu.FlagC = false;
            Assert.AreEqual(_alu.Inc8(0xFF), 0);
            Assert.IsFalse(_alu.FlagC);
        }

        [TestMethod]
        public void alu_Dec8_z()
        {
            Assert.AreEqual(_alu.Dec8(1), 0);
            Assert.IsTrue(_alu.FlagZ);

            Assert.AreEqual(_alu.Dec8(2), 1);
            Assert.IsFalse(_alu.FlagZ);
        }

        [TestMethod]
        public void alu_Dec8_c()
        {
            _alu.FlagC = true;
            Assert.AreEqual(_alu.Dec8(0), 0xFF);
            Assert.IsTrue(_alu.FlagC);

            _alu.FlagC = false;
            Assert.AreEqual(_alu.Dec8(0), 0xFF);
            Assert.IsFalse(_alu.FlagC);
        }

        [TestMethod]
        public void alu_Rol16()
        {
            Assert.AreEqual(_alu.Rol16(0x8000, 2), 2);
            Assert.IsFalse(_alu.FlagC);

            Assert.AreEqual(_alu.Rol16(0x8000, 1), 1);
            Assert.IsTrue(_alu.FlagC);
        }

        [TestMethod]
        public void alu_Ror16()
        {
            Assert.AreEqual(_alu.Ror16(1, 2), 0x4000);
            Assert.IsFalse(_alu.FlagC);

            Assert.AreEqual(_alu.Ror16(1, 1), 0x8000);
            Assert.IsTrue(_alu.FlagC);
        }

        [TestMethod]
        public void alu_Rcl16()
        {
            _alu.FlagC = false;
            Assert.AreEqual(_alu.Rcl16(1, 2), 1 << 2);
            Assert.IsFalse(_alu.FlagC);

            Assert.AreEqual(_alu.Rcl16(0x8000, 1), 0);
            Assert.IsTrue(_alu.FlagC);

            _alu.FlagC = false;
            Assert.AreEqual(_alu.Rcl16(0x8000, 2), 1);
            Assert.IsFalse(_alu.FlagC);

            _alu.FlagC = true;
            Assert.AreEqual(_alu.Rcl16(0, 1), 1);
            Assert.IsFalse(_alu.FlagC);
        }

        [TestMethod]
        public void alu_Rcr16()
        {
            _alu.FlagC = false;
            Assert.AreEqual(_alu.Rcr16(1, 2), 0x8000);
            Assert.IsFalse(_alu.FlagC);

            Assert.AreEqual(_alu.Rcr16(1, 1), 0);
            Assert.IsTrue(_alu.FlagC);

            Assert.AreEqual(_alu.Rcr16(0, 1), 0x8000);
            Assert.IsFalse(_alu.FlagC);
        }

        [TestMethod]
        public void alu_Rcl8()
        {
            _alu.FlagC = false;
            Assert.AreEqual(_alu.Rcl8(1, 2), 1 << 2);
            Assert.IsFalse(_alu.FlagC);

            Assert.AreEqual(_alu.Rcl8(0x80, 1), 0);
            Assert.IsTrue(_alu.FlagC);

            _alu.FlagC = false;
            Assert.AreEqual(_alu.Rcl8(0x80, 2), 1);
            Assert.IsFalse(_alu.FlagC);

            _alu.FlagC = true;
            Assert.AreEqual(_alu.Rcl8(0, 1), 1);
            Assert.IsFalse(_alu.FlagC);
        }

        [TestMethod]
        public void alu_Rcr8()
        {
            _alu.FlagC = false;
            Assert.AreEqual(_alu.Rcr8(1, 2), 0x80);
            Assert.IsFalse(_alu.FlagC);

            Assert.AreEqual(_alu.Rcr8(1, 1), 0);
            Assert.IsTrue(_alu.FlagC);

            Assert.AreEqual(_alu.Rcr8(0, 1), 0x80);
            Assert.IsFalse(_alu.FlagC);
        }

        [TestMethod]
        public void alu_Shl16()
        {
            Assert.AreEqual(_alu.Shl16(1, 2), 1 << 2);
            Assert.IsFalse(_alu.FlagC);
            Assert.IsFalse(_alu.FlagZ);

            Assert.AreEqual(_alu.Shl16(0x4000, 2), 0);
            Assert.IsTrue(_alu.FlagC);
            Assert.IsTrue(_alu.FlagZ);
        }

        [TestMethod]
        public void alu_Shr16()
        {
            Assert.AreEqual(_alu.Shr16(1 << 5, 4), 1 << 1);
            Assert.IsFalse(_alu.FlagC);
            Assert.IsFalse(_alu.FlagZ);
        }

        [TestMethod]
        public void alu_Sar16()
        {
            Assert.AreEqual(_alu.Sar16(Signed16(-40), 1), Signed16(-20));
        }

        [TestMethod]
        public void alu_Aaa()
        {
            _alu.FlagA = false;
            Assert.AreEqual(_alu.Aaa(0x0201), 0x0201);
            Assert.IsFalse(_alu.FlagA);
            Assert.IsFalse(_alu.FlagC);

            _alu.FlagA = true;
            Assert.AreEqual(_alu.Aaa(0x0201), 0x0307);
            Assert.IsTrue(_alu.FlagA);
            Assert.IsTrue(_alu.FlagC);

            _alu.FlagA = false;
            Assert.AreEqual(_alu.Aaa(0x020b), 0x301);
            Assert.IsTrue(_alu.FlagA);
            Assert.IsTrue(_alu.FlagC);
        }

        [TestMethod]
        public void alu_Aad()
        {
            Assert.AreEqual(_alu.Aad(0x0203, 10), 23);
            Assert.IsFalse(_alu.FlagS);
        }

        [TestMethod]
        public void alu_Cbw()
        {
            Assert.AreEqual(_alu.Cbw(0xFF), 0xFFFF);
            Assert.AreEqual(_alu.Cbw(0x0F), 0x000F);
        }

        [TestMethod]
        public void alu_Cwd()
        {
            Assert.AreEqual(_alu.Cwd(0xFFFF), 0xFFFFFFFFU);
            Assert.AreEqual(_alu.Cwd(0x000F), 0x0000000FU);
        }

        [TestMethod]
        public void alu_Div16()
        {
            Assert.AreEqual(_alu.Div16(60005, 200), (uint)(300 | (5 << 16)));
        }

        [TestMethod]
        public void alu_Div8()
        {
            Assert.AreEqual(_alu.Div8(205, 20), 10 | 5 << 8);
        }

        [TestMethod]
        public void alu_IDiv16()
        {
            Assert.AreEqual(_alu.IDiv16(Signed32(-60005), 200), (uint)(Signed16(-300) | (Signed16(-5) << 16)));
        }

        [TestMethod]
        public void alu_IDiv8()
        {
            Assert.AreEqual(_alu.IDiv8(Signed16(-6005), 100), (uint)(Signed8(-60) | (Signed8(-5) << 8)));
        }

        [TestMethod]
        public void alu_Mul16()
        {
            Assert.AreEqual(_alu.Mul16(400, 50), 20000U);
            Assert.IsFalse(_alu.FlagC);
            Assert.IsFalse(_alu.FlagO);

            Assert.AreEqual(_alu.Mul16(40000, 50000), 2000000000U);
            Assert.IsTrue(_alu.FlagC);
            Assert.IsTrue(_alu.FlagO);
        }

        [TestMethod]
        public void alu_Mul8()
        {
            Assert.AreEqual(_alu.Mul8(4, 5), 20U);
            Assert.IsFalse(_alu.FlagC);
            Assert.IsFalse(_alu.FlagO);

            Assert.AreEqual(_alu.Mul8(200, 80), 16000U);
            Assert.IsTrue(_alu.FlagC);
            Assert.IsTrue(_alu.FlagO);
        }

        [TestMethod]
        public void alu_IMul16()
        {
            Assert.AreEqual(_alu.IMul16(Signed16(-200), Signed16(50)), Signed32(-200*50));
            Assert.IsFalse(_alu.FlagC);
            Assert.IsFalse(_alu.FlagO);

            Assert.AreEqual(_alu.IMul16(Signed16(-300), Signed16(500)), Signed32(-300 * 500));
            Assert.IsTrue(_alu.FlagC);
            Assert.IsTrue(_alu.FlagO);
        }

        [TestMethod]
        public void alu_IMul8()
        {
            Assert.AreEqual(_alu.IMul8(Signed8(-2), Signed8(5)), Signed16(-10));
            Assert.IsFalse(_alu.FlagC);
            Assert.IsFalse(_alu.FlagO);

            Assert.AreEqual(_alu.IMul8(Signed8(-3), Signed8(50)), Signed16(-150));
            Assert.IsTrue(_alu.FlagC);
            Assert.IsTrue(_alu.FlagO);
        }

        [TestMethod]
        public void alu_Neg16()
        {
            Assert.AreEqual(_alu.Neg16(Signed16(-10)), Signed16(10));
            Assert.IsTrue(_alu.FlagC);
            Assert.IsFalse(_alu.FlagZ);

            Assert.AreEqual(_alu.Neg16(Signed16(10)), Signed16(-10));
            Assert.IsTrue(_alu.FlagC);
            Assert.IsFalse(_alu.FlagZ);

            Assert.AreEqual(_alu.Neg16(Signed16(0)), Signed16(0));
            Assert.IsFalse(_alu.FlagC);
            Assert.IsTrue(_alu.FlagZ);
        }

        [TestMethod]
        public void alu_Neg8()
        {
            Assert.AreEqual(_alu.Neg8(Signed8(-10)), Signed8(10));
            Assert.IsTrue(_alu.FlagC);
            Assert.IsFalse(_alu.FlagZ);

            Assert.AreEqual(_alu.Neg8(Signed8(10)), Signed8(-10));
            Assert.IsTrue(_alu.FlagC);
            Assert.IsFalse(_alu.FlagZ);

            Assert.AreEqual(_alu.Neg8(Signed8(0)), Signed8(0));
            Assert.IsFalse(_alu.FlagC);
            Assert.IsTrue(_alu.FlagZ);
        }

        [TestMethod]
        public void alu_Not16()
        {
            Assert.AreEqual(_alu.Not16(0xAAAA), 0x5555U);
        }

        [TestMethod]
        public void alu_Not8()
        {
            Assert.AreEqual(_alu.Not8(0xAA), 0x55U);
        }
    }
}
