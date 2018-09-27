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
    public abstract class ModuleBase
    {
        public abstract string GetModuleName();
        public abstract string GetModuleFileName();
        public abstract void Load(Machine machine);
        public abstract void Unload(Machine machine);
        public abstract IEnumerable<string> GetReferencedModules();
        public abstract void Link(Machine machine);
        public abstract void Init(Machine machine);
        public abstract void Uninit(Machine machine);
        public abstract ushort GetOrdinalFromName(string functionName);
        public abstract string GetNameFromOrdinal(ushort ordinal);
        public abstract uint GetProcAddress(ushort ordinal);
        public abstract IEnumerable<ushort> GetExports();

        public int LoadCount;
        public bool Initialized;
        public ushort hModule;
    }
}
