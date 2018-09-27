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
using System.IO;

namespace Win3muCore.NeFile
{
    public class ResourceTypeTable
    {
        public string name;
        public ushort type;
        public ResourceEntry[] resources;

        public ResourceTypeTable(FileStream r)
        {
            resources = null;
            name = null;

            // Read the type
            type = r.ReadUInt16();
            if (type == 0)
                return;

            // Read the count
            int count = r.ReadUInt16();

            // Reserved
            r.ReadUInt16();
            r.ReadUInt16();

            // Allocate resource entries
            resources = new ResourceEntry[count];
            for (int i = 0; i < count; i++)
            {
                resources[i] = new ResourceEntry();
                resources[i].Read(r);
            }
        }

        Dictionary<string, ResourceEntry> _entryMap;
        public ResourceEntry FindEntry(string name)
        {
            if (_entryMap == null)
            {
                _entryMap = new Dictionary<string, ResourceEntry>(StringComparer.InvariantCultureIgnoreCase);
                foreach (var e in resources)
                {
                    if (!_entryMap.ContainsKey(e.name))
                    {
                        _entryMap.Add(e.name, e);
                    }
                    if (e.nameTableName != null && !_entryMap.ContainsKey(e.nameTableName))
                        _entryMap.Add(e.nameTableName, e);
                }
            }

            ResourceEntry re;
            if (!_entryMap.TryGetValue(name, out re))
                return null;

            return re;
        }
    }

}
