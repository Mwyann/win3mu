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
using Sharp86;

namespace Win3muCore
{
    public class StringOrId
    {
        public StringOrId(ushort id)
        {
            ID = id;
        }

        public StringOrId(string str)
        {
            Name = str;
        }

        public StringOrId(Machine machine, uint lpszName)
        {
            if (lpszName.Hiword() == 0)
            {
                ID = lpszName.Loword();
            }
            else
            {
                Name = machine.MemoryBus.ReadString(lpszName);
            }
        }

        public string Name;
        public ushort ID;

        public bool IsNull
        {
            get { return Name == null && ID == 0; }
        }

        public override string ToString()
        {
            if (Name != null)
                return Name;
            else
                return string.Format("#{0}", ID);
        }
    }
}
