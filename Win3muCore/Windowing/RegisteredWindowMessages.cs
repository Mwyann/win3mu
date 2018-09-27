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
    public static class RegisteredWindowMessages
    {
        static HashSet<ushort> _registeredWindowMessages = new HashSet<ushort>();
        public static void Register(uint message)
        {
            if (message <= 0xFFFF)
            {
                _registeredWindowMessages.Add(message.Loword());
            }
            else
            {
                throw new NotImplementedException(string.Format("registered window message outside 16-bit range - {0}", message));
            }
        }

        public static bool IsRegistered(ushort message)
        {
            return _registeredWindowMessages.Contains(message);
        }

    }
}
