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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Win3muCore
{
    public static class Log
    {                                    
        static TextWriter output1 = null;
        static TextWriter output2 = null;

        public static void Init(bool console, string outputFile)
        {
            if (console)
                output1 = Console.Out;

            if (outputFile != null)
            {
                // Make sure the folder exists
                var folder = System.IO.Path.GetDirectoryName(outputFile);
                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);

                output2 = new StreamWriter(outputFile, false, Encoding.UTF8);
            }
        }

        public static void Close()
        {
            if (output2 != null)
            {
                output2.Close();
                output2.Dispose();
            }
        }

        public static void Flush()
        {
            if (output2 != null)
                output2.Flush();
        }

        public static void Write(string str)
        {
            if (output1 != null)
                output1.Write(str);
            if (output2 != null)
                output2.Write(str);
        }

        public static void WriteLine(string str="")
        {
            if (output1 != null)
                output1.WriteLine(str);
            if (output2 != null)
                output2.WriteLine(str);
        }

        public static void WriteLine(string str, params object[] args)
        {
            if (output1 != null)
                output1.WriteLine(string.Format(str, args));
            if (output2 != null)
                output2.WriteLine(string.Format(str, args));
        }

        public static void Write(string str, params object[] args)
        {
            if (output1 != null)
                output1.Write(string.Format(str, args));
            if (output2 != null)
                output2.Write(string.Format(str, args));
        }
    }
}
