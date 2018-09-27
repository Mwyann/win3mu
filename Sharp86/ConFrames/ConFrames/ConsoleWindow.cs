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
    public class ConsoleWindow : Window
    {
        public ConsoleWindow(string title, Rect frameRectangle) : base(title, frameRectangle)
        {
            _buffer.Add(null);
            ScrollPos = 0;
            _prompt = ">";
            CursorVisible = true;
            CursorPosition = new Point(_prompt.Length, 0);
        }

        public string Prompt
        {
            get { return _prompt; }
            set { _prompt = value; }
        }

        public void Write(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;

            str = str.Replace("\r\n", "\n").Replace('\r', '\n');
            var lines = str.Split('\n');

            bool trailingCR = str.EndsWith("\n");
            int lineCount = trailingCR ? lines.Length - 1 : lines.Length;

            int oldBufferSize = _buffer.Count;

            for (int i=0; i < lineCount; i++)
            {
                int count = _buffer.Count;
                if (_buffer[count - 1] == null)
                    _buffer[count - 1] = lines[i];
                else
                    _buffer[count - 1] = _buffer[count - 1] + lines[i];

                if (i < lineCount-1)
                    _buffer.Add(null);
            }

            if (trailingCR)
                _buffer.Add(null);

            CursorY += _buffer.Count - oldBufferSize;

            Invalidate();
            ScrollToBottom();
        }

        public void WriteLine(string str)
        {
            Write(str);
            Write("\n");
        }

        public void WriteLine(string str, params object[] args)
        {
            WriteLine(string.Format(str, args));
        }

        public override void OnPaint(PaintContext ctx)
        {
            base.OnPaint(ctx);

            var height = ClientSize.Height;
            if (_prompt!=null)
                height--;

            int firstLine = ScrollPos;
            int lastLine = ScrollPos + height + 1;

            if (lastLine > TrimmedBufferLines)
                lastLine = TrimmedBufferLines;

            if (firstLine < _buffer.Count)
            {
                for (int i=firstLine; i<lastLine; i++)
                {
                    var line = _buffer[i];
                    ctx.WriteLine(line == null ? "" : line);
                }
            }

            if (lastLine == TrimmedBufferLines && _prompt!= null)
            {
                ctx.Write(_prompt);
                ctx.WriteLine(_inputBuffer);
            }
        }

        int _scrollPos;
        public int ScrollPos
        {
            get
            {
                return _scrollPos;
            }
            set
            {
                if (value < 0)
                    value = 0;
                if (value > TrimmedBufferLines)
                    value = TrimmedBufferLines;

                if (_scrollPos == value)
                    return;

                CursorY += _scrollPos - value;
                _scrollPos = value;
                Invalidate();
            }
        }

        List<string> _buffer = new List<string>();
        string _prompt = null;

        string _inputBuffer = "";

        void RedrawPrompt()
        {
            var ctx = GetPaintContext();

            ctx.ClearLineOnReturn = true;
            ctx.Position = new Point(0, TrimmedBufferLines - ScrollPos);
            ctx.Write(_prompt);
            ctx.WriteLine(_inputBuffer);
        }

        int TrimmedBufferLines
        {
            get
            {
                int count = _buffer.Count;
                if (_buffer[count - 1] == null)
                    count--;
                return count;
            }
        }


        void ScrollToBottom()
        {
            ScrollPos = _buffer.Count - ClientSize.Height;
            if (ScrollPos < 0)
                ScrollPos = 0;
        }

        public override bool OnKey(ConsoleKeyInfo key)
        {
            int posInBuffer = CursorX - _prompt.Length;

            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                    if (posInBuffer > 0)
                        CursorX--;
                    ScrollToBottom();
                    return true;

                case ConsoleKey.RightArrow:
                    if (posInBuffer < _inputBuffer.Length)
                        CursorX++;
                    ScrollToBottom();
                    return true;

                case ConsoleKey.UpArrow:
                    if (key.Modifiers == ConsoleModifiers.Shift)
                        ScrollPos--;
                    else
                        OnCommandHistory(-1);
                    return true;

                case ConsoleKey.DownArrow:
                    if (key.Modifiers == ConsoleModifiers.Shift)
                        ScrollPos++;
                    else
                        OnCommandHistory(1);
                    return true;

                case ConsoleKey.Home:
                    CursorX = _prompt.Length;
                    ScrollToBottom();
                    return true;

                case ConsoleKey.End:
                    CursorX = _prompt.Length + _inputBuffer.Length;
                    ScrollToBottom();
                    return true;

                case ConsoleKey.Backspace:
                    if (posInBuffer > 0)
                    {
                        _inputBuffer = _inputBuffer.Substring(0, posInBuffer - 1) + _inputBuffer.Substring(posInBuffer);
                        RedrawPrompt();
                        CursorX--;
                    }
                    ScrollToBottom();
                    return true;

                case ConsoleKey.Delete:
                    if (posInBuffer < _inputBuffer.Length)
                    {
                        _inputBuffer = _inputBuffer.Substring(0, posInBuffer) + _inputBuffer.Substring(posInBuffer + 1);
                        RedrawPrompt();
                    }
                    ScrollToBottom();
                    return true;

                case ConsoleKey.Escape:
                    _inputBuffer = "";
                    RedrawPrompt();
                    CursorX = _prompt.Length;
                    ScrollToBottom();
                    return true;

                case ConsoleKey.Enter:
                {
                    WriteLine(_prompt + _inputBuffer);
                    string str = _inputBuffer;
                    _inputBuffer = "";
                    CursorX = _prompt.Length;
                    ScrollToBottom();
                    OnCommand(str);
                    return true;
                }
            }

            if (key.KeyChar!=0)
            {
                _inputBuffer = _inputBuffer.Substring(0, posInBuffer) + key.KeyChar + _inputBuffer.Substring(posInBuffer);
                CursorX++;
                ScrollToBottom();
                RedrawPrompt();                              
                return true;
            }

            return false;
        }

        public void SetInputBuffer(string str)
        {
            _inputBuffer = str;
            RedrawPrompt();
            CursorX = _prompt.Length + _inputBuffer.Length;
            ScrollToBottom();
        }

        List<string> _commandHistory = new List<String>();
        int _commandHistoryPos;

        protected virtual void OnCommand(string command)
        {
            if (_commandHistory.Count>0)
            {
                if (_commandHistory[_commandHistory.Count - 1] == command)
                {
                    _commandHistoryPos = _commandHistory.Count;
                     return;
                }
            }

            _commandHistory.Add(command);
            _commandHistoryPos = _commandHistory.Count;
        }

        void OnCommandHistory(int delta)
        {
            int newPos = _commandHistoryPos + delta;
            if (newPos < 0)
                return;
            if (newPos >= _commandHistory.Count)
            {
                SetInputBuffer("");
                return;
            }
            else
            {
                _commandHistoryPos = newPos;
                SetInputBuffer(_commandHistory[newPos]);
            }
        }
    }
}
