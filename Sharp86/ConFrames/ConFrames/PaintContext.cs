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
    public class PaintContext
    {
        public PaintContext(CharInfo[] buf, Size bufSize, Rect drawRect)
        {
            _x = 0;
            _y = 0;
            _buf = buf;
            _bufSize = bufSize;
            _drawRect = drawRect;
            _clipRect = drawRect;
            _clipRect.Intersect(new Rect(0, 0, bufSize.Width, bufSize.Height));
            _baseOffset = _clipRect.Top * bufSize.Width + _clipRect.Left;
        }

        public PaintContext(PaintContext underlying, Rect drawRect)
        {
            _buf = underlying._buf;
            _bufSize = underlying._bufSize;
            _drawRect = drawRect;
            _clipRect = drawRect;
            _clipRect.Intersect(underlying._clipRect);
            _clipRect.Left += underlying._clipRect.Left;
            _clipRect.Top += underlying._clipRect.Top;
            _clipRect.Intersect(new Rect(0, 0, _bufSize.Width, _bufSize.Height));
            _baseOffset = _clipRect.Top * _bufSize.Width + _clipRect.Left;
        }

        CharInfo[] _buf;            // The buffer to draw to
        Size _bufSize;              // Size of the buffer
        Rect _drawRect;             // Drawing rectangle
        Rect _clipRect;             // Clip rectangle
        int _baseOffset;            // Origin of first character
        int _x;                     // Current output position
        int _y;                     // Current output position

        // Left margin for indent text writes
        public int LeftMargin
        {
            get;
            set;
        }

        // Whether to word wrap output text or not
        public bool WordWrap
        {
            get;
            set;
        }

        // Foreground color
        ushort _attributes;
        public ConsoleColor ForegroundColor
        {
            get { return (ConsoleColor)(_attributes & 0x0F); }
            set { _attributes = (ushort)((_attributes & 0xF0) | (ushort)value); }
        }

        // Background color
        public ConsoleColor BackgroundColor
        {
            get { return (ConsoleColor)((_attributes & 0xF0) >> 4); }
            set { _attributes = (ushort)((_attributes & 0x0F) | ((ushort)value) << 4); }
        }

        // Current fore/back color attributes
        public ushort Attributes
        {
            get
            {
                return _attributes;
            }
            set
            {
                _attributes = value;
            }
        }

        // Clear to end of line when outputting a carriage return
        bool _clearLineOnReturn;
        public bool ClearLineOnReturn
        {
            get { return _clearLineOnReturn; }
            set { _clearLineOnReturn = value; }
        }

        // Write text at the current position (and update position)
        public void Write(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '\r')
                {
                    // Clear to eol
                    if (_clearLineOnReturn)
                    {
                        if (_y>=0 && _y<_clipRect.Height)
                        {
                            int pos = _baseOffset + _bufSize.Width * _y + _x;
                            for (int x = _x; x < _clipRect.Width; x++, pos++)
                            {
                                _buf[pos].Char = ' ';
                                _buf[pos].Attributes = _attributes;
                            }
                        }
                    }
                    _x = LeftMargin;
                }
                else if (str[i] == '\n')
                {
                    _y++;
                }
                else
                {
                    if (_x >= 0 && _y >= 0 && _x < _clipRect.Width && _y < _clipRect.Height)
                    {
                        int pos = _baseOffset + _bufSize.Width * _y + _x;
                        _buf[pos].Char = str[i];
                        _buf[pos].Attributes = _attributes;
                    }

                    _x++;

                    if (WordWrap && _x >= _clipRect.Width)
                    {
                        _x = LeftMargin;
                        _y++;
                    }
                }
            }
        }

        // Write string and a carriage return
        public void WriteLine(string str="")
        {
            Write(str);
            Write("\r\n");
        }

        // Write formatted text
        public void Write(string str, params object[] args)
        {
            Write(string.Format(str, args));
        }

        // Write formatted line
        public void WriteLine(string str, params object[] args)
        {
            WriteLine(string.Format(str, args));
        }

        // Current position
        public Point Position
        {
            get { return new Point(_x, _y); }
            set { _x = value.X; _y = value.Y; }
        }

        // Clear the entire paint context
        public void Clear()
        {
            // Clear buffer
            for (int y = 0; y<_clipRect.Height; y++)
            {
                int pos = _baseOffset + _bufSize.Width * y;
                for (int x = 0; x < _clipRect.Width; x++, pos++)
                {
                    _buf[pos].Attributes = _attributes;
                    _buf[pos].Char = ' ';
                }
            }
        }

        // Set a specific character
        public void SetChar(int x, int y, char ch)
        {
            if (x >= 0 && x < _clipRect.Width &&
                y >= 0 && y < _clipRect.Height)
            {
                var pos = _baseOffset + _bufSize.Width * y + x;
                _buf[pos].Char = ch;
                _buf[pos].Attributes = _attributes;
            }
        }

        // Set a character but don't set attributes
        public void SetCharNoAttr(int x, int y, char ch)
        {
            if (x >= 0 && x < _clipRect.Width &&
                y >= 0 && y < _clipRect.Height)
            {
                var pos = _baseOffset + _bufSize.Width * y + x;
                _buf[pos].Char = ch;
            }
        }

        // Set a character with supplied attributes
        public void SetChar(int x, int y, char ch, ushort attributes)
        {
            if (x >= 0 && x < _clipRect.Width &&
                y >= 0 && y < _clipRect.Height)
            {
                var pos = _baseOffset + _bufSize.Width * y + x;
                _buf[pos].Char = ch;
                _buf[pos].Attributes = attributes;
            }
        }

        // Draw a box
        public void DrawBox(Rect rect, bool doubleLine)
        {
            // Draw the box
            var offsBottom = (rect.Height - 1) * rect.Width;
            var offsRight = rect.Width - 1;

            // Work out character set
            char[] boxDraw;
            if (doubleLine)
            {
                boxDraw = new char[] { '╔', '╗', '╚', '╝', '═', '═', '║', '║' };
            }
            else
            {
                boxDraw = new char[] { '┌', '┐', '└', '┘', '─', '─', '│', '│' };
            }

            // Corners
            SetChar(rect.Left, rect.Top, boxDraw[0]);
            SetChar(rect.Right - 1, rect.Top, boxDraw[1]);
            SetChar(rect.Left, rect.Bottom - 1, boxDraw[2]);
            SetChar(rect.Right - 1, rect.Bottom - 1, boxDraw[3]);

            // Top/bottom
            for (int i = 1; i < rect.Width - 1; i++)
            {
                SetChar(i, rect.Top, boxDraw[4]);
                SetChar(i, rect.Bottom - 1, boxDraw[5]);
            }

            // Left/Right
            for (int i = 1; i < rect.Height - 1; i++)
            {
                SetChar(rect.Left, i, boxDraw[6]);
                SetChar(rect.Right - 1, i, boxDraw[7]);
            }

        }
    }
}
