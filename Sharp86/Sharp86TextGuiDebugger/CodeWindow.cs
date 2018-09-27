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

using System;
using System.Collections.Generic;
using ConFrames;

namespace Sharp86
{
    public class CodeWindow : Window
    {
        public CodeWindow(TextGuiDebugger owner) : base("Code", new Rect(0, 0, 100, 18))
        {
            _dis = new Disassembler();
            _debugger = owner;
            ClearAttributes = ConFrames.Attribute.Make(ConsoleColor.Gray, ConsoleColor.Blue);
            CursorVisible = true;
            CursorX = 0;

            _debugger.SettingsChanged += () => Invalidate();
        }

        Disassembler _dis;
        TextGuiDebugger _debugger;

        ushort _renderedCS;
        List<ushort> _renderedIPs = new List<ushort>();

        ushort _topLineCS;
        ushort _topLineIP;

        public override void OnPaint(PaintContext ctx)
        {
            ctx.ClearLineOnReturn = true;

            // Setup disassembler
            _dis.MemoryBus = _debugger.CPU.MemoryBus;
            _dis.cs = _topLineCS;
            _dis.ip = _topLineIP;

            /*
            // See if the current ip is already on screen
            if (_renderedCS == _caretCS)
            {
                int line = _renderedIPs.IndexOf(_caretIP);
                if (line>=0)
                {
                    if (line > ClientSize.Height - 3)
                    {
                        _dis.ip = _renderedIPs[line - (ClientSize.Height - 3)];
                    }
                    else
                    {
                        _dis.ip = _renderedIPs[0];
                    }
                }
            }


            */

            // Remember the current set of displayed instruction pointers
            _renderedCS = _dis.cs;
            _renderedIPs.Clear();


            for (int i=0; i<ClientSize.Height; i++)
            {
                ctx.Attributes = this.ClearAttributes;
                var bp = _debugger.FindCodeBreakPoint(_dis.cs, _dis.ip);
                if (bp != null && bp.Enabled)
                {
                    ctx.BackgroundColor = ConsoleColor.Red;
                    ctx.ForegroundColor = ConsoleColor.White;
                }

                string annot = "";
                char arrow;
                if (_dis.ip == _debugger.CPU.ip)
                {
                    ctx.BackgroundColor = ConsoleColor.Yellow;
                    ctx.ForegroundColor = ConsoleColor.Black;
                    arrow = '→';
                }
                else
                {
                    arrow = ' ';
                }

                _renderedIPs.Add(_dis.ip);

                var ipPos = _dis.ip;

                ctx.Write("{0:X4}:{1:X4} ", _dis.cs, _dis.ip);

                var asm = _dis.Read();

                if (ipPos == _debugger.CPU.ip)
                {
                    annot = _debugger.ExpressionContext.GenerateDisassemblyAnnotations(asm, _dis.ImplicitParams);
                    if (!string.IsNullOrWhiteSpace(annot))
                    {
                        annot = "   ; " + annot;
                    }
                }

                    for (ushort b = 0; b<6; b++)
                {
                    if (ipPos + b < _dis.ip)
                    {
                        try
                        {
                            ctx.Write("{0:X2} ", _debugger.MemoryBus.ReadByte(_dis.cs, (ushort)(ipPos + b)));
                        }
                        catch (CPUException)
                        {
                            ctx.Write("?? ");
                        }
                    }
                    else
                    {
                        ctx.Write("   ");
                    }
                }

                ctx.WriteLine("{0} {1,3} {2} {3}", arrow, bp == null ? "" : ("#" + bp.Number.ToString()), asm, annot);
            }
        }

        public void MoveToIP()
        {
            // Already on screen?
            if (_renderedCS == _debugger.CPU.cs)
            {
                int line = _renderedIPs.IndexOf(_debugger.CPU.ip);
                if (line>=0)
                {
                    if (line + 5 > _renderedIPs.Count)
                    {
                        _topLineIP = _renderedIPs[line + 5 - _renderedIPs.Count];
                        CursorY = line - 1;
                    }
                    else
                    {
                        CursorY = line;
                    }
                    Invalidate();
                    return;
                }
            }

            _topLineCS = _debugger.CPU.cs;
            _topLineIP = _debugger.CPU.ip;
            CursorY = 0;
            Invalidate();
        }

