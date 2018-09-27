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
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Win3muCore;

namespace Win3muCoreUnitTests
{
    /*
    [TestClass]
    public class PathMapperTests 
    {
        [TestInitialize]
        public void Setup()
        {
            _pm = new PathMapper(null);
            _pm.AddMount(@"C:\WINDOWS", @"C:\Program Files\WowBox\Files\Windows", @"D:\Users\Blah\WowBox\Files\Windows");
            _pm.AddMount(@"C:\$(AppName)", @"$(AppFolder)", @"D:\Users\Blah\WowBox\Files\$(AppName)");
            _pm.Prepare();
        }

        PathMapper _pm;

        [TestMethod]
        public void HostToGuest()
        {
            Assert.AreEqual(_pm.MapHostToGuest(@"C:\Program Files\WowBox\Files\Windows", false), @"C:\WINDOWS");
            Assert.AreEqual(_pm.MapHostToGuest(@"C:\Program Files\WowBox\Files\Windows\system\krnl286.exe", false), @"C:\WINDOWS\system\krnl286.exe");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidHostToGuest()
        {
            _pm.MapHostToGuest(@"C:\Program Files\WowBox\Files\Windows\thisnamecantbemapped.extension", false);
        }

        [TestMethod]
        public void GuestToHost()
        {
            Assert.AreEqual(_pm.MapGuestToHost(@"C:\WINDOWS", false), @"C:\Program Files\WowBox\Files\Windows");
            Assert.AreEqual(_pm.MapGuestToHost(@"C:\WINDOWS\system\krnl286.exe", false), @"C:\Program Files\WowBox\Files\Windows\system\krnl286.exe");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UnmappedGuestToHost()
        {
            _pm.MapGuestToHost(@"C:\unknown\blah", false);
        }

        [TestMethod]
        public void AppNameRead()
        {
            Assert.AreEqual(_pm.MapGuestToHost(@"C:\CLINK2\CLINK2.EXE", false), @"C:\Program Files\Games\CLINK2.EXE");
            Assert.AreEqual(_pm.MapHostToGuest(@"C:\Program Files\Games\CLINK2.EXE", false), @"C:\Clink2\CLINK2.EXE");
        }

        [TestMethod]
        public void AppNameWrite()
        {
            Assert.AreEqual(_pm.MapGuestToHost(@"C:\CLINK2\CLINK2.DAT", true), @"D:\Users\Blah\WowBox\Files\Clink2\CLINK2.DAT");
            Assert.AreEqual(_pm.MapHostToGuest(@"D:\Users\Blah\WowBox\Files\Clink2\CLINK2.DAT", true), @"C:\Clink2\CLINK2.DAT");
        }


    }
*/
}
