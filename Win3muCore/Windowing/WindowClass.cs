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

namespace Win3muCore
{
    public class WindowClass
    {
        public WindowClass(Machine machine, Module16 module, StringOrId menuID, Win32.WNDCLASS wc32, Win16.WNDCLASS wc16)
        {
            _machine = machine;
            _module = module;
            _menuID = menuID;
            this.wc32 = wc32;
            this.wc16 = wc16;
            _procInstance = _machine.MakeProcInstance(module.DataSelector, wc16.lpfnWndProc);

            WndProc32 = _machine.Messaging.GetWndProc32(_procInstance, false);
        }

        public ushort Atom;

        public string Name
        {
            get
            {
                return wc32.lpszClassName;
            }
        }

        public Win32.WNDCLASS wc32;
        public Win16.WNDCLASS wc16;
        public IntPtr WndProc32;
        Machine _machine;
        Module16 _module;
        StringOrId _menuID;
        uint _procInstance;

        public uint WndProcInstance
        {
            get
            {
                return _procInstance;
            }
        }

        public void LoadResourceMenu(IntPtr hWnd)
        {
            if (_menuID.IsNull)
                return;

            var res = _module.NeFile.GetResourceStream(Win16.ResourceType.RT_MENU.ToString(), _menuID.ToString());
            if (res != null)
            {
                var menu = Resources.LoadMenu(res);
                User.SetMenu(hWnd, menu);
            }
        }

        public static void OnNcCreate(IntPtr hWnd)
        {
            var className = User.GetClassName(hWnd);
            WindowClass wc;
            if (!_registeredClasses.TryGetValue(className, out wc))
                return;

            wc.LoadResourceMenu(hWnd);
        }

        public static bool IsRegistered(string name)
        {
            return _registeredClasses.ContainsKey(name);
        }

        public static void Register(WindowClass wndClass)
        {
            // Keep it alive
            _registeredClasses.Add(wndClass.Name, wndClass);
        }

        public static void Unregister(StringOrId name)
        {
            if (name.Name != null)
            {
                _registeredClasses.Remove(name.Name);
            }
            else
            {
                var wc = _registeredClasses.Values.FirstOrDefault(x => x.Atom == name.ID);
                if (wc != null)
                    _registeredClasses.Remove(wc.Name);
            }
        }

        public static WindowClass Find(string name)
        {
            WindowClass wc;
            if (_registeredClasses.TryGetValue(name, out wc))
                return wc;

            return null;
        }

        static Dictionary<string, WindowClass> _registeredClasses = new Dictionary<string, WindowClass>(StringComparer.InvariantCultureIgnoreCase);
    }
}
