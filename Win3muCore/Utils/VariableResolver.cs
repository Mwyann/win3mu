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
using System.Text;
using System.Threading.Tasks;

namespace Win3muCore
{
    public class VariableResolver
    {
        public VariableResolver()
        {
            RegisterStandardVariables();
        }

        public IEnumerable<string> AllVariables
        {
            get { return _variables.Keys; }
        }

        public void Register(string variable, Func<string> resolver)
        {
            if (resolver == null)
            {
                _variables.Remove(variable);
            }
            else
            {
                _variables[variable] = resolver;
            }
        }

        public void Revoke(string variable)
        {
            _variables.Remove(variable);
        }

        abstract class Token
        {
            public abstract string Resolve();
            public virtual int Width { get { return 0; } }
        };

        class LiteralToken : Token
        {
            public LiteralToken(string text)
            {
                _text = text;
            }

            string _text;

            public override string Resolve()
            {
                return _text;
            }
        }

        class VariableToken : Token
        {
            public VariableToken(string name, int width, Func<string> resolver)
            {
                _name = name;
                _width = width;
                _resolver = resolver;
            }

            string _name;
            int _width;
            Func<string> _resolver;

            public string Name
            {
                get { return _name; }
            }

            public override int Width
            {
                get { return _width; }
            }

            public override string Resolve()
            {
                return _resolver();
            }
        }

        class TokenizedString : List<Token>
        {
            public List<Action> Monitors;
        };

        public string StripVariables(string str)
        {
            var t = TokenizeString(str) as TokenizedString;

            var sb = new StringBuilder();
            foreach (var token in t)
            {
                if (token is LiteralToken)
                {
                    sb.Append(token.Resolve());
                }
            }

            return sb.ToString();
        }


        public object TokenizeString(string str)
        {
            if (str == null)
                return null;

            var p = new StringPointer(str);

            var tokens = new TokenizedString();

            var sb = new StringBuilder();

            while (!p.EOF)
            {
                if (p.Skip("$("))
                {
                    var start = p.Position;
                    if (p.SkipUntilOnOf(')', ','))
                    {
                        var variableName = p.Extract(start, p.Position);

                        int width = 0;
                        if (p[0] == ',')
                        {
                            p++;
                            int multplier = 1;
                            if (p[0] == '-')
                            {
                                multplier = -1;
                                p++;
                            }

                            p.ParseInt(out width);
                            width *= multplier;
                        }

                        p++;
                        Func<string> resolver;
                        if (_variables.TryGetValue(variableName, out resolver))
                        {
                            if (sb.Length > 0)
                            {
                                tokens.Add(new LiteralToken(sb.ToString()));
                                sb.Length = 0;
                            }

                            tokens.Add(new VariableToken(variableName, width, resolver));
                        }
                        else
                        {
                            sb.AppendFormat("$({0})", variableName);
                        }
                    }
                    else
                    {
                        sb.Append("$(");
                    }
                }
                else
                {
                    sb.Append(p[0]);
                    p++;
                }
            }

            if (sb.Length > 0)
            {
                tokens.Add(new LiteralToken(sb.ToString()));
            }

            return tokens;
        }

        public string ResolveTokenizedString(object obj)
        {
            var tokens = obj as TokenizedString;
            if (tokens == null)
                return null;

            _resolveTime = DateTime.Now;

            var sb = new StringBuilder();
            for (int i = 0; i < tokens.Count; i++)
            {
                int width = tokens[i].Width;

                var value = tokens[i].Resolve().ToString();

                if (width < 0)
                {
                    width = -width;
                    sb.Append(value);
                    if (width > value.Length)
                        sb.Append(new string(' ', width - value.Length));
                }
                else
                {
                    if (width > value.Length)
                        sb.Append(new string(' ', width - value.Length));
                    sb.Append(value);
                }


            }

            return sb.ToString();
        }

        public static bool DoesContainVariables(object obj)
        {
            var tokens = obj as TokenizedString;
            if (tokens == null)
                return false;

            if (tokens.Count > 1)
                return true;

            return !(tokens[0] is LiteralToken);
        }

        public string Resolve(string str)
        {
            if (str == null)
                return null;

            var tokens = TokenizeString(str);
            return ResolveTokenizedString(tokens);
        }

        public void StartMonitoring(object tokens, Action callback)
        {
            var t = tokens as TokenizedString;
            if (t == null)
                return;

            if (t.Monitors == null)
                t.Monitors = new List<Action>();
            t.Monitors.Add(callback);

            if (t.Monitors.Count == 1)
            {
                foreach (var varName in t.OfType<VariableToken>().Select(x => x.Name).Distinct())
                {
                    List<TokenizedString> listStrings;
                    if (!_monitors.TryGetValue(varName, out listStrings))
                    {
                        listStrings = new List<TokenizedString>();
                        _monitors.Add(varName, listStrings);
                    }

                    listStrings.Add(t);
                }
            }
        }

