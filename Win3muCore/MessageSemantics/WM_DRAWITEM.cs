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
    class WM_DRAWITEM : Callable
    {
        public override uint Call32from16(Machine machine, bool hook, bool dlgproc, ref Win16.MSG msg16, ref Win32.MSG msg32, Func<IntPtr> callback)
        {
            var di16 = machine.ReadStruct<Win16.DRAWITEMSTRUCT>(msg16.lParam);
            unsafe
            {
                Win16.DRAWITEMSTRUCT* ptr = &di16;
                msg32.wParam = (IntPtr)msg16.wParam;
                msg32.lParam = (IntPtr)ptr;
                var retv = callback();
                return (uint)retv.ToInt32();
            }
        }

        public override IntPtr Call16from32(Machine machine, bool hook, bool dlgproc, ref Win32.MSG msg32, ref Win16.MSG msg16, Func<uint> callback)
        {
            var di32 = Marshal.PtrToStructure<Win32.DRAWITEMSTRUCT>(msg32.lParam);
            var di16 = di32.Convert();
            var saveSP = machine.sp;
            
            try
            {
                // NB: This needs to be on stack - Wordzap incorrectly uses near pointer from lParam and won't work 
                //     if drawitemstruct is in a different segment (see red bar in Skill -> Handcap, comms options)
                var ptr = machine.StackAlloc(di16);
                msg16.wParam = (ushort)msg32.wParam.ToInt32();
                msg16.lParam = ptr;
                var retv = callback();
                return (IntPtr)retv;
            }
            finally
            {
                machine.sp = saveSP;
            }
        }
    }
}
