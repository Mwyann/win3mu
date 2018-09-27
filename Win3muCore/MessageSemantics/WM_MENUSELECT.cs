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

namespace Win3muCore.MessageSemantics
{
    class WM_MENUSELECT : Postable
    {
        public override void To32(Machine machine, ref Win16.MSG msg16, ref Win32.MSG msg32)
        {
            msg32.lParam = HMENU.Map.To32(msg16.lParam.Hiword());
            if ((msg16.lParam & Win16.MF_POPUP) != 0)
            {
                // Get the menu handle
                IntPtr hPopup = HMENU.Map.To32(msg16.wParam);

                // Need to convert back to index
                int i;
                for (i = 0; i < User.GetMenuItemCount(msg32.lParam); i++)
                {
                    if (User.GetSubMenu(msg32.lParam, i).value == hPopup)
                    {
                        break;
                    }
                }

                msg32.wParam = (IntPtr)(int)BitUtils.MakeDWord((ushort)i, msg16.lParam.Loword());
            }
            else
            {
                msg32.wParam = (IntPtr)(int)BitUtils.MakeDWord(msg16.wParam, msg16.lParam.Loword());
            }
        }

        public override void To16(Machine machine, ref Win32.MSG msg32, ref Win16.MSG msg16)
        {
            uint uwParam = (uint)msg32.wParam.ToUInt32();
            if ((uwParam.Hiword() & Win16.MF_POPUP) != 0)
            {
                IntPtr hSubMenu = User.GetSubMenu(msg32.lParam, (int)uwParam.Loword()).value;
                msg16.wParam = HMENU.Map.To16(hSubMenu);
            }
            else
            {
                msg16.wParam = uwParam.Loword();
            }
            msg16.lParam = BitUtils.MakeDWord(uwParam.Hiword(), HMENU.Map.To16(msg32.lParam));
        }
    }
}
