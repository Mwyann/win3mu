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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Win3muCore.Utils
{
    class CommandLine
    {
        public CommandLine(string[] commandArgs)
        {
            // Split command line args at "--"
            var sepPos = Array.IndexOf(commandArgs, "--");
            if (sepPos >= 0)
            {
                ProcessArgs(commandArgs.Skip(sepPos + 1));
                commandArgs = commandArgs.Take(sepPos).ToArray();
            }

            CommandTail16 = string.Join(" ", commandArgs.Select(x => {
                if (x.Contains(' ') || x.Contains('\t') || x.Contains('\"'))
                    return "\"" + x.Replace("\"", "\\\"") + "\"";
                else
                    return x;
            }));
        }

        public string Config
        {
            get;
            set;
        }

        public bool Break
        {
            get;
            set;
        }

        public bool ProcessArg(string arg)
        {
            if (arg == null)
                return true;

            // Args are in format [/-]<switchname>[:<value>];
            if (arg.StartsWith("/") || arg.StartsWith("-"))
            {
                string SwitchName = arg.Substring(arg.StartsWith("--") ? 2 : 1);
                string Value = null;

                int colonpos = SwitchName.IndexOf(':');
                if (colonpos >= 0)
                {
                    // Split it
                    Value = SwitchName.Substring(colonpos + 1);
                    SwitchName = SwitchName.Substring(0, colonpos);
                }

                switch (SwitchName.ToLower())
                {
                    case "config":
                        Config = Value;
                        break;

                    case "debug":
                        Config = "debug";
                        break;

                    case "release":
                        Config = "release";
                        break;

                    case "break":
                        Break = true;
                        Config = "debug";
                        break;

                    default:
                        throw new InvalidOperationException(string.Format("Unknown switch '{0}'", arg));
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("Unknown command line option - don't know what to do with '{0}'", arg));
            }

            return true;
        }

        public bool ProcessArgs(IEnumerable<string> args)
        {
            if (args == null)
                return true;

            // Parse args
            foreach (var a in args)
            {
                if (!ProcessArg(a))
                    return false;
            }

            return true;
        }

        public string CommandTail16
        {
            get;
            private set;
        }



    }
}
