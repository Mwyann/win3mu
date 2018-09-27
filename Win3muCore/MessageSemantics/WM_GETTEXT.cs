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
    class WM_GETTEXT : Callable
    {
        public override uint Call32from16(Machine machine, bool hook, bool dlgproc, ref Win16.MSG msg16, ref Win32.MSG msg32, Func<IntPtr> callback)
        {
            unsafe
            {
                var buf = new char[msg16.wParam];

                fixed (char* psz = buf)
                {
                    msg32.wParam = (IntPtr)msg16.wParam;
                    msg32.lParam = (IntPtr)psz;
                    var len = callback().ToInt32();
                    if (len >= 0)
                    {
                        var str = new String(psz, 0, len);
                        return machine.WriteString(msg16.lParam, str, msg16.wParam);
                    }
                    else
                    {
                        return (uint)(len);
                    }
                }
            }
        }

        public override IntPtr Call16from32(Machine machine, bool hook, bool dlgproc, ref Win32.MSG msg32, ref Win16.MSG msg16, Func<uint> callback)
        {
            var ptr = machine.SysAlloc(msg32.wParam);
            msg16.wParam = msg32.wParam.ToInt32().Loword();
            msg16.lParam = ptr;
            var retv = callback();
            if (retv >= 0)
            {
                var str = machine.ReadString(ptr);
                var unibytes = Encoding.Unicode.GetBytes(str);
                Marshal.Copy(unibytes, 0, msg32.lParam, Math.Min((int)msg32.wParam, str.Length * 2));
            }
            machine.SysFree(ptr);
            return (IntPtr)retv;
        }
    }
}
