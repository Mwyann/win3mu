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
    public static class EnumUtils
    {
        public static string FormatFlags<T>(T value)
        {
            var intVal = (int)Convert.ChangeType(value, typeof(int));
            return FormatFlags(typeof(T), intVal);
        }

        public static string FormatFlags(Type type, int value)
        {
            var values = Enum.GetValues(type);
            var sb = new StringBuilder();

            foreach (var vo in values)
            {
                int mask = (int)Convert.ChangeType(vo, typeof(int));
                if ((value & mask)!=0)
                {
                    if (sb.Length>0)
                    {
                        sb.Append(" | ");
                    }
                    sb.Append(Enum.GetName(type, vo));

                    value = value & ~((int)mask);
                }
            }

            if (value != 0)
            {
                if (sb.Length > 0)
                    sb.Append(" | ");
                sb.AppendFormat("0x{0:X}", value);
            }

            return sb.ToString();
        }
    }
}