        public void MoveTo(ushort cs, ushort ip, int onLine)
        {
            _topLineCS = cs;
            _topLineIP = FindPriorIP(cs, ip, onLine);
            Invalidate();
        }

        public override bool OnKey(ConsoleKeyInfo key)
        {
            // Hex digit?
            int hexDigit = -1;
            if (key.KeyChar >= '0' && key.KeyChar <= '9')
            {
                hexDigit = key.KeyChar - '0';
            }
            else if (key.KeyChar >= 'A' && key.KeyChar <= 'F')
            {
                hexDigit = key.KeyChar - 'A' + 10;
            }
            else if (key.KeyChar >= 'a' && key.KeyChar <= 'f')
            {
                hexDigit = key.KeyChar - 'a' + 10;
            }

            if (hexDigit >= 0)
            {
                if (CursorX == 4)
                    CursorX = 5;

                if (CursorPosition.X >= 0 && CursorPosition.X <= 3)
                {
                    var shift = (3 - CursorPosition.X) * 4;
                    var mask = 0x0F << shift;
                    MoveTo((ushort)((_topLineCS & ~mask) | (hexDigit << shift)), _topLineIP, CursorY);

                    CursorX++;
                    Invalidate();
                    return true;
                }

                if (CursorPosition.X >= 5 && CursorPosition.X <= 8)
                {
                    var shift = (3 - (CursorPosition.X - 5)) * 4;
                    var mask = 0x0F << shift;
                    MoveTo(_topLineCS, (ushort)((_renderedIPs[CursorY] & ~mask) | (hexDigit << shift)), CursorY);

                    CursorX++;
                    Invalidate();
                    return true;
                }
            }

            if (key.KeyChar == ' ' || key.KeyChar == ':')
            {
                if (CursorX < 74)
                    CursorX++;
            }

            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                case ConsoleKey.Backspace:
                    if (CursorPosition.X > 0)
                        CursorX--;
                    break;

                case ConsoleKey.RightArrow:
                    if (CursorPosition.X < 74)
                        CursorX++;
                    break;

                case ConsoleKey.UpArrow:
                    if (CursorY > 0)
                    {
                        CursorY--;
                    }
                    else
                    {
                        _topLineIP = FindPriorIP(_topLineCS, _topLineIP, 1);
                        Invalidate();
                    }
                    break;

                case ConsoleKey.DownArrow:
                    if (CursorY+1 < ClientSize.Height)
                    {
                        CursorY++;
                    }
                    else
                    {
                        _topLineIP = _renderedIPs[1];
                        Invalidate();
                    }
                    break;

                case ConsoleKey.F9:
                    _debugger.ToggleCodeBreakPoint(_renderedCS, _renderedIPs[CursorY]);
                    Invalidate();
                    break;

                case ConsoleKey.F10:
                    if (key.Modifiers == ConsoleModifiers.Control)
                    {
                        _debugger.ExecuteCommand(string.Format("r to 0x{0:X4}:0x{1:X4}", _renderedCS, _renderedIPs[CursorY]));
                    }
                    break;
            }
            return true;
        }

        ushort FindPriorIP(ushort cs, ushort ip, int instructions)
        {
            if (instructions == 0)
                return ip;

            ushort retryOffset = 0;
            var ips = new List<ushort>();


            retry:
            // Go back 4x number of instructions that we want to go back by
            var goBackBytes = 10 + (ushort)(instructions * 20);
            ushort dip;
            if (goBackBytes > ip)
                dip = 0;
            else
                dip = (ushort)(ip - goBackBytes);

            dip -= retryOffset;

            // Disassamble from the address until we go past the current ip
            _dis.cs = cs;
            _dis.ip = dip;

            // Couldn't find a match
            if (_dis.ip == ip)
                return ip;

            // Build a list of ip addresses
            ips.Clear();
            while (_dis.ip < ip)
            {
                ips.Add(_dis.ip);
                _dis.Read();
            }

            // Did we hit it?
            if (_dis.ip != ip)
            {
                if (dip == 0)
                    return 0;
                retryOffset++;
                if (retryOffset < 10)
                    goto retry;
            }

            // Find the nth previous instruction
            return ips[ips.Count - instructions];
        }
    }
}
