/*
ConFrames - Gui Stuff for Console Windows
Copyright (C) 2017-2018 Topten Software.

ConFrames is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ConFrames is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ConFrames.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ConFrames
{
    public class Desktop
    {
        public Desktop(int width, int height)
        {
            // Save stdout
            _stdout = Interop.GetStdHandle(Interop.STD_OUTPUT_HANDLE);
            
            // Create buffer
            _buffer = Interop.CreateConsoleScreenBuffer(Interop.GENERIC_READWRITE, (uint)0, IntPtr.Zero, Interop.CONSOLE_TEXTMODE_BUFFER, IntPtr.Zero);

            // Default colors
            ActiveBorderBackgroundColor = ConsoleColor.Blue;
            ActiveBorderLineColor = ConsoleColor.White;
            InactiveBorderLineColor = ConsoleColor.Gray;
            InactiveBorderBackgroundColor = ConsoleColor.DarkBlue;

            // Default desktop size
            DesktopSize = new Size(width, height);
            DesktopColor = ConsoleColor.DarkBlue;
        }

        // Handle to the screen and stdout buffers
        IntPtr _stdout;
        IntPtr _buffer;

        // Desktop size
        Size _desktopSize;
        public Size DesktopSize
        {
            get { return _desktopSize; }
            set
            {
                try
                {
                    // Try to set it
                    Interop.SetBufferAndScreenSize(_buffer, (short)value.Width, (short)value.Height);

                    // Save it
                    _desktopSize = value;
                }
                catch
                {
                    try
                    {
                        // try to set it back
                        Interop.SetBufferAndScreenSize(_buffer, (short)_desktopSize.Width, (short)_desktopSize.Height);
                    }
                    catch { }
                    throw;
                }
            }
        }

        // List of all windows
        List<Window> _windows = new List<Window>();

        // Register a new window
        internal void AddWindow(Window window)
        {
            if (_windows.IndexOf(window)<0)
            {
                _windows.Add(window);
                InvalidateDesktop();
            }
        }

        // Remove a window
        internal void RemoveWindow(Window window)
        {
            _windows.Remove(window);
            InvalidateDesktop();
        }


        // Colors
        public ConsoleColor ActiveBorderBackgroundColor
        {
            get;
            set;
        }

        public ConsoleColor ActiveBorderLineColor
        {
            get;
            set;
        }

        public ConsoleColor InactiveBorderBackgroundColor
        {
            get;
            set;
        }

        public ConsoleColor InactiveBorderLineColor
        {
            get;
            set;
        }

        // Color of area behind all windows
        ConsoleColor _desktopColor;
        public ConsoleColor DesktopColor
        {
            get
            {
                return _desktopColor;
            }
            set
            {
                _desktopColor = value;
                InvalidateDesktop();
            }
        }

        // Called just before repainting the screen
        protected virtual void OnWillUpdate()
        {
        }

        // Called just after repainting the screen
        protected virtual void OnDidUpdate()
        {
        }

        // Called just before waiting for input
        protected virtual void OnEnterProcessing()
        {
        }

        // Called just after waiting for input
        protected virtual void OnLeaveProcessing()
        {
        }

        // Preview received keys - return true if handled
        protected virtual bool OnPreviewKey(ConsoleKeyInfo key)
        {
            if (PreviewKey != null)
                return PreviewKey(key);
            return false;
        }

        // Handler
        public Func<ConsoleKeyInfo, bool> PreviewKey;

        // Get/Set the currently active window
        public Window ActiveWindow
        {
            get { return _windows.Count == 0 ? null : _windows[_windows.Count - 1]; }
            set
            {
                var oldActive = ActiveWindow;               
                int pos = _windows.IndexOf(value);
                if (pos < _windows.Count-1)
                {
                    _windows.RemoveAt(pos);
                    _windows.Add(value);
                }

                if (oldActive!=ActiveWindow)
                {
                    Invalidate(oldActive);
                    Invalidate(ActiveWindow);
                }
            }
        }

        // Invalidate flags
        bool _needRedraw = false;
        bool _needClear = false;

        public void Invalidate(Window w)
        {
            _needRedraw = true;
        }

        public void InvalidateDesktop()
        {
            _needClear = true;
            _needRedraw = true;
        }

        // Update 
        public void Update()
        {
            // Quit if we don't need a redraw
            if (_needRedraw)
            {
                // Notify
                OnWillUpdate();

                // Clear flag
                _needRedraw = false;

                // Do we need to clear?
                if (_needClear)
                {
                    _needClear = false;

                    // Get screens size
                    Interop.CONSOLE_SCREEN_BUFFER_INFO info;
                    Interop.GetConsoleScreenBufferInfo(_buffer, out info);

                    // Create buffer
                    CharInfo[] buf = new CharInfo[info.dwSize.X * info.dwSize.Y];

                    // Clear buffer
                    var defAttributes = (ushort)((ushort)0 | ((ushort)_desktopColor << 4));
                    for (int i = 0; i < buf.Length; ++i)
                    {
                        buf[i].Attributes = defAttributes;
                        buf[i].Char = (char)' ';
                    }

                    // Copy it
                    var r = new Interop.SmallRect()
                    {
                        Top = (short)0,
                        Left = (short)0,
                        Right = (short)info.dwSize.X,
                        Bottom = (short)info.dwSize.Y,
                    };
                    Interop.WriteConsoleOutput(_buffer,
                            buf,
                            new Interop.Coord() { X = (short)info.dwSize.X, Y = info.dwSize.Y },
                            new Interop.Coord() { X = 0, Y = 0 },
                            ref r);
                }


                // Draw all windows
                foreach (var w in _windows)
                {
                    var buf = w.Draw();
                    var r = new Interop.SmallRect()
                    {
                        Top = (short)w.FrameRectangle.Top,
                        Left = (short)w.FrameRectangle.Left,
                        Right = (short)w.FrameRectangle.Right,
                        Bottom = (short)w.FrameRectangle.Bottom,
                    };
                    Interop.WriteConsoleOutput(_buffer,
                            buf,
                            new Interop.Coord() { X = (short)w.FrameRectangle.Width, Y = (short)w.FrameRectangle.Height },
                            new Interop.Coord() { X = 0, Y = 0 },
                            ref r);
                }

                // Finished
                OnDidUpdate();
            }

            // Reposition cursor according to how the active window wants it
            var active = ActiveWindow;
            if (active != null)
            {
                if (active.CursorPosition.X >= 0 && active.CursorPosition.Y >= 0 &&
                    active.CursorPosition.X < active.FrameRectangle.Width - 2 &&
                    active.CursorPosition.Y < active.FrameRectangle.Height - 2)
                {
                    Interop.SetConsoleCursorPosition(_buffer, new Interop.Coord(
                        (short)(active.FrameRectangle.Left + active.CursorPosition.X + 1),
                        (short)(active.FrameRectangle.Top + active.CursorPosition.Y + 1)
                        ));
                    Interop.SetConsoleCursorVisible(_buffer, active.CursorVisible);
                }
                else
                {
                    Interop.SetConsoleCursorVisible(_buffer, false);
                }
            }
            else
            {
                Interop.SetConsoleCursorVisible(_buffer, false);
            }

        }

        // Cancel from the process loop
        bool _continueProcessing = false;
        public void EndProcessing()
        {
            _continueProcessing = false;
        }

        // Process
        public void Process()
        {
            // Notfiy
            OnEnterProcessing();

            // Make active
            ViewMode = ViewMode.Desktop;

            // Process loop
            _continueProcessing = true;
            while (_continueProcessing)
            {
                // Do any update
                Update();

                // Bring console to front
                BringToFront();

                // Read the next key
                var key = Console.ReadKey(true);

                // Switch windows?
                if (key.Key == ConsoleKey.Tab)
                {
                    if (key.Modifiers == ConsoleModifiers.Control)
                    {
                        if (_windows.Any())
                        {
                            var top = _windows[_windows.Count - 1];
                            _windows.RemoveAt(_windows.Count-1);
                            _windows.Insert(0, top);
                            _needRedraw = true;
                        }
                        continue;
                    }
                    if (key.Modifiers == (ConsoleModifiers.Control | ConsoleModifiers.Shift))
                    {
                        if (_windows.Any())
                        {
                            var top = _windows[0];
                            _windows.RemoveAt(0);
                            _windows.Add(top);
                            _needRedraw = true;
                        }
                        continue;
                    }
                }

                // Toggle to stdout?
                if (key.Key == ConsoleKey.F4 && key.Modifiers == 0)
                {
                    ViewMode = ViewMode.StdOut;
                    Console.ReadKey(true);
                    ViewMode = ViewMode.Desktop;
                    continue;
                }

                // Preview key event
                if (OnPreviewKey(key))
                    continue;

                // Send key to the active window
                var aw = ActiveWindow;
                if (aw!= null)
                {
                    aw.OnKey(key);
                }
            }

            // Notify
            OnLeaveProcessing();

            // Finished
            return;
        }

        // Bring the console window to foreground
        IntPtr _oldForegroundWindow;
        public void BringToFront()
        {
            _oldForegroundWindow = Interop.GetActiveWindow();
            Interop.SetForegroundWindow(Interop.GetConsoleWindow());
        }

        // Restore the old active foreground window
        public void RestoreForegroundWindow()
        {
            if (_oldForegroundWindow==IntPtr.Zero)
            {
                _oldForegroundWindow = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            }
            if (_oldForegroundWindow !=IntPtr.Zero)
            {
                Interop.SetForegroundWindow(_oldForegroundWindow);
                _oldForegroundWindow = IntPtr.Zero;
            }
        }

        // Set the view mode (desktop or stdout)
        ViewMode _viewMode = ViewMode.StdOut;
        public ViewMode ViewMode
        {
            get
            {
                return _viewMode;
            }
            set
            {
                if (_viewMode !=value)
                {
                    _viewMode = value;
                    if (_viewMode==ViewMode.Desktop)
                    {
                        Interop.SetConsoleActiveScreenBuffer(_buffer);
                    }
                    else
                    {
                        Interop.SetConsoleActiveScreenBuffer(_stdout);
                    }
                }
            }
        }
    }
}
