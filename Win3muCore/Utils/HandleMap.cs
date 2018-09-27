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

namespace Win3muCore
{
    public class HandleMap
    {
        public HandleMap()
        {

        }

        public void DefineMapping(IntPtr handle32, ushort handle16)
        {
            _map16to32.Add(handle16, handle32);
            _map32to16.Add(handle32, handle16);
        }

        ushort AllocateHandle()
        {
            while (_map16to32.ContainsKey(_nextHandle) || _nextHandle<baseHandle)
                _nextHandle++;
            return _nextHandle;
        }

        public void Destroy32(IntPtr handle32)
        {
            if (_map32to16.ContainsKey(handle32))
            {
                _map16to32.Remove(To16(handle32));
                _map32to16.Remove(handle32);
            }
        }

        public void Destroy16(ushort handle16)
        {
            if (_map16to32.ContainsKey(handle16))
            {
                _map32to16.Remove(To32(handle16));
                _map16to32.Remove(handle16);
            }
        }

        public bool IsValid16(ushort handle16)
        {
            return _map16to32.ContainsKey(handle16);
        }

        public ushort To16(IntPtr handle32)
        {
            if (handle32 == IntPtr.Zero)
                return 0;

            ushort handle16;
            if (_map32to16.TryGetValue(handle32, out handle16))
                return handle16;

            handle16 = AllocateHandle();
            _map32to16.Add(handle32, handle16);
            _map16to32.Add(handle16, handle32);

            return handle16;
        }

        public IntPtr To32(ushort handle16)
        {
            if (handle16 == 0)
                return IntPtr.Zero;

            IntPtr handle32;
            if (_map16to32.TryGetValue(handle16, out handle32))
                return handle32;

            Log.WriteLine("Invalid handle - 0x{0:X4}", handle16);
            //throw new InvalidOperationException(string.Format("Unknown 16 bit handle 0x{0:X4}", handle16));
            return (IntPtr)(-1);
        }

        public IEnumerable<IntPtr> GetAll32()
        {
            return _map32to16.Keys;
        }

        const ushort baseHandle = 32;      // Less than this wordzap won't start??
        ushort _nextHandle = baseHandle;
        Dictionary<IntPtr, ushort> _map32to16 = new Dictionary<IntPtr, ushort>();
        Dictionary<ushort, IntPtr> _map16to32 = new Dictionary<ushort, IntPtr>();
    }
}
