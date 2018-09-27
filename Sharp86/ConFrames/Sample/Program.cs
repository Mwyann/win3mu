using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConFrames;

namespace Sample
{
    class Window1 : Window
    {
        public Window1() :base("Window 1", new Rect(0, 0, 50, 20))
        {
        }

        public override void OnPaint(PaintContext ctx)
        {
            ctx.ForegroundColor = ConsoleColor.Cyan;
            for (int i=0; i<20; i++)
            {
                ctx.WriteLine("This is line {0}", _baseNumber + i);
            }
        }

        public override bool OnKey(ConsoleKeyInfo key)
        {
            _baseNumber += 5;
            Invalidate();
            return base.OnKey(key);
        }

        int _baseNumber = 1;
    }

    class Window2 : Window
    {
        public Window2() : base("Window 2", new Rect(50, 0, 50, 20))
        {
            CursorVisible = true;
            CursorX = 5;
            CursorY = 1;
        }

        public override void OnPaint(PaintContext ctx)
        {
            ctx.WriteLine("Type something, or use Ctrl+Tab to switch windows");
            ctx.Write("blah>");
        }

        public override bool OnKey(ConsoleKeyInfo key)
        {
            if (key.KeyChar != 0)
            {
                var ctx = GetPaintContext();
                ctx.SetChar(CursorX++, CursorY, key.KeyChar);
            }
            else
            {
                switch (key.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (CursorX > 0)
                            CursorX--;
                        break;

                    case ConsoleKey.RightArrow:
                        if (CursorX < ClientSize.Width)
                            CursorX++;
                        break;

                }
            }
            return base.OnKey(key);
        }
    }

    class CommandWindow : ConsoleWindow
    {
        public CommandWindow() : base("Command", new Rect(0, 20, 100, 20))
        {
            Prompt = ">";
            WriteLine("Type something and press enter.  Use Shift+Up/Down to scroll");
        }

        protected override void OnCommand(string command)
        {
            WriteLine("You typed: '{0}'", command);

            // Call base to update command history (up/down arrow)
            base.OnCommand(command);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var desktop = new Desktop(100, 40);
            var w1 = new Window1();
            var w2 = new Window2();
            var cmd = new CommandWindow();
            w1.Open(desktop);
            w2.Open(desktop);
            cmd.Open(desktop);

            desktop.Process();
        }
    }
}
