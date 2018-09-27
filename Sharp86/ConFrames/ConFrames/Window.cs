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
using System.Text;
using System.Threading.Tasks;

namespace ConFrames
{
    public class Window
    {
        public Window(string title, Rect frameRectangle)
        {
            Title = title;
            FrameRectangle = frameRectangle;

            _clearAttributes = Attribute.Make(ConsoleColor.White, ConsoleColor.Blue);

            _needsFrameRender = true;
            _needsClientRender = true;
        }

        public int CursorX;
        public int CursorY;

        bool _needsFrameRender;
        bool _needsClientRender;
        bool _frameRenderedActive;
        Rect _frame;
        Desktop _manager;
        CharInfo[] _buf;


        // Current cursor position when this window is active
        public Point CursorPosition
        {
            get { return new Point(CursorX, CursorY); }
            set { CursorX = value.X; CursorY = value.Y; }
        }

        // Show the cursor?
        public bool CursorVisible
        {
            get;
            set;
        }

        // Default attributes to clear paint contexts with
        ushort _clearAttributes;
        public ushort ClearAttributes
        {
            get
            {
                return _clearAttributes;
            }
            set
            {
                _clearAttributes = value;
            }
        }

        // Window title
        string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                _needsFrameRender = true;
                if (_manager != null)
                    _manager.Invalidate(this);
            }
        }

        // Window frame
        public Rect FrameRectangle
        {
            get
            {
                return _frame;
            }
            set
            {
                if (_frame == value)
                    return;

                // Did it change size?
                if (_frame.Size != value.Size)
                {
                    _needsClientRender = true;
                    _needsFrameRender = true;
                    _buf = null;
                }

                // Store new frame
                _frame = value;

                // Re-render desktop
                if (_manager != null)
                {
                    _manager.InvalidateDesktop();
                    _manager.Invalidate(this);
                }
            }
        }

        // Client size
        public Size ClientSize
        {
            get
            {
                return new Size(FrameRectangle.Width - 2, FrameRectangle.Height - 2);
            }
        }

        // Open the window and add to the desktop manager
        public void Open(ConFrames.Desktop manager)
        {
            if (_manager != null)
                throw new InvalidOperationException();

            _manager = manager;
            _manager.AddWindow(this);
        }

        // Close this window and remove from desktop
        public void Close()
        {
            if (_manager != null)
            {
                _manager.RemoveWindow(this);
                _manager = null;
            }
        }

        // Override to paint the context of this window
        public virtual void OnPaint(PaintContext ctx)
        {
        }

        // Override receive key events
        public virtual bool OnKey(ConsoleKeyInfo key)
        {
            return false;
        }

        // Force this window to repaint
        public void Invalidate()
        {
            _needsClientRender = true;
            if (_manager != null)
                _manager.Invalidate(this);
        }

        // Is this window active?
        public bool IsActive
        {
            get
            {
                if (_manager == null)
                    return false;
                return _manager.ActiveWindow == this;
            }
        }

        // Activate this window
        public void Activate()
        {
            if (_manager == null)
                return;
            _manager.ActiveWindow = this;
        }


        // Get a paint context for this window
        public PaintContext GetPaintContext()
        {
            // Create buffer?
            if (_buf == null)
            {
                _buf = new CharInfo[FrameRectangle.Width * FrameRectangle.Height];
            }

            var ctx = new PaintContext(_buf, FrameRectangle.Size, new Rect(1, 1, FrameRectangle.Width - 2, FrameRectangle.Height - 2));
            ctx.Attributes = _clearAttributes;
            _manager.Invalidate(this);
            return ctx;
        }

        // Called by manager to draw this window - including frame
        public virtual CharInfo[] Draw()
        {
            // Create buffer?
            if (_buf == null)
            {
                _buf = new CharInfo[FrameRectangle.Width * FrameRectangle.Height];
                _needsFrameRender = true;
                _needsClientRender = true;
            }

            // Create paint context for the frame
            var px = new PaintContext(_buf, new Size(FrameRectangle.Width, FrameRectangle.Height), new Rect(0, 0, FrameRectangle.Width, FrameRectangle.Height));

            // Paint frame?
            if (_needsFrameRender || _frameRenderedActive != IsActive)
            {
                _frameRenderedActive = IsActive;

                // Draw border
                if (IsActive)
                {
                    px.ForegroundColor = _manager.ActiveBorderLineColor;
                    px.BackgroundColor = _manager.ActiveBorderBackgroundColor;
                }
                else
                {
                    px.ForegroundColor = _manager.InactiveBorderLineColor;
                    px.BackgroundColor = _manager.InactiveBorderBackgroundColor;
                }
                px.DrawBox(new Rect(0, 0, FrameRectangle.Width, FrameRectangle.Height), IsActive);

                // Draw title
                if (!string.IsNullOrEmpty(Title))
                {
                    var title = " " + Title + " ";
                    int titleX = (FrameRectangle.Width - title.Length) / 2;
                    if (titleX < 2)
                        titleX = 2;

                    for (int i = 0; i < title.Length && i < FrameRectangle.Width - 4; i++)
                    {
                        _buf[titleX + i].Char = title[i];
                    }
                }
            }

            // Paint client area
            if (_needsClientRender)
            {
                var ctx = new PaintContext(_buf, FrameRectangle.Size, new Rect(1, 1, FrameRectangle.Width - 2, FrameRectangle.Height - 2));
                ctx.Attributes = _clearAttributes;
                ctx.Clear();
                OnPaint(ctx);
            }

            _needsClientRender = false;
            _needsFrameRender = false;

            return _buf;
        }
    }

}
