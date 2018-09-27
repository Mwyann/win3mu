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
    public static class DosPath
    {
        public static string Join(string a, string b)
        {
            while (a.EndsWith("\\"))
            {
                a = a.Substring(0, a.Length - 1);
            }

            while (b.StartsWith("\\"))
            {
                b = b.Substring(1);
            }

            return $"{a}\\{b}";
        }

        // Check if path is valid 8.3 filename
        public static bool IsValid(string path)
        {
            var parts = path.Split('\\');
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];

                // Drive letter?
                if (i == 0 && IsValidDriveLetterSpecification(part))
                    continue;

                // Check for double backslash
                if (i != 0 && i != parts.Length - 1 && part.Length == 0)
                    return false;

                if (!IsValidElement(part))
                    return false;
            }

            return true;
        }

        public static int DriveFromPath(string path)
        {
            if (path.Length < 2 || path[1]!=':')
                throw new InvalidOperationException("Can't extract drive letter from non-qualified path");

            if (path[0] >= 'A' && path[0] <= 'Z')
                return path[0] - 'A';
            if (path[0] >= 'a' && path[0] <= 'z')
                return path[0] - 'a';

            throw new InvalidOperationException("Invalid drive letter");
        }

        public static bool IsValidDriveLetterSpecification(string part)
        {
            if (part.Length == 2)
            {
                if (part[1]==':')
                {
                    var drive = part[0];
                    if ((drive >= 'A' && drive <= 'Z') || (drive >= 'a' && drive <= 'z'))
                    {
                        // Allow it
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsValidElement(string part)
        {
            // Pseudo directory
            if (part == "." || part == "..")
                return true;

            var fparts = part.Split('.');

            // Only allowed one period in file name part
            if (fparts.Length > 2)
                return false;

            // Limit to 8.3
            if (fparts.Length > 0 && fparts[0].Length > 8)
                return false;
            if (fparts.Length > 1 && fparts[1].Length > 3)
                return false;

            // Check they're all valid
            if (fparts.Any(x => !IsValidCharacters(x)))
                return false;

            return true;
        }

        public static bool IsValidCharacters(string filename)
        {
            if (filename == null)
                return true;

            for (int i = 0; i < filename.Length; i++)
            {
                var ch = filename[i];
                if ((ch >= 'A' && ch <= 'Z') ||
                    (ch >= 'a' && ch <= 'z') ||
                    (ch >= '0' && ch <= '9') ||
                    (ch >= 128 && ch <= 255))
                    continue;

                switch (ch)
                {
                    case '!':
                    case '#':
                    case '$':
                    case '%':
                    case '&':
                    case '\'':
                    case '(':
                    case '9':
                    case '-':
                    case '@':
                    case '^':
                    case '_':
                    case '`':
                    case '{':
                    case '}':
                        continue;
                }

                return false;
            }

            return true;
        }

        // Check if a path is fully qualified
        public static bool IsFullyQualified(string path)
        {
            if (path == null)
                return false;
            return path.Length >= 3 && path[2] == '\\' && IsValidDriveLetterSpecification(path.Substring(0, 2));
        }

        public static string CanonicalizePath(string path)
        {
            var parts = path.Split('\\').ToList();
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i] == "..")
                {
                    // In first part is weird?
                    if (i == 0)
                        continue;

                    // Don't remove drive letter
                    if (i==1 && IsValidDriveLetterSpecification(parts[0]))
                    {
                        continue;
                    }

                    // Pop one element
                    parts.RemoveAt(i);
                    parts.RemoveAt(i - 1);
                    i -= 2;
                }
                else if (parts[i] == ".")
                {
                    // Remove redundant
                    parts.RemoveAt(i);
                    i--;
                }
                else
                {
                    parts[i] = string.Join(".", parts[i].Split('.').Select(x => x.Trim()));

                }
            }
            return string.Join("\\", parts);
        }

        // Resolve relative path
        public static string ResolveRelativePath(string basePath, string path)
        {
            System.Diagnostics.Debug.Assert(IsFullyQualified(basePath));
            System.Diagnostics.Debug.Assert(IsValid(basePath));

            // Null?
            if (path == null)
                return null;

            // Empty?
            if (path.Length == 0)
                return basePath;

            // Already fully qualified?
            if (IsFullyQualified(path))
                return path;

            // Root path?
            if (path[0] == '\\')
                return CanonicalizePath(basePath.Substring(0, 2) + path);

            // Join paths
            if (!basePath.EndsWith("\\"))
                basePath += "\\";

            return CanonicalizePath(basePath + path);
        }

        public static bool GlobMatch(string pattern, string filename)
        {
            while (true)
            {
                var newPattern = pattern.Replace("*?", "*").Replace("**", "*");
                if (newPattern == pattern)
                    break;
                pattern = newPattern;
            }

            int j = 0;
            for (int i=0; i<pattern.Length; i++)
            {
                if (pattern[i] == '*')
                {
                    // Star end of pattern - match anything
                    if (i == pattern.Length - 1)
                        return true;

                    // At end of file, finish matching the wilcard
                    if (j == filename.Length)
                        continue;

                    // Does the next character match?
                    if (i+1 < pattern.Length && char.ToLowerInvariant(pattern[i+1]) != char.ToLowerInvariant(filename[j]))
                    {
                        i--;    // Stay on the *
                        j++;    // Skip anything 
                    }
                }
                else if (pattern[i] == '?')
                {
                    if (j<filename.Length)
                        j++;
                }
                else if (pattern[i] == '.')
                {
                    if (j<filename.Length)
                    {
                        if (filename[j] != '.')
                            return false;
                        j++;
                    }
                }
                else
                {
                    if (j == filename.Length)
                        return false;

                    // Same character?
                    if (char.ToLowerInvariant(pattern[i]) != char.ToLowerInvariant(filename[j]))
                        return false;
                    j++;
                }
            }
            return j == filename.Length;
        }

    }
}
