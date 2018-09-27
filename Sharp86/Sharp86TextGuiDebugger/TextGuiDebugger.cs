/*
Sharp86 - 8086 Emulator
Copyright (C) 2017-2018 Topten Software.

Sharp86 is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Sharp86 is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Sharp86.  If not, see <http://www.gnu.org/licenses/>.
*/

using Sharp86;
using ConFrames;
using System;
using PetaJson;

namespace Sharp86
{
    public class TextGuiDebugger : DebuggerCore
    {
        public TextGuiDebugger()
        {
            CommandDispatcher.RegisterCommandHandler(new TextGuiDebuggerCommands(this));

            _desktop = new ConFrames.Desktop(120, 40);
            _desktop.PreviewKey = OnPreviewKey;

            _codeWindow = new CodeWindow(this);
            _codeWindow.Open(_desktop);
            _registersWindow = new RegistersWindow(this);
            _registersWindow.Open(_desktop);
            _watchWindow = new WatchWindow(this);
            _watchWindow.Open(_desktop);
            _consoleWindow = new CommandWindow(this);
            _consoleWindow.Open(_desktop);
            _memoryWindow = new MemoryWindow(this);
            _memoryWindow.Open(_desktop);

            _codeWindow.Activate();
        }

        Desktop _desktop;
        CodeWindow _codeWindow;
        RegistersWindow _registersWindow;
        ConsoleWindow _consoleWindow;
        WatchWindow _watchWindow;

        [Json("memory1", KeepInstance = true)]
        MemoryWindow _memoryWindow;

        bool OnPreviewKey(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.F10 && key.Modifiers == 0)
            {
                ExecuteCommand("o");
                return true;
            }

            if (key.Key == ConsoleKey.F8)
            {
                if (key.Modifiers == ConsoleModifiers.Shift)
                {
                    ExecuteCommand("t");
                }
                else
                {
                    ExecuteCommand("s");
                }
                return true;
            }

            if (key.Key == ConsoleKey.F5)
            {
                ExecuteCommand("r");
                return true;
            }

            return false;
        }

        protected override bool OnBreak()
        {
            _codeWindow.MoveToIP();
            _registersWindow.Invalidate();
            _memoryWindow.OnBreak();
            _watchWindow.OnBreak();

            _desktop.Process();

            _registersWindow.OnResume();
            _memoryWindow.OnResume();

            return true;
        }

        public override void WriteConsole(string output)
        {
            _consoleWindow.Write(output);
            //base.Write(output);
        }

        public override void PromptConsole(string str)
        {
            _consoleWindow.SetInputBuffer(str);
            _consoleWindow.Activate();
        }

        public void ExecuteCommand(string command)
        {
            base.CommandDispatcher.ExecuteCommand(command);
            if (base.ShouldContinue)
            {
                if (!base.IsStepping)
                {
                    _desktop.ViewMode = ViewMode.StdOut;
                    _desktop.RestoreForegroundWindow();
                }

                _desktop.EndProcessing();
            }
        }

        public CodeWindow CodeWindow
        {
            get { return _codeWindow; }
        }

        public MemoryWindow MemoryWindow
        {
            get { return _memoryWindow; }
        }
    }
}
