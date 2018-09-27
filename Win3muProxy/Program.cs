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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Win3muProxy
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                // Get location of Win3muCore.dll
                var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Win3mu");
                if (key== null)
                    throw new InvalidOperationException("Failed to open registry key");

                var location = key.GetValue("Location") as string;
                if (string.IsNullOrEmpty(location))
                    throw new InvalidOperationException("Can't locate Win3muCore.dll - please re-install");

                // Find the Run method
                var core = Assembly.LoadFrom(System.IO.Path.Combine(location, "Win3muRuntime.dll"));
                var API = core.GetType("Win3muRuntime.API");
                var Run = (Func<string, string[], int, int>)API.GetMethod("GetRunMethod").Invoke(null, null);

                // Work out params

                var program = System.IO.Path.ChangeExtension(typeof(Program).Assembly.Location, ".exe16");
                var showWindow = 1;

                // Run it
                return Run(program, args, showWindow);
            }
            catch (Exception x)
            {
                MessageBox(IntPtr.Zero, x.Message, "Win3mu Failed", 0x10);
                return 7;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int MessageBox(IntPtr hWnd, string text, string caption, int options);
    }
}
