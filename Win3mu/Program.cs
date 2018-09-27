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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Win3muRuntime;

namespace Win3mu
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                MessageBox(IntPtr.Zero, "Usage: win3mu <programName> [/debug|/release] [/break] [/config:name]", "Win3mu", 0x10);
            }

            return API.Run(args[0], args.Skip(1).ToArray(), 1 /* SW_SHOWNORMAL */);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int MessageBox(IntPtr hWnd, string text, string caption, int options);
    }
}
                                                                                                      