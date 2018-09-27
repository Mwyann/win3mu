/*
Win3mu - Windows 3 Emulator
Copyright (C) 2017 Topten Software.

Win3mu is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Win3mu is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Win3mu.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Win3muCore;

namespace Win3muCoreUnitTests
{
    [TestClass]
    public class BitUtilsTest
    {
        [TestMethod]
        public void uint_loword()
        {
            uint x = 0x12345678U;
            Assert.AreEqual(x.Loword(), (ushort)0x5678);

            x = 0x12348989U;
            Assert.AreEqual(x.Loword(), (ushort)0x8989);
        }

        [TestMethod]
        public void uint_hiword()
        {
            uint x = 0x12345678U;
            Assert.AreEqual(x.Hiword(), (ushort)0x1234);

            x = 0x82348989U;
            Assert.AreEqual(x.Hiword(), (ushort)0x8234);
        }

        [TestMethod]
        public void IntPtr_loword()
        {
            var x = (IntPtr)0x12345678U;
            Assert.AreEqual(x.Loword(), (ushort)0x5678);

            x = (IntPtr)0x12348989U;
            Assert.AreEqual(x.Loword(), (ushort)0x8989);
        }

        [TestMethod]
        public void IntPtr_hiword()
        {
            var x = (IntPtr)0x12345678U;
            Assert.AreEqual(x.Hiword(), (ushort)0x1234);

            x = (IntPtr)unchecked((int)0x82348989U);
            Assert.AreEqual(x.Hiword(), (ushort)0x8234);
        }

        [TestMethod]
        public void IntPtr_dword()
        {
            var x = (IntPtr)0x12345678U;
            Assert.AreEqual(x.DWord(), (uint)0x12345678);

            x = BitUtils.DWordToIntPtr(0x82348989U);
            Assert.AreEqual(x.DWord(), (uint)0x82348989);
        }

        [TestMethod]
        public void make_intptr()
        {
            Assert.AreEqual(BitUtils.MakeIntPtr(0x1234, 0x5678), (IntPtr)0x56781234U);
            Assert.AreEqual(BitUtils.MakeIntPtr(0x1234, 0x8678), (IntPtr)unchecked((int)0x86781234));
        }

    }
}
