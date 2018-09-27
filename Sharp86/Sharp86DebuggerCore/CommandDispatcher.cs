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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sharp86
{
    public class CommandDispatcher
    {
        public CommandDispatcher(DebuggerCore debugger)
        {
            _debugger = debugger;
            RegisterCommandHandler(new DebuggerCommands());
        }

        DebuggerCore _debugger;
        internal List<object> _commandHandlers = new List<object>();

        public void RegisterCommandHandler(object handler)
        {
            _commandHandlers.Add(handler);
        }

        bool IsRedirect(Expression.Token token)
        {
            return token == Expression.Token.Redirect ||
                   token == Expression.Token.RedirectAppend;
        }

        public object TryExecuteCommand(object handler, System.Reflection.MethodInfo mi, string cmdline, string args)
        {
            var tokenizer = new Expression.Tokenizer(new StringReader(args));

            // Setup parameters
            var pis = mi.GetParameters();
            object[] paramValues = new object[pis.Length];
            for (int i = 0; i < pis.Length; i++)
            {
                // Get parameter info
                var pi = pis[i];

                // Debugger reference?
                if (pi.ParameterType.IsAssignableFrom(typeof(DebuggerCore)))
                {
                    paramValues[i] = _debugger;
                    continue;
                }

                // Redirect is essential eof

                // Check we have enough parameters
                if (tokenizer.CurrentToken == Expression.Token.EOF || IsRedirect(tokenizer.CurrentToken))
                {
                    if (pi.HasDefaultValue)
                    {
                        paramValues[i] = pi.DefaultValue;
                        continue;
                    }

                    throw new ArgumentException(string.Format("Missing parameter: {0}", pis[i].Name));
                }

                if (tokenizer.CurrentToken == Expression.Token.Comma)
                {
                    if (pi.HasDefaultValue)
                    {
                        paramValues[i] = pi.DefaultValue;
                        tokenizer.NextToken();
                        continue;
                    }

                    throw new ArgumentException(string.Format("Missing parameter: {0}", pis[i].Name));
                }

                if (pi.ParameterType == typeof(string))
                {
                    if (pi.GetCustomAttributes<ArgTailAttribute>().Any())
                    {
                        // Capture rest of the command string
                        paramValues[i] = args.Substring(tokenizer.CurrentTokenPosition);

                        // Skip everything
                        tokenizer.SkipToEnd();
                        continue;
                    }

                    if (tokenizer.CurrentToken == Expression.Token.Identifier)
                    {
                        paramValues[i] = tokenizer.String;
                        tokenizer.NextToken();
                        tokenizer.SkipIf(Expression.Token.Comma);
                        continue;
                    }
                }

                // Parse the expression
                var expr = new Expression(null);
                expr.Parse(tokenizer, args);

                // Does command want unevaluated expression?
                if (pi.ParameterType == typeof(Expression))
                {
                    expr.ResolveImmediateNodes(_debugger.ExpressionContext);

                    paramValues[i] = expr;
                    tokenizer.SkipIf(Expression.Token.Comma);
                    continue;
                }

                // Does it want far pointer?
                if (pi.ParameterType == typeof(FarPointer))
                {
                    var fp = _debugger.ExpressionContext.ResolveFarPointer(expr);
                    paramValues[i] = fp;
                    tokenizer.SkipIf(Expression.Token.Comma);
                    continue;
                }

                // Eval the expression
                var exprValue = expr.Eval(_debugger.ExpressionContext);

                if (pi.ParameterType == typeof(BreakPoint))
                {
                    var bpNumber = (int)Convert.ChangeType(exprValue, typeof(int));
                    var bp = _debugger.BreakPoints.FirstOrDefault(x => x.Number == bpNumber);
                    if (bp == null)
                    {
                        throw new InvalidDataException(string.Format("Breakpoint #{0} doesn't exist", bpNumber));
                    }

                    paramValues[i] = bp;
                }
                else
                {
                    // Store it
                    paramValues[i] = Convert.ChangeType(exprValue, pi.ParameterType);
                }

                // Skip commas
                tokenizer.SkipIf(Expression.Token.Comma);
            }

            // All command line parameters used?
            if (tokenizer.CurrentToken != Expression.Token.EOF && !IsRedirect(tokenizer.CurrentToken))
            {
                throw new ArgumentException("Too many parameters on command line");
            }

            // Redirect?
            Action FinishRedirect = null;
            if (IsRedirect(tokenizer.CurrentToken))
            {
                bool append = tokenizer.CurrentToken == Expression.Token.RedirectAppend;
                tokenizer.NextToken();

                var target = args.Substring(tokenizer.CurrentTokenPosition).Trim();

                if (target == "clipboard")
                {
                    var tw = new StringWriter();
                    _debugger.Redirect(tw);
                    FinishRedirect = () =>
                    {
                        tw.WriteLine();
                        tw.Close();
                        Clipboard.SetText(tw.ToString());
                        _debugger.Redirect(null);
                    };
                }
                else if (target == "editor")
                {
                    var filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "debug.txt");
                    var tw = new StreamWriter(filename, append, Encoding.UTF8);
                    _debugger.Redirect(tw);
                    FinishRedirect = () =>
                    {
                        tw.WriteLine();
                        tw.Close();
                        System.Diagnostics.Process.Start(filename);
                        _debugger.Redirect(null);
                    };
                }
                else
                {
                    var tw = new StreamWriter(target, append, Encoding.UTF8);
                    _debugger.Redirect(tw);
                    FinishRedirect = () =>
                    {
                        tw.WriteLine();
                        tw.Close();
                        _debugger.Redirect(null);
                    };
                }

                _debugger.WriteLine(">" + cmdline);
            }

            try
            {

                // Execute
                var retv = mi.Invoke(handler, paramValues);

                // Write return value
                if (retv is string)
                {
                    _debugger.WriteLine(retv.ToString());
                }

                return retv;
            }
            finally
            {
                if (FinishRedirect != null)
                    FinishRedirect();
            }
        }

        public string FormatCommandHelp(MethodInfo mi)
        {
            var sb = new StringBuilder();

            sb.Append(mi.Name);

            foreach (var pi in mi.GetParameters())
            {
                if (pi.ParameterType == typeof(DebuggerCore))
                    continue;

                sb.Append(" ");
                if (pi.HasDefaultValue)
                {
                    sb.Append("[");
                    sb.Append(pi.Name);
                    sb.Append("]");
                }
                else
                {
                    sb.Append(pi.Name);
                }
            }

            return sb.ToString();
        }

        List<string> _pendingCommands = new List<string>();

        public void EnqueueCommand(string commandLine)
        {
            _pendingCommands.Add(commandLine);
            _debugger.PendingCommands = true;
            _debugger.Continue();
        }

        public void ExecuteQueuedCommands()
        {
            while (_pendingCommands.Count>0 && _debugger.IsStopped && !_debugger.ShouldContinue)
            {
                // Dequeue command
                var cmd = _pendingCommands[0];
                _pendingCommands.RemoveAt(0);

                // Execute one command
                _debugger.WriteLine(">" + cmd);
                ExecuteCommand(cmd);
            }

            _debugger.PendingCommands = _pendingCommands.Count > 0;
        }

        public object ExecuteCommand(string commandLine)
        {
            try
            {
                // If debugger isn't stopped then just queue the command for now
                if (!_debugger.IsStopped || _debugger.ShouldContinue)
                {
                    throw new InvalidOperationException("Can't execute commands unless the debugger is stopped");
                }

                // Create a tokenizer
                var tokenizer = new Expression.Tokenizer(new StringReader(commandLine));

                // Get the command
                if (tokenizer.CurrentToken != Expression.Token.Identifier)
                {
                    _debugger.WriteLine("Syntax error: {0}", commandLine);
                    return null;
                }

                // Capture the command
                var cmd = tokenizer.String;
                tokenizer.NextToken();

                // Look for sub-comamnds
                while (tokenizer.CurrentToken == Expression.Token.Identifier)
                {
                    if (!_commandHandlers.SelectMany(x => x.GetType().GetMethods())
                                .Any(x => x.Name.StartsWith(cmd + "_" + tokenizer.String)))
                        break;

                    cmd = cmd + "_" + tokenizer.String;
                    tokenizer.NextToken();
                }

                // Split off the arguments
                var args = commandLine.Substring(tokenizer.CurrentTokenPosition);

                // Look for a matching command
                List<MethodInfo> attempted = new List<MethodInfo>();
                ArgumentException argException = null;
                foreach (var handler in _commandHandlers)
                {
                    foreach (var mi in handler.GetType().GetMethods().Where(x => x.Name == cmd))
                    {
                        attempted.Add(mi);

                        try
                        {
                            return TryExecuteCommand(handler, mi, commandLine, args);
                        }
                        catch (ArgumentException x)
                        {
                            argException = x;
                            continue;
                        }
                        catch (Exception x)
                        {
                            while (x is System.Reflection.TargetInvocationException)
                                x = x.InnerException;
                            _debugger.WriteLine(x.Message);
                            return null;
                        }
                    }
                }

                if (attempted.Count == 0)
                {
                    _debugger.WriteLine("Unknown command: '{0}'", cmd);
                    return null;
                }

                if (attempted.Count == 1)
                {
                    _debugger.WriteLine(argException.Message);
                    return null;
                }

                _debugger.WriteLine("Parameters not accepted by any of {0} command variants:", attempted.Count);
                foreach (var mi in attempted)
                {
                    _debugger.WriteLine("  {0}", FormatCommandHelp(mi));
                }
                return null;
            }
            catch (Exception x)
            {
                _debugger.WriteLine(x.Message);
                return null;
            }
        }

    }
}
