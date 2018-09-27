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
    class BitAllocator<T> where T : class
    {
        public BitAllocator(int count)
        {
            _allocated = new T[count];
            _count = count;
        }

        T[] _allocated;
        int _count;
        int _nextAllocation;

        public void Reserve(int index, T value)
        {
            System.Diagnostics.Debug.Assert(_allocated[index]==null);
            _allocated[index] = value;
        }

        public int Allocate(T val)
        {
            while (_allocated[_nextAllocation]!=null)
            {
                _nextAllocation++;
            }

            _allocated[_nextAllocation++] = val;
            return _nextAllocation - 1;
        }

        public void Free(int index)
        {
            System.Diagnostics.Debug.Assert(_allocated[index]!=null);
            _allocated[index] = null;
        }

        public T Get(int index)
        {
            return _allocated[index];
        }
    }
}
