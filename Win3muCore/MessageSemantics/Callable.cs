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
using Sharp86;

namespace Win3muCore.MessageSemantics
{
    abstract class Callable : Base
    {
        public Callable()
        {
        }

        public override bool ShouldBypass(Machine machine, ref Win32.MSG msg)
        {
            return false;
        }

        public abstract uint Call32from16(Machine machine, bool hook, bool dlgproc, ref Win16.MSG msg16, ref Win32.MSG msg32, Func<IntPtr> callback);
        public abstract IntPtr Call16from32(Machine machine, bool hook, bool dlgproc, ref Win32.MSG msg32, ref Win16.MSG msg16, Func<uint> callback);

        /*
        public abstract uint CallWndProc32from16(Machine machine, Win32.WNDPROC pfnProc, ushort hWnd, ushort message, ushort wParam, uint lParam);
        public abstract IntPtr CallWndProc16from32(Machine machine, uint pfnProc, IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam, bool dlgProc);
        */
    }


    class unused_string : Callable
    {
        public override uint Call32from16(Machine machine, bool hook, bool dlgproc, ref Win16.MSG msg16, ref Win32.MSG msg32, Func<IntPtr> callback)
        {
            unsafe
            {
                var str = machine.ReadString(msg16.lParam);
                fixed (char* psz = str)
                {
                    msg32.wParam = IntPtr.Zero;
                    msg32.lParam = (IntPtr)psz;
                    return (uint)(callback());
                }
            }
        }

        public override IntPtr Call16from32(Machine machine, bool hook, bool dlgproc, ref Win32.MSG msg32, ref Win16.MSG msg16, Func<uint> callback)
        {
            // Get the string
            string text = null;
            if (msg32.lParam != IntPtr.Zero)
            {
                text = Marshal.PtrToStringUni(msg32.lParam);
            }

            // Allocate string, call 16 and free
            var ptr = machine.SysAllocString(text);
            msg16.wParam = 0;
            msg16.lParam = ptr;
            var retv = callback();
            machine.SysFree(ptr);

            return (IntPtr)retv;
        }

        public static unused_string Instance = new unused_string();
    }

    class copy_string : Callable
    {
        public override uint Call32from16(Machine machine, bool hook, bool dlgproc, ref Win16.MSG msg16, ref Win32.MSG msg32, Func<IntPtr> callback)
        {
            unsafe
            {
                var str = machine.ReadString(msg16.lParam);
                fixed (char* psz = str)
                {
                    msg32.wParam = (IntPtr)msg16.wParam;
                    msg32.lParam = (IntPtr)psz;
                    return (uint)(callback());
                }
            }
        }

        public override IntPtr Call16from32(Machine machine, bool hook, bool dlgproc, ref Win32.MSG msg32, ref Win16.MSG msg16, Func<uint> callback)
        {
            // Get the string
            string text = null;
            if (msg32.lParam != IntPtr.Zero)
            {
                text = Marshal.PtrToStringUni(msg32.lParam);
            }

            // Allocate string, call 16 and free
            var ptr = machine.SysAllocString(text);
            msg16.wParam = msg32.wParam.Loword();
            msg16.lParam = ptr;
            var retv = callback();
            machine.SysFree(ptr);

            return (IntPtr)retv;
        }

        public static unused_string Instance = new unused_string();
    }

    class copy_outstring : Callable
    {
        public override uint Call32from16(Machine machine, bool hook, bool dlgproc, ref Win16.MSG msg16, ref Win32.MSG msg32, Func<IntPtr> callback)
        {
            var ptr = Marshal.AllocHGlobal(0xFFFF);
            Marshal.WriteInt16(ptr, 0, 0);

            msg32.wParam = (IntPtr)msg16.wParam;
            msg32.lParam = (IntPtr)ptr;
            var retv = callback();
            machine.WriteString(msg16.lParam, Marshal.PtrToStringUni(ptr), 0xFFFF);
            Marshal.FreeHGlobal(ptr);
            return retv.DWord();
        }

        public override IntPtr Call16from32(Machine machine, bool hook, bool dlgproc, ref Win32.MSG msg32, ref Win16.MSG msg16, Func<uint> callback)
        {
            // Allocate ememory
            var sel = machine.GlobalHeap.Alloc("temp", 0, 0xFFFF);
            var ptr = BitUtils.MakeDWord(0, sel);

            // Call 16-bit 
            msg16.wParam = msg32.wParam.Loword();
            msg16.lParam = ptr;
            var retv = callback();

            // Get the string
            var str = machine.ReadString(ptr);
            machine.GlobalHeap.Free(sel);

            // Write string back (Ugh!)
            for (int i=0; i<str.Length; i++)
            {
                Marshal.WriteInt16(msg32.lParam, i * 2, str[i]);
            }
            Marshal.WriteInt16(msg32.lParam, str.Length * 2, 0);

            // Done
            return (IntPtr)retv;
        }
    }



}
