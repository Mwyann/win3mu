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
    public class TempContext : IDisposable
    {
        public TempContext(Machine machine)
        {
            _machine = machine;
        }

        Machine _machine;

        public void Dispose()
        {
            for (int i = 0; i < _globalAllocations.Count; i++)
            {
                Marshal.FreeHGlobal(_globalAllocations[i]);
            }
            _globalAllocations.Clear();
        }

        // Allocate a temporary unmanaged strnig
        public IntPtr AllocUnmanagedString(string str)
        {
            var ptr = Marshal.StringToHGlobalUni(str);
            _globalAllocations.Add(ptr);
            return ptr;
        }

        List<IntPtr> _globalAllocations = new List<IntPtr>();
    }

}
