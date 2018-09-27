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

namespace Win3muCore
{
    public class CreateStructMap
    {
        public CreateStructMap(Machine machine)
        {
            _machine = machine;
        }

        Machine _machine;

        class Entry
        {
            public uint p16;
            public IntPtr p32;
            public bool allocated;
            public int refCount;
            public IntPtr hWnd;
        }

        Entry _pendingCreateStruct;
        Dictionary<IntPtr, Entry> _map32to16 = new Dictionary<IntPtr, Entry>();
        Dictionary<uint, Entry> _map16to32 = new Dictionary<uint, Entry>();
        public void PreCreateWindow(uint ptr)
        {
            var mapping = new Entry()
            {
                refCount = 1,
                p16 = ptr,
            };
            _pendingCreateStruct = mapping;
        }

        public void PostCreateWindow(uint ptr)
        {
            if (_pendingCreateStruct != null && _pendingCreateStruct.p16 == ptr)
            {
                // Never called (ie: non-16-bit window class created)
                _pendingCreateStruct = null;
                return;
            }

            // Find the mapping
            Entry mapping;
            if (_map16to32.TryGetValue(ptr, out mapping))
            {
                // Reduce reference count (should always go to zero)
                mapping.refCount--;
                if (mapping.refCount == 0)
                {
                    // Clean up
                    _map16to32.Remove(mapping.p16);
                    _map32to16.Remove(mapping.p32);

                    // Should never be allocated
                    System.Diagnostics.Debug.Assert(!mapping.allocated);
                    return;
                }

                // What the?
                System.Diagnostics.Debug.Assert(false);
            }

            // What the?
            System.Diagnostics.Debug.Assert(false);
        }

        void Connect(Entry mapping)
        {
            _map32to16.Add(mapping.p32, mapping);
            _map16to32.Add(mapping.p16, mapping);
        }

        public IntPtr Get(uint p16)
        {
            if (p16 == 0)
                return IntPtr.Zero;

            Entry mapping;
            if (_map16to32.TryGetValue(p16, out mapping))
            {
                return mapping.p32;
            }

            System.Diagnostics.Debug.Assert(false);
            return IntPtr.Zero;
        }

        public uint Get(IntPtr p32, IntPtr hWnd32)
        {
            if (p32 == IntPtr.Zero)
                return 0;

            // Already connected
            Entry mapping;
            if (_map32to16.TryGetValue(p32, out mapping))
            {
                mapping.refCount++;
                return mapping.p16;
            }

            // CreateWindow call in progress?
            if (_pendingCreateStruct != null)
            {
                mapping = _pendingCreateStruct;
                _pendingCreateStruct = null;
                mapping.p32 = p32;
                mapping.refCount++;
                mapping.hWnd = hWnd32;
                Connect(mapping);
                return mapping.p16;
            }

            // Try to find by hWnd
            mapping = _map16to32.Values.FirstOrDefault(x => x.hWnd == hWnd32);
            if (mapping!= null)
            {
                mapping.refCount++;
                return mapping.p16;
            }

            // Other - probably loading dialog resource template, need to convert

            // Get the 32-bit create struct
            var cs32 = Marshal.PtrToStructure<Win32.CREATESTRUCT>(p32);

            // Convert it
            var cs16 = new Win16.CREATESTRUCT();
            cs16.x = (short)(cs32.cx == Win32.CW_USEDEFAULT ? Win16.CW_USEDEFAULT : cs32.x);
            cs16.y = (short)(cs32.cx == Win32.CW_USEDEFAULT ? Win16.CW_USEDEFAULT : cs32.y);
            cs16.cx = (short)(cs32.cx == Win32.CW_USEDEFAULT ? Win16.CW_USEDEFAULT : cs32.cx);
            cs16.cy = (short)(cs32.cx == Win32.CW_USEDEFAULT ? Win16.CW_USEDEFAULT : cs32.cy);
            cs16.dwExStyle = cs32.dwExStyle;
            cs16.style = cs32.style;
            cs16.lpszClassName = _machine.SysAllocString(Marshal.PtrToStringUni(cs32.lpszClassName));
            cs16.lpszName = _machine.SysAllocString(Marshal.PtrToStringUni(cs32.lpszClassName));

            if ((cs16.style & Win16.WS_CHILD) != 0)
            {
                cs16.hMenu = (ushort)(short)(cs32.hMenu);
            }
            else
            {
                cs16.hMenu = HMENU.Map.To16(cs32.hMenu);
            }

            cs16.hWndParent = HWND.Map.To16(cs32.hWndParent);

            // Allocate it in emulated memory
            mapping = new Entry()
            {
                p16 = _machine.SysAlloc(cs16),
                p32 = p32,
                allocated = true,
                refCount = 1,
                hWnd = hWnd32,
            };

            // Connect 'em
            Connect(mapping);

            return mapping.p16;
        }

        public void FreeCreateStruct(IntPtr p32, IntPtr hWnd32)
        {
            Entry mapping;
            if (!_map32to16.TryGetValue(p32, out mapping))
                mapping = _map32to16.Values.FirstOrDefault(x => x.hWnd == hWnd32);

            if (mapping!= null)
            {
                mapping.refCount--;
                if (mapping.refCount > 0)
                    return;


                // Free up allocated create structs
                if (mapping.allocated)
                {
                    var cs16 = _machine.ReadStruct<Win16.CREATESTRUCT>(mapping.p16);
                    _machine.SysFree(cs16.lpszClassName);
                    _machine.SysFree(cs16.lpszName);
                    _machine.SysFree(mapping.p16);
                }

                // Remove from map
                _map32to16.Remove(mapping.p32);
                _map16to32.Remove(mapping.p16);
            }
        }

    }
}
