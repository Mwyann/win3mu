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
using System.Text;
using System.Threading.Tasks;

namespace Win3muCore.MessageSemantics
{
    abstract class Postable : Base
    {
        public override bool ShouldBypass(Machine machine, ref Win32.MSG msg)
        {
            return false;
        }

        public abstract void To32(Machine machine, ref Win16.MSG msg16, ref Win32.MSG msg32);
        public abstract void To16(Machine machine, ref Win32.MSG msg32, ref Win16.MSG msg16);
    }

    class packAndPost : Postable
    {
        static packAndPost()
        {
            WM_PACKANDPOST = User._RegisterWindowMessage("WIN3MU_PACKANDPOST");
        }

        public static uint WM_PACKANDPOST;

        public override void To32(Machine machine, ref Win16.MSG msg16, ref Win32.MSG msg32)
        {
            msg32.message = WM_PACKANDPOST;
            msg32.wParam = BitUtils.MakeIntPtr(msg16.wParam, msg16.message);
            msg32.lParam = (IntPtr)(int)msg16.lParam;
        }

        public override void To16(Machine machine, ref Win32.MSG msg32, ref Win16.MSG msg16)
        {
            msg16.message = msg32.wParam.Hiword();
            msg16.wParam = msg32.wParam.Loword();
            msg16.lParam = msg32.lParam.DWord();
        }

        public static packAndPost Instance = new packAndPost();
    }

    class unused : Postable
    {
        public override void To32(Machine machine, ref Win16.MSG msg16, ref Win32.MSG msg32)
        {
            msg32.wParam = IntPtr.Zero;
            msg32.lParam = IntPtr.Zero;
        }

        public override void To16(Machine machine, ref Win32.MSG msg32, ref Win16.MSG msg16)
        {
            msg16.wParam = 0;
            msg16.lParam = 0;
        }
    }

    class copy : Postable
    {
        public override void To32(Machine machine, ref Win16.MSG msg16, ref Win32.MSG msg32)
        {
            msg32.wParam = (IntPtr)msg16.wParam;
            msg32.lParam = (IntPtr)(int)msg16.lParam;
        }

        public override void To16(Machine machine, ref Win32.MSG msg32, ref Win16.MSG msg16)
        {
            msg16.wParam = msg32.wParam.Loword();
            msg16.lParam = msg32.lParam.ToUInt32();
        }

        public static copy Instance = new copy();
    }

    class notimpl : Postable
    {
        public override void To32(Machine machine, ref Win16.MSG msg16, ref Win32.MSG msg32)
        {
            throw new NotImplementedException($"Message type not implemented: {MessageNames.NameOfMessage(msg16.message)}");
        }

        public override void To16(Machine machine, ref Win32.MSG msg32, ref Win16.MSG msg16)
        {
            throw new NotImplementedException($"Message type not implemented: {MessageNames.NameOfMessage(msg16.message)}");
        }
    }

    class copy_zero : Postable
    {
        public override void To32(Machine machine, ref Win16.MSG msg16, ref Win32.MSG msg32)
        {
            if (msg16.lParam == 0)
            {
                msg32.wParam = (IntPtr)msg16.wParam;
                msg32.lParam = (IntPtr)(int)msg16.lParam;
            }
            else
                throw new NotImplementedException("lParam must be zero");
        }

        public override void To16(Machine machine, ref Win32.MSG msg32, ref Win16.MSG msg16)
        {
            if (msg32.lParam == IntPtr.Zero)
            {
                msg16.wParam = (ushort)msg32.wParam.ToInt32();
                msg16.lParam = 0;
            }
            else
            {
                return;
                throw new NotImplementedException("lParam must be zero");
            }
        }
    }

    class copy_unused : Postable
    {
        public override void To32(Machine machine, ref Win16.MSG msg16, ref Win32.MSG msg32)
        {
            msg32.wParam = (IntPtr)msg16.wParam;
            msg32.lParam = IntPtr.Zero;
        }

        public override void To16(Machine machine, ref Win32.MSG msg32, ref Win16.MSG msg16)
        {
            msg16.wParam = (ushort)msg32.wParam.ToInt32();
            msg16.lParam = 0;
        }
    }

    class hdc_unused : Postable
    {
        public override void To32(Machine machine, ref Win16.MSG msg16, ref Win32.MSG msg32)
        {
            msg32.wParam = HDC.Map.To32(msg16.wParam);
            msg32.lParam = IntPtr.Zero;
        }

