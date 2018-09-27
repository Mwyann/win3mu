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
    public class StackWalker
    {
        public StackWalker(Machine machine)
        {
            _machine = machine;
        }

        Machine _machine;

        public class StackEntry
        {
            public uint csip;
            public ushort sp;
            public string name;
        }
        List<StackEntry> _transitions = new List<StackEntry>();

        public void EnterTransition(string name)
        {
            _transitions.Add(new StackEntry()
            {
                csip = BitUtils.MakeDWord(_machine.ip, _machine.cs),
                sp = _machine.sp,
                name = name,
            });
        }

        public void LeaveTransition()
        {
            _transitions.RemoveAt(_transitions.Count - 1);
        }

        public IEnumerable<StackEntry> WalkStack()
        {
            if (_machine.cs == 0 || _machine.ss == 0)
                return Enumerable.Empty<StackEntry>();

            var list = new List<StackEntry>();
            list.Add(new StackEntry()
            {
                csip = BitUtils.MakeDWord(_machine.ip, _machine.cs),
                sp = _machine.sp,
                name = "cs:ip",
            });
            try
            {
                var ss = _machine.ss;
                var bp = _machine.bp;
                var cs = _machine.cs;
                while (true)
                {
                    var priorBP = _machine.MemoryBus.ReadWord(ss, bp);
                    if (priorBP <= bp)
                        break;
                    uint returnAddress;
                    bool farCall;
                    if ((priorBP & 1) != 0)
                    {
                        // far call
                        returnAddress = _machine.MemoryBus.ReadDWord(ss, (ushort)(bp + 2));
                        priorBP = (ushort)(priorBP & ~1);
                        cs = (ushort)(returnAddress >> 16);
                        farCall = true;
                    }
                    else
                    {
                        // near call    
                        returnAddress = (uint)(cs << 16 | _machine.MemoryBus.ReadWord(ss, (ushort)(bp + 2)));
                        farCall = false;
                    }
                    list.Add(new StackEntry() {
                        csip = returnAddress,
                        sp = bp,
                        name = string.Format("0x{0:X4}:{1:X4} {2} call bp = 0x{3:X4}", 
                                    returnAddress.Hiword(), returnAddress.Loword(), 
                                    farCall ? "far" : "near", bp),
                    });
                    bp = priorBP;
                }
                return list.Concat(_transitions).OrderBy(x => x.sp);
            }
            catch
            {
                return list.Concat(_transitions).OrderBy(x => x.sp);
            }
        }

    }
}
