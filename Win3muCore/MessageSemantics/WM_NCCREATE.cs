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
    class WM_NC_OR_CREATE : Callable
    {
        public WM_NC_OR_CREATE(bool nc)
        {
            _nc = nc;
        }

        bool _nc;

        public override uint Call32from16(Machine machine, bool hook, bool dlgproc, ref Win16.MSG msg16, ref Win32.MSG msg32, Func<IntPtr> callback)
        {
            // Get info
            var info = GetInfo(machine, msg32.hWnd);

            // Make sure no funny business going on
            // (ie: the same CREATESTRUCT is passed out from 16-bit code as was passed in)
            System.Diagnostics.Debug.Assert(info.Struct32 != IntPtr.Zero);
            System.Diagnostics.Debug.Assert(info.Struct16 != 0);
            System.Diagnostics.Debug.Assert(info.Struct16 == msg16.lParam);

            // Convert
            Win16.CREATESTRUCT cs16 = machine.ReadStruct<Win16.CREATESTRUCT>(msg16.lParam);
            Win32.CREATESTRUCT cs32;
            info.Convert(ref cs16, out cs32);
            Marshal.StructureToPtr(cs32, info.Struct32, false);

            // Call
            msg32.wParam = IntPtr.Zero;
            msg32.lParam = info.Struct32;
            IntPtr retv = callback();

            // Convert back
            cs32 = Marshal.PtrToStructure<Win32.CREATESTRUCT>(info.Struct32);
            info.Convert(ref cs32, out cs16);
            machine.WriteStruct(info.Struct16, ref cs16);

            // Return value depends on WM_CREATE/WM_NCCREATE
            if (_nc)
            {
                return retv != IntPtr.Zero ? 1U : 0U;
            }
            else
            {
                return retv.ToInt64() < 0 ? unchecked((uint)-1) : 0U;
            }
        }


        public override IntPtr Call16from32(Machine machine, bool hook, bool dlgproc, ref Win32.MSG msg32, ref Win16.MSG msg16, Func<uint> callback)
        {
            var info = GetInfo(machine, msg32.hWnd);

            // Do we need to clean up after this call?
            bool cleanup = !_nc && info.WM_CREATE_Pending;

            IntPtr saveStruct32 = info.Struct32;
            uint saveStruct16 = info.Struct16;
            ushort saveSP = machine.sp;
            try
            {
                // Store the 32-bit struct pointer
                info.Struct32 = msg32.lParam;

                // Allocate a 16-bit struct on the stack (unless one already exists)
                if (info.Struct16 == 0)
                {
                    machine.sp -= (ushort)Marshal.SizeOf<Win16.CREATESTRUCT>();
                    info.Struct16 = BitUtils.MakeDWord(machine.sp, machine.ss);
                }

                // Convert
                Win32.CREATESTRUCT cs32 = Marshal.PtrToStructure<Win32.CREATESTRUCT>(msg32.lParam);
                Win16.CREATESTRUCT cs16;
                info.Convert(ref cs32, out cs16);
                machine.WriteStruct(info.Struct16, ref cs16);

                // Call 
                msg16.wParam = 0;
                msg16.lParam = info.Struct16;
                var retv = callback();

                // Convert back
                cs16 = machine.ReadStruct<Win16.CREATESTRUCT>(info.Struct16);
                info.Convert(ref cs16, out cs32);
                Marshal.StructureToPtr(cs32, msg32.lParam, false);

                if (_nc)
                {
                    // If failed by WM_NCCREATE and we're top of the stack then clean up info
                    if (retv == 0 && saveStruct32==IntPtr.Zero)
                    {
                        cleanup = true;
                    }

                    return (IntPtr)(retv == 0 ? 0 : 1);
                }
                else
                {
                    return (IntPtr)(retv == 0 ? 0 : -1);
                }
            }
            finally
            {
                info.Struct32 = saveStruct32;
                info.Struct16 = saveStruct16;
                machine.sp = saveSP;

                if (cleanup)
                {
                    info.Discard();
                }
            }
        }

        static Dictionary<IntPtr, CreateInfo> _createInfos = new Dictionary<IntPtr, CreateInfo>();
        static CreateInfo GetInfo(Machine machine, IntPtr hWnd)
        {
            CreateInfo ci;
            if (!_createInfos.TryGetValue(hWnd, out ci))
            {
                ci = new CreateInfo(machine, hWnd);
                _createInfos.Add(hWnd, ci);
            }

            return ci;
        }

        class CreateInfo
        {
            public CreateInfo(Machine machine, IntPtr hWnd)
            {
                _machine = machine;
                _hWnd = hWnd;
            }

            Machine _machine;
            public IntPtr _hWnd;
            Dictionary<IntPtr, uint> _strings16 = new Dictionary<IntPtr, uint>();
            Dictionary<uint, IntPtr> _strings32 = new Dictionary<uint, IntPtr>();
            List<IntPtr> _allocated32 = new List<IntPtr>();
            public bool WM_CREATE_Pending = true;

            public IntPtr Struct32;
            public uint Struct16;

            public void Discard()
            {
                // Remove from map
                _createInfos.Remove(_hWnd);

                // Clean up strings
                foreach (var s in _strings16.Values)
                {
                    _machine.SysFree(s);
                }
                foreach (var s in _allocated32)
                {
                    Marshal.FreeHGlobal(s);
                }

                _allocated32.Clear();
                _strings16.Clear();
                _strings32.Clear();
            }

            uint GetString(IntPtr ptr32)
            {
                // Easy case
                if (ptr32 == IntPtr.Zero)
                    return 0;

                // Already allocated?
                uint ptr16;
                if (!_strings16.TryGetValue(ptr32, out ptr16))
                {
                    var str = Marshal.PtrToStringUni(ptr32);
                    ptr16 = _machine.SysAllocString(str);
                    _strings16.Add(ptr32, ptr16);
                    _strings32.Add(ptr16, ptr32);
                }

                // Return it
                return ptr16;
            }

            IntPtr GetString(uint ptr16)
            {
                // Easy case
                if (ptr16 == 0)
                    return IntPtr.Zero;

                // Already allocated?
                IntPtr ptr32;
                if (!_strings32.TryGetValue(ptr16, out ptr32))
                {
                    var str = _machine.ReadString(ptr16);
                    ptr32 = Marshal.StringToHGlobalUni(str);

                    _strings16.Add(ptr32, ptr16);
                    _strings32.Add(ptr16, ptr32);

                    // Remember to free it
                    _allocated32.Add(ptr32);
                }

                return ptr32;
            }


            public void Convert(ref Win32.CREATESTRUCT cs32, out Win16.CREATESTRUCT cs16)
            {
                // Convert it
                cs16.lpCreateParams = cs32.lpCreateParams.DWord();
                cs16.hInstance = 0;
                cs16.x = (short)(cs32.cx == Win32.CW_USEDEFAULT ? Win16.CW_USEDEFAULT : cs32.x);
                cs16.y = (short)(cs32.cx == Win32.CW_USEDEFAULT ? Win16.CW_USEDEFAULT : cs32.y);
                cs16.cx = (short)(cs32.cx == Win32.CW_USEDEFAULT ? Win16.CW_USEDEFAULT : cs32.cx);
                cs16.cy = (short)(cs32.cx == Win32.CW_USEDEFAULT ? Win16.CW_USEDEFAULT : cs32.cy);
                cs16.dwExStyle = cs32.dwExStyle;
                cs16.style = cs32.style;
                cs16.lpszClassName = GetString(cs32.lpszClassName);
                cs16.lpszName = GetString(cs32.lpszName);

                if (cs32.lpCreateParams != IntPtr.Zero && !WindowClass.IsRegistered(_machine.ReadString(cs16.lpszClassName)))
                {
                    throw new NotImplementedException("CREATESTRUCT.lpCreateParams not supported");
                }

                if ((cs16.style & Win16.WS_CHILD) != 0)
                {
                    cs16.hMenu = (ushort)(short)(cs32.hMenu);
                }
                else
                {
                    cs16.hMenu = HMENU.Map.To16(cs32.hMenu);
                }

                cs16.hWndParent = HWND.Map.To16(cs32.hWndParent);
            }

            public void Convert(ref Win16.CREATESTRUCT cs16, out Win32.CREATESTRUCT cs32)
            {
                if (cs16.lpCreateParams != 0 && !WindowClass.IsRegistered(_machine.ReadString(cs16.lpszClassName)))
                {
                    throw new NotImplementedException("CREATESTRUCT.lpCreateParams not supported");
                }

                // Convert it
                cs32.lpCreateParams = BitUtils.DWordToIntPtr(cs16.lpCreateParams);
                cs32.hInstance = IntPtr.Zero;
                cs32.x = (short)(cs16.cx == Win16.CW_USEDEFAULT ? Win16.CW_USEDEFAULT : cs16.x);
                cs32.y = (short)(cs16.cx == Win16.CW_USEDEFAULT ? Win16.CW_USEDEFAULT : cs16.y);
                cs32.cx = (short)(cs16.cx == Win16.CW_USEDEFAULT ? Win16.CW_USEDEFAULT : cs16.cx);
                cs32.cy = (short)(cs16.cx == Win16.CW_USEDEFAULT ? Win16.CW_USEDEFAULT : cs16.cy);
                cs32.dwExStyle = cs16.dwExStyle;
                cs32.style = cs16.style;
                cs32.lpszClassName = GetString(cs16.lpszClassName);
                cs32.lpszName = GetString(cs16.lpszName);

                if ((cs16.style & Win16.WS_CHILD) != 0)
                {
                    cs32.hMenu = (IntPtr)cs16.hMenu;
                } 
                else                            
                {
                    cs32.hMenu = HMENU.Map.To32(cs16.hMenu);
                }

                cs32.hWndParent = HWND.Map.To32(cs16.hWndParent);
            }


        }
    }
}
