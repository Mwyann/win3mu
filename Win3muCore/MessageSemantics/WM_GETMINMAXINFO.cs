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

namespace Win3muCore.MessageSemantics
{
    class WM_GETMINMAXINFO : Callable
    {
        public override uint Call32from16(Machine machine, bool hook, bool dlgproc, ref Win16.MSG msg16, ref Win32.MSG msg32, Func<IntPtr> callback)
        {
            unsafe
            {
                var mmi32 = machine.ReadStruct<Win16.MINMAXINFO>(msg16.lParam).Convert();
                msg32.wParam = IntPtr.Zero;
                msg32.lParam = new IntPtr(&mmi32);
                var ret = callback();
                machine.WriteStruct(msg16.lParam, mmi32.Convert());
                return (uint)ret;
            }
        }
        public override IntPtr Call16from32(Machine machine, bool hook, bool dlgproc, ref Win32.MSG msg32, ref Win16.MSG msg16, Func<uint> callback)
        {
            // Get the Win32 struct
            var mmi = Marshal.PtrToStructure<Win32.MINMAXINFO>(msg32.lParam);

            // Call 16 bit function
            var ptr = machine.SysAlloc(mmi.Convert());
            msg16.wParam = 0;
            msg16.lParam = ptr;
            var ret = callback();
            mmi = machine.SysReadAndFree<Win16.MINMAXINFO>(ptr).Convert();

            // Return it to Win32
            Marshal.StructureToPtr(mmi, msg32.lParam, true);
            return IntPtr.Zero;
        }
    }
}
