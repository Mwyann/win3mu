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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConFrames;

namespace Sharp86
{
    public class WatchWindow : Window
    {
        public WatchWindow(TextGuiDebugger owner) : base("Watch", new Rect(80, 18, 40, 22))
        {
            _owner = owner;
            _owner.SettingsChanged += () => Invalidate();
        }

        public void OnBreak()
        {
            Invalidate();
        }

        public override void OnPaint(PaintContext ctx)
        {
            ctx.ForegroundColor = ConsoleColor.Gray;

            int y = 0;
            foreach (var w in _owner.WatchExpressions)
            {
                ctx.Position = new Point(0, y);
                ctx.Write(string.Format("#{0,2}: {1}", w.Number, string.IsNullOrEmpty(w.Name) ? w.ExpressionText : w.Name));

                var val = w.EvalAndFormat(_owner);
                var x = ClientSize.Width - val.Length;
                if (x < 5)
                    x = 5;
                ctx.Position = new Point(x, y);
                ctx.Write(val);
                y++;
            }
        }

        TextGuiDebugger _owner;
    }
}
