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
    public static class sprintf
    {
        public class Token
        {
            public char type;
            public int precision;
            public int width;
            public bool leadingZeroes;
            public bool rightAlign;
            public bool hexPrefix;
            public bool isLong;
            public string literal;

            public int StackSize
            {
                get
                {
                    if (literal != null)
                        return 0;

                    if (type == 's')
                        return 4;

                    return isLong ? 4 : 2;
                }
            }
        }
        
        public static List<Token> Parse(string formatString)
        {
            int litPos = 0;
            int i = 0;
            var tokens = new List<Token>();

            formatString += '\0';

            Action Flush = () =>
            {
                if (i > litPos)
                {
                    tokens.Add(new Token()
                    {
                        literal = formatString.Substring(litPos, i - litPos)
                    });
                }
                litPos = i;
            };

            Func<int> ReadInt = () =>
            {
                int val = 0;
                while (char.IsDigit(formatString[i]))
                {
                    val = val * 10 + (formatString[i] - '0');
                    i++;
                }
                return val;
            };

            while (formatString[i]!='\0')
            {
                if (formatString[i]=='%')
                {
                    Flush();

                    i++;

                    var token = new Token();

                    // Modifiers
                modifiers:
                    switch (formatString[i])
                    {
                        case '0':
                            token.leadingZeroes = true;
                            i++;
                            goto modifiers;

                        case '#':
                            token.hexPrefix = true;
                            i++;
                            goto modifiers;

                        case '-':
                            token.rightAlign = true;
                            i++;
                            goto modifiers;
                    }

                    // Precision
                    if (char.IsDigit(formatString[i]))
                    {
                        token.width = ReadInt();
                    }

                    // Width
                    if (formatString[i]=='.')
                    {
                        i++;
                        token.precision = ReadInt();
                    }

                    // Long?
                    if (formatString[i]=='l')
                    {
                        i++;
                        token.isLong = true;
                    }

                    // Type
                    switch (formatString[i])
                    {
                        case 's':
                        case 'c':
                        case 'd':
                        case 'u':
                        case 'i':
                        case 'X':
                        case 'x':
                            token.type = formatString[i];
                            i++;
                            break;

                        default:
                            token.literal = formatString[i].ToString();
                            break;
                    }

                    // Add it
                    tokens.Add(token);

                    // Next literal starts after this token
                    litPos = i;

                    continue;
                }

                i++;
            }

            // Flush the tail
            Flush();

            // Done
            return tokens;
        }

        public static string Format(string format, params object[] parms)
        {
            return Format(Parse(format), parms);
        }

        public static string Format(List<Token> tokens, params object[] parms)
        {
            var sb = new StringBuilder();
            int paramIndex = 0;
            for (int i=0; i<tokens.Count; i++)
            {
                var t = tokens[i];

                if (t.literal!= null)
                {
                    sb.Append(t.literal);
                    continue;
                }

                // Width specifier
                int width = t.width;

                string value = "";

                // Format specifier
                switch (t.type)
                {
                    case 's':
                        value = (string)parms[paramIndex];
                        break;

                    case 'c':
                        if ((char)parms[paramIndex]!=0)
                        {
                            value = ((char)parms[paramIndex]).ToString();
                        }
                        break;

                    case 'd':
                    case 'i':
                        if (t.isLong)
                            value = ((int)parms[paramIndex]).ToString("D" + t.precision.ToString());
                        else
                            value = ((short)parms[paramIndex]).ToString("D" + t.precision.ToString());
                        break;

                    case 'u':
                        if (t.isLong)
                            value = ((uint)parms[paramIndex]).ToString("D" + t.precision.ToString());
                        else
                            value = ((ushort)parms[paramIndex]).ToString("D" + t.precision.ToString());
                        break;

                    case 'x':
                    case 'X':
                        if (t.isLong)
                            value = ((uint)parms[paramIndex]).ToString(t.type.ToString() + t.precision.ToString());
                        else
                            value = ((ushort)parms[paramIndex]).ToString(t.type.ToString() + t.precision.ToString());

                        if (t.hexPrefix)
                        {
                            value = "0" + t.type.ToString() + value;
                            if (width > 0)
                                width += 2;
                        }
                        break;

                }

                string widthSpec = "";
                if (width > 0)
                {
                    if (t.leadingZeroes)
                    {
                        if (value.Length < t.width)
                        {
                            value = new string('0', t.width - value.Length) + value;
                        }
                    }
                    else
                    {
                        widthSpec = string.Format(",{0}", t.rightAlign ? width : -width);
                    }
                }

                // Format it...
                string formatString = string.Format("{{0{0}}}", widthSpec);
                sb.AppendFormat(formatString, value);

                paramIndex++;
            }

            return sb.ToString();
        }
    }
}
