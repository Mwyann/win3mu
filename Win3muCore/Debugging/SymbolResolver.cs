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
    public class SymbolResolver : ISymbolScope
    {
        public SymbolResolver(Machine machine)
        {
            _machine = machine;
        }

        Machine _machine;
        Dictionary<string, Symbol> _symbolMap;

        public Symbol ResolveSymbol(string name)
        {
            if (_symbolMap == null)
                BuildSymbolMap();

            Symbol sym;
            if (_symbolMap.TryGetValue(name, out sym))
                return sym;

            return null;
        }

        void BuildSymbolMap()
        {
            _symbolMap = new Dictionary<string, Symbol>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var m in _machine.ModuleManager.AllModules.OfType<Module32>())
            {
                foreach (var ord in m.GetExports())
                {
                    var name = m.GetNameFromOrdinal(ord);
                    var addr = m.GetProcAddress(ord);
                    _symbolMap.Add(name, new LiteralSymbol(new FarPointer(addr)));
                }
            }

            foreach (var mname in MessageNames.All)
            {
                _symbolMap.Add(mname.Value, new LiteralSymbol(mname.Key));
            }
        }
    }
}
