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
    public class GlobTest
    {
        [TestMethod]
        public void glob_exact_match()
        {
            Assert.IsTrue(DosPath.GlobMatch("abc.def", "abc.def"));
        }

        [TestMethod]
        public void glob_exact_mismatch()
        {
            Assert.IsFalse(DosPath.GlobMatch("abd.def", "abc.def"));
        }

        [TestMethod]
        public void glob_case_test()
        {
            Assert.IsTrue(DosPath.GlobMatch("abc.def", "ABC.DEF"));
        }

        [TestMethod]
        public void glob_star()
        {
            Assert.IsTrue(DosPath.GlobMatch("*", "abc.def"));
        }

        [TestMethod]
        public void glob_star_dot_star()
        {
            Assert.IsTrue(DosPath.GlobMatch("*.*", "abc.def"));
        }


        [TestMethod]
        public void glob_question()
        {
            Assert.IsTrue(DosPath.GlobMatch("ab?.???", "abc.def"));
        }

        [TestMethod]
        public void glob_question_star()
        {
            Assert.IsTrue(DosPath.GlobMatch("ab?d*.*", "abcd.def"));
            Assert.IsFalse(DosPath.GlobMatch("ab??d*.*", "abcd.def"));
        }

        [TestMethod]
        public void glob_star_dot_ext()
        {
            Assert.IsTrue(DosPath.GlobMatch("*.doc", "abcd.doc"));
            Assert.IsFalse(DosPath.GlobMatch("*.exe", "abcd.doc"));
        }

        [TestMethod]
        public void glob_redundant()
        {
            Assert.IsTrue(DosPath.GlobMatch("*?.doc", "abcd.doc"));
            Assert.IsTrue(DosPath.GlobMatch("*?***.doc", "abcd.doc"));
            Assert.IsTrue(DosPath.GlobMatch("abcd.doc*", "abcd.doc"));
            Assert.IsTrue(DosPath.GlobMatch("abcd.doc?", "abcd.doc"));
        }

        [TestMethod]
        public void glob_tail()
        {
            Assert.IsFalse(DosPath.GlobMatch("*.c", "abc.cur"));
        }


    }
}
