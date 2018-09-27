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
    [MappedTypeAttribute]
    public struct nuint : IFormattable
    {
        public nuint(uint value)
        {
            this.value = value;
        }

        public uint value;

        public static nuint To32(ushort value)
        {
            return new nuint(value);
        }

        public static ushort To16(nuint value)
        {
            return (ushort)value.value;
        }

        public static implicit operator nuint(uint value)
        {
            return new nuint(value);
        }

        public static implicit operator uint (nuint value)
        {
            return value.value;
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return value.ToString(format, formatProvider);
        }
    }
}
