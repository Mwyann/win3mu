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
    class WM_NCCALCSIZE : Callable
    {
        public override uint Call32from16(Machine machine, bool hook, bool dlgproc, ref Win16.MSG msg16, ref Win32.MSG msg32, Func<IntPtr> callback)
        {
            unsafe
            {
                var rc32 = machine.ReadStruct<Win16.RECT>(msg16.lParam).Convert();

                msg32.wParam = IntPtr.Zero;
                msg32.lParam = new IntPtr(&rc32);
                var ret = callback();
                machine.WriteStruct(msg16.lParam, rc32.Convert());
                return (uint)ret;
            }
        }

        public override IntPtr Call16from32(Machine machine, bool hook, bool dlgproc, ref Win32.MSG msg32, ref Win16.MSG msg16, Func<uint> callback)
        {
            if (msg32.wParam != IntPtr.Zero)
            {
                // 16bit windows didn't support this
                if (!hook)
                    return User.DefWindowProc(msg32.hWnd, msg32.message, msg32.wParam, msg32.lParam);
                return IntPtr.Zero;
            }
            else
            {

                var rc = Marshal.PtrToStructure<Win32.RECT>(msg32.lParam);
                var ptr = machine.SysAlloc(rc.Convert());
                msg16.wParam = 0;
                msg16.lParam = ptr;
                var ret = callback();
                rc = machine.SysReadAndFree<Win16.RECT>(ptr).Convert();
                Marshal.StructureToPtr(rc, msg32.lParam, true);
                return IntPtr.Zero;
            }

        }                                                
    }
}
