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

using System.Collections.Generic;
using System.IO;

namespace Win3muCore.NeFile
{
    public class ResourceTable
    {
        public ResourceTable()
        {
            types = new ResourceTypeTable[0];
            names = new Dictionary<int, string>();
        }

        public ResourceTable(FileStream r)
        {
            baseFileOffset = (int)r.Position;

            alignShift = r.ReadUInt16();
            names = null;

            // Read the types
            var typesList = new List<ResourceTypeTable>();
            while (true)
            {
                var save = r.Position;
                var rt = new ResourceTypeTable(r);
                if (rt.type == 0)
                {
                    break;
                }
                typesList.Add(rt);
            }
            types = typesList.ToArray();

            // Read the names
            names = new Dictionary<int, string>();
            while (true)
            {
                int offset = 
                    (int)r.Position - baseFileOffset;
                var str = r.ReadLengthPrefixedString();
                if (str == null)
                    break;

                names.Add(offset, str);
            }

//            r.Seek(save, SeekOrigin.Begin);

            foreach (var rt in types)
            {
                rt.name = ResourceName(rt);
                foreach (var res in rt.resources)
                {
                    res.name = ResourceName(res);
                    res.offset = (1 << alignShift) * res.offset;
                    res.length = (1 << alignShift) * res.length;
                }
            }

        }

        public ushort alignShift;
        public int baseFileOffset;
        public ResourceTypeTable[] types;
        public Dictionary<int, string> names;

        public string ResourceName(ResourceEntry rt)
        {
            if ((rt.id & 0x8000) != 0)
            {
                return string.Format("#{0}", rt.id & ~0x8000);
            }
            else
            {
                return names[rt.id];
            }
        }

        public string ResourceName(ResourceTypeTable rt)
        {
            if ((rt.type & 0x8000) != 0)
            {
                var name = ((Win16.ResourceType)(rt.type & ~0x8000)).ToString();
                if (char.IsDigit(name[0]))
                    name = "#" + name;
                return name;
            }
            else
            {
                return names[rt.type];
            }
        }

    }

}