        public void StopMonitoring(object tokens, Action callback = null)
        {
            var t = tokens as TokenizedString;
            if (t == null || t.Monitors == null)
                return;

            // Remove the listener from the tokenized string
            if (callback == null)
            {
                t.Monitors.Clear();
            }
            else
            {
                t.Monitors.Remove(callback);
            }

            // If no more monitors, remove from variable map
            if (t.Monitors.Count == 0)
            {
                foreach (var varName in t.OfType<VariableToken>().Select(x => x.Name).Distinct())
                {
                    List<TokenizedString> listStrings;
                    if (_monitors.TryGetValue(varName, out listStrings))
                    {
                        listStrings.Remove(t);
                    }
                }
            }
        }


        int _updateDepth = 0;
        HashSet<TokenizedString> _changedStrings = new HashSet<TokenizedString>();
        public void StartUpdates()
        {
            if (_updateDepth == -1)
                return;

            _updateDepth++;
        }

        public void EndUpdates()
        {
            if (_updateDepth == -1)
                return;

            _updateDepth--;
            if (_updateDepth != 0)
                return;

            _updateDepth = -1;

            // Fire updates
            foreach (var s in _changedStrings)
            {
                foreach (var cb in s.Monitors)
                {
                    cb();
                }
            }

            // Clear
            _changedStrings.Clear();

            _updateDepth = 0;
        }

        public void VariableChanged(string name)
        {
            if (_updateDepth == -1)
                return;

            StartUpdates();

            List<TokenizedString> affectedStrings;
            if (_monitors.TryGetValue(name, out affectedStrings))
            {
                foreach (var str in affectedStrings)
                {
                    _changedStrings.Add(str);
                }
            }

            EndUpdates();
        }

        DateTime _lastTime = DateTime.Now;
        public void UpdateTimeBasedVariables()
        {
            var newTime = DateTime.Now;

            StartUpdates();

            if (newTime.Second != _lastTime.Second)
            {
                VariableChanged("sec");
            }

            if (newTime.Minute != _lastTime.Minute)
            {
                VariableChanged("min");
            }

            if (newTime.Hour != _lastTime.Hour)
            {
                VariableChanged("h");
                VariableChanged("hh");
                VariableChanged("h24");
                VariableChanged("hh24");
            }

            if ((newTime.Hour < 12) != (_lastTime.Hour < 12))
            {
                VariableChanged("am");
            }

            if (newTime.Day != _lastTime.Day)
            {
                VariableChanged("dy");
                VariableChanged("day");
                VariableChanged("d");
                VariableChanged("dd");

                if (WeekNumberFromDateTime(newTime) != WeekNumberFromDateTime(_lastTime))
                {
                    VariableChanged("w");
                    VariableChanged("ww");
                    VariableChanged("wm");
                    VariableChanged("wwm");
                }
            }

            if (newTime.Month != _lastTime.Month)
            {
                VariableChanged("m");
                VariableChanged("mm");
                VariableChanged("mmm");
                VariableChanged("mmmm");
            }

            if (newTime.Year != _lastTime.Year)
            {
                VariableChanged("yy");
                VariableChanged("yyyy");
            }

            _lastTime = newTime;

            EndUpdates();
        }


        Dictionary<string, List<TokenizedString>> _monitors = new Dictionary<string, List<TokenizedString>>(StringComparer.InvariantCultureIgnoreCase);
        Dictionary<string, Func<string>> _variables = new Dictionary<string, Func<string>>(StringComparer.InvariantCultureIgnoreCase);
        DateTime _resolveTime;

        int WeekNumberFromDateTime(DateTime dt)
        {
            var year = new DateTime(dt.Year, 1, 1);
            var yearStart = (int)(year.ToOADate() - (int)year.DayOfWeek);
            return (int)((dt.ToOADate() - yearStart) / 7) + 1;
        }

        int WeekNumber
        {
            get
            {
                return WeekNumberFromDateTime(_resolveTime);
            }
        }

        int WeekOfMonth
        {
            get
            {
                var month = new DateTime(_resolveTime.Year, _resolveTime.Month, 1);
                var monthStart = (int)(month.ToOADate() - (int)month.DayOfWeek);
                return (int)((_resolveTime.ToOADate() - monthStart) / 7) + 1;
            }
        }

        int Hour
        {
            get
            {
                int h = _resolveTime.Hour % 12;
                return h == 0 ? 12 : h;
            }
        }

        void RegisterStandardVariables()
        {
            // Special folders
            Register("AppData", () => System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            Register("CommonAppData", () => System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            Register("CommonDesktop", () => System.Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory));
            Register("Desktop", () => System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            Register("Favorites", () => System.Environment.GetFolderPath(Environment.SpecialFolder.Favorites));
            Register("MyDocuments", () => System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            Register("MyMusic", () => System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
            Register("MyPictures", () => System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            Register("ProgramFiles", () => System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            Register("ProgramFilesX86", () => System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
            Register("System", () => System.Environment.GetFolderPath(Environment.SpecialFolder.System));
            Register("SystemX86", () => System.Environment.GetFolderPath(Environment.SpecialFolder.SystemX86));
            Register("Temp", () => System.IO.Path.GetTempPath());
            Register("Windows", () => System.Environment.GetFolderPath(Environment.SpecialFolder.Windows));
        }
    }
}
