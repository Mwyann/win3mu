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


    [TestClass]
    public class FileNameTests 
    {
        [TestMethod]
        public void ValidCharacters()
        {
            Assert.IsTrue(DosPath.IsValidCharacters("ABCDEFGHIJKLMNOPQRSTUVWYXZabcdefghijklmnopqrstuvwyxz0123456789!#$%&'()-@^_`{}~\xFE\x80"));
        }

        [TestMethod]
        public void InvalidCharacters()
        {
            Assert.IsFalse(DosPath.IsValidCharacters("\x7F"));
            Assert.IsFalse(DosPath.IsValidCharacters("\""));
            Assert.IsFalse(DosPath.IsValidCharacters("*"));
            Assert.IsFalse(DosPath.IsValidCharacters("+"));
            Assert.IsFalse(DosPath.IsValidCharacters(","));
            Assert.IsFalse(DosPath.IsValidCharacters("/"));
            Assert.IsFalse(DosPath.IsValidCharacters(":"));
            Assert.IsFalse(DosPath.IsValidCharacters(";"));
            Assert.IsFalse(DosPath.IsValidCharacters("<"));
            Assert.IsFalse(DosPath.IsValidCharacters("="));
            Assert.IsFalse(DosPath.IsValidCharacters(">"));
            Assert.IsFalse(DosPath.IsValidCharacters("?"));
            Assert.IsFalse(DosPath.IsValidCharacters("\\"));
            Assert.IsFalse(DosPath.IsValidCharacters("["));
            Assert.IsFalse(DosPath.IsValidCharacters("]"));
            Assert.IsFalse(DosPath.IsValidCharacters("|"));
        }

        [TestMethod]
        public void DriveLetter()
        {
            Assert.IsTrue(DosPath.IsValidDriveLetterSpecification("A:"));
            Assert.IsTrue(DosPath.IsValidDriveLetterSpecification("C:"));
            Assert.IsTrue(DosPath.IsValidDriveLetterSpecification("Z:"));
            Assert.IsFalse(DosPath.IsValidDriveLetterSpecification("1:"));
            Assert.IsFalse(DosPath.IsValidDriveLetterSpecification("?:"));
        }

        [TestMethod]
        public void PathElement()
        {
            Assert.IsTrue(DosPath.IsValidElement("XYZ.123"));
            Assert.IsTrue(DosPath.IsValidElement("."));
            Assert.IsTrue(DosPath.IsValidElement(".."));
            Assert.IsFalse(DosPath.IsValidElement("XYZ.>"));
            Assert.IsFalse(DosPath.IsValidElement("XYZ.A.B"));
            Assert.IsTrue(DosPath.IsValidElement("12345678.123"));
            Assert.IsFalse(DosPath.IsValidElement("12345678.123x"));
            Assert.IsFalse(DosPath.IsValidElement("12345678x.123"));
        }

        [TestMethod]
        public void Path()
        {
            Assert.IsTrue(DosPath.IsValid(@"C:\DIRECTOR\FILENAME.TXT"));
            Assert.IsFalse(DosPath.IsValid(@"C:\DIRECTORY\FILENAME.TXT"));
            Assert.IsFalse(DosPath.IsValid(@"C:\DIRECTORY\\FILENAME.TXT"));
            Assert.IsTrue(DosPath.IsValid(@"C:\DIRECTOR.EXT\FILENAME.TXT"));
            Assert.IsTrue(DosPath.IsValid(@"c:\director.ext\filename.txt"));
            Assert.IsTrue(DosPath.IsValid(@"\DIRECTOR.EXT\FILENAME.TXT"));
            Assert.IsTrue(DosPath.IsValid(@"C:\DIRECTOR.EXT\"));
            Assert.IsTrue(DosPath.IsValid(@"C:\"));
            Assert.IsTrue(DosPath.IsValid(@"\"));
            Assert.IsTrue(DosPath.IsValid(@""));
        }
    }
}
