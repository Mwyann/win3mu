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

namespace Win3muCore
{
    public class HeapPointer : IDisposable
    {
        public HeapPointer(GlobalHeap heap, uint ptr, bool forWrite)
        {
            if (ptr != 0)
            {
                // Get the buffer
                var buf = heap.GetBuffer(ptr, forWrite, out _ofs);
                _pin = GCHandle.Alloc(buf, GCHandleType.Pinned);
            }
        }

        GCHandle _pin;
        int _ofs;

        public void Dispose()
        {
            if (_pin.IsAllocated)
                _pin.Free();
        }

        public static implicit operator IntPtr(HeapPointer ptr)
        {
            if (ptr._pin.IsAllocated)
                return (IntPtr)(ptr._pin.AddrOfPinnedObject().ToInt64() + ptr._ofs);
            else
                return IntPtr.Zero;
        }
    }
}
