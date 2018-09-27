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
    public class ProcMap<TDelegate>
    {
        public IntPtr Connect(TDelegate wndProc32, uint wndProc16)
        {
            IntPtr ptr = Marshal.GetFunctionPointerForDelegate(wndProc32);
            _proc16Map.Add(wndProc16, ptr);
            _proc32Map.Add(ptr, wndProc16);
            _keepAliveMap.Add(wndProc16, wndProc32);
            return ptr;
        }

        public void Connect(IntPtr wndProc32, uint wndProc16)
        {
            _proc16Map.Add(wndProc16, wndProc32);
            _proc32Map.Add(wndProc32, wndProc16);
        }

        public IntPtr To32(uint proc16)
        {
            IntPtr proc32;
            if (_proc16Map.TryGetValue(proc16, out proc32))
                return proc32;
            return IntPtr.Zero;
        }

        public uint To16(IntPtr proc32)
        {
            uint proc16;
            if (_proc32Map.TryGetValue(proc32, out proc16))
                return proc16;
            return 0;
        }

        Dictionary<uint, TDelegate> _keepAliveMap = new Dictionary<uint, TDelegate>();
        Dictionary<uint, IntPtr> _proc16Map = new Dictionary<uint, IntPtr>();
        Dictionary<IntPtr, uint> _proc32Map = new Dictionary<IntPtr, uint>();
    }
}
