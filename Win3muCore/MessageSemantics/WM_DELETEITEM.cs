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
    class WM_DELETEITEM : Callable
    {
        public override uint Call32from16(Machine machine, bool hook, bool dlgproc, ref Win16.MSG msg16, ref Win32.MSG msg32, Func<IntPtr> callback)
        {
            unsafe
            {
                // Convert
                var mi16 = machine.ReadStruct<Win16.DELETEITEMSTRUCT>(msg16.lParam);
                var mi32 = mi16.Convert();

                // Call 
                msg32.wParam = (IntPtr)msg16.wParam;
                msg32.lParam = (IntPtr)(&mi32);
                var retv = callback();

                // Convert back
                mi16 = mi32.Convert();
                machine.WriteStruct(msg16.lParam, mi16);

                return (uint)retv.ToInt32();
            }
        }

        public override IntPtr Call16from32(Machine machine, bool hook, bool dlgproc, ref Win32.MSG msg32, ref Win16.MSG msg16, Func<uint> callback)
        {
            var saveSP = machine.sp;
            
            try
            {
                // Convert
                var mi32 = Marshal.PtrToStructure<Win32.DELETEITEMSTRUCT>(msg32.lParam);
                var mi16 = mi32.Convert();

                var ptr = machine.StackAlloc(mi16);
                msg16.wParam = 0;
                msg16.lParam = ptr;
                var retv = callback();

                // Copy back
                mi32 = mi16.Convert();
                Marshal.StructureToPtr(mi32, msg32.lParam, false);

                return (IntPtr)retv;
            }
            finally
            {
                machine.sp = saveSP;
            }
        }
    }
}
