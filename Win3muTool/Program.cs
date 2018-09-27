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
using System.Runtime.InteropServices;
using Win3muRuntime;

namespace Win3muTool
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
                return 0;

            try
            {
                if (args.Length > 0 && args[0] == "/register")
                {
                    API.Register();
                    return 0;
                }
                if (args.Length > 0 && args[0] == "/unregister")
                {
                    API.Unregister();
                    return 0;
                }

                // Get location of proxy
                var sourceStub = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location), "Win3muProxy.exe");
                if (!System.IO.File.Exists(sourceStub))
                {
                    throw new System.IO.FileNotFoundException(string.Format("Win3mu stub exe not found.\n\n{0}", sourceStub));
                }

                // Get the exe we're repacking
                var sourceExe = args[0];

                // Check it exists
                if (!System.IO.File.Exists(sourceExe))
                {
                    throw new System.IO.FileNotFoundException(string.Format("16-bit executable to be upgraded not found.\n\n{0}", sourceExe));
                }

                // Build it
                API.BuildStub(sourceStub, sourceExe, sourceExe, true);

                return 0;
            }
            catch (Exception x)
            {
                MessageBox(IntPtr.Zero, x.Message, "Win3mu", 0x10);
                return 7;
            }
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int MessageBox(IntPtr hWnd, string text, string caption, int options);
    }
}
