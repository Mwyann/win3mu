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
    class WM_NEXTMENU : Callable
    {
        public override IntPtr Call16from32(Machine machine, bool hook, bool dlgproc, ref Win32.MSG msg32, ref Win16.MSG msg16, Func<uint> callback)
        {
            // Convert info
            var info32 = Marshal.PtrToStructure<Win32.MDINEXTMENU>(msg32.lParam);
            var info16 = info32.Convert();

            // Setup message
            msg16.wParam = msg32.lParam.Loword();
            msg16.lParam = machine.SysAlloc(info16);

            try
            {
                // Do it
                return BitUtils.DWordToIntPtr(callback());
            }
            finally
            {
                // Clean up
                machine.SysFree(msg16.lParam);
            }
        }

        public override uint Call32from16(Machine machine, bool hook, bool dlgproc, ref Win16.MSG msg16, ref Win32.MSG msg32, Func<IntPtr> callback)
        {
            // Convert info
            var info16 = machine.ReadStruct<Win16.MDINEXTMENU>(msg16.lParam);
            var info32 = info16.Convert();

            unsafe
            {
                // Setup message
                msg32.wParam = (IntPtr)msg16.wParam;
                msg32.lParam = (IntPtr)(&info32);

                // Do it
                return callback().DWord();
            }
        }

    }
}