        public override void To16(Machine machine, ref Win32.MSG msg32, ref Win16.MSG msg16)
        {
            msg16.wParam = HDC.Map.To16(msg32.wParam);
            msg16.lParam = 0;
        }
    }

    class hwnd_copy : Postable
    {
        public override void To32(Machine machine, ref Win16.MSG msg16, ref Win32.MSG msg32)
        {
            msg32.wParam = HWND.Map.To32(msg16.wParam);
            msg32.lParam = (IntPtr)(int)msg16.lParam;
        }

        public override void To16(Machine machine, ref Win32.MSG msg32, ref Win16.MSG msg16)
        {
            msg16.wParam = HWND.Map.To16(msg32.wParam);
            msg16.lParam = (uint)msg32.lParam.ToInt32();
        }
    }

    class copy_hwnd : Postable
    {
        public override void To32(Machine machine, ref Win16.MSG msg16, ref Win32.MSG msg32)
        {
            msg32.wParam = (IntPtr)msg16.wParam;
            msg32.lParam = HWND.Map.To32(msg16.lParam.Loword());
        }

        public override void To16(Machine machine, ref Win32.MSG msg32, ref Win16.MSG msg16)
        {
            msg16.wParam = (ushort)msg32.wParam.ToInt32();
            msg16.lParam = HWND.Map.To16(msg32.lParam);
        }
    }

    class hmenu_copy : Postable
    {
        public override void To32(Machine machine, ref Win16.MSG msg16, ref Win32.MSG msg32)
        {
            msg32.wParam = HMENU.Map.To32(msg16.wParam);
            msg32.lParam = (IntPtr)(int)msg16.lParam;
        }

        public override void To16(Machine machine, ref Win32.MSG msg32, ref Win16.MSG msg16)
        {
            msg16.wParam = HMENU.Map.To16(msg32.wParam);
            msg16.lParam = (uint)msg32.lParam.ToInt32();
        }
    }



    class copy_htask : Postable
    {
        public override void To32(Machine machine, ref Win16.MSG msg16, ref Win32.MSG msg32)
        {
            msg32.wParam = (IntPtr)msg16.wParam;
            msg32.lParam = (IntPtr)(int)msg16.lParam;
        }

        public override void To16(Machine machine, ref Win32.MSG msg32, ref Win16.MSG msg16)
        {
            msg16.wParam = (ushort)msg32.wParam.ToInt32();
            msg16.lParam = msg16.wParam == 1 ? machine.ProcessModule.hModule : (ushort)0;
        }
    }


    class hgdiobj_unused: Postable
    {
        public override void To32(Machine machine, ref Win16.MSG msg16, ref Win32.MSG msg32)
        {
            msg32.wParam = HGDIOBJ.To32(msg16.wParam).value;
            msg32.lParam = IntPtr.Zero;
        }

        public override void To16(Machine machine, ref Win32.MSG msg32, ref Win16.MSG msg16)
        {
            msg16.wParam = HGDIOBJ.To16(new HGDIOBJ(msg32.wParam));
            msg16.lParam = 0;
        }
    }

    class hgdiobj_copy: Postable
    {
        public override void To32(Machine machine, ref Win16.MSG msg16, ref Win32.MSG msg32)
        {
            msg32.wParam = HGDIOBJ.To32(msg16.wParam).value;
            msg32.lParam = (IntPtr)(int)msg16.lParam;
        }

        public override void To16(Machine machine, ref Win32.MSG msg32, ref Win16.MSG msg16)
        {
            msg16.wParam = HGDIOBJ.To16(new HGDIOBJ(msg32.wParam));
            msg16.lParam = (uint)msg32.lParam.ToInt32();
        }
    }

    // Cracks lparam16 -> wParam32 and lParam32
    class cracked_lparam16 : Postable
    {
        public override void To32(Machine machine, ref Win16.MSG msg16, ref Win32.MSG msg32)
        {
            msg32.wParam = (IntPtr)(short)msg16.lParam.Loword();
            msg32.lParam = (IntPtr)(short)msg16.lParam.Hiword();
        }

        public override void To16(Machine machine, ref Win32.MSG msg32, ref Win16.MSG msg16)
        {
            msg16.wParam = 0;
            msg16.lParam = BitUtils.MakeDWord(msg32.wParam.Loword(), msg32.lParam.Loword());
        }
    }

    class XXX : Postable
    {
        public override void To32(Machine machine, ref Win16.MSG msg16, ref Win32.MSG msg32)
        {
        }

        public override void To16(Machine machine, ref Win32.MSG msg32, ref Win16.MSG msg16)
        {
        }
    }


}
