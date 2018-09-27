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
using PetaJson;

namespace Sharp86
{
    public class WatchExpression
    {
        public WatchExpression()
        {
        }

        public WatchExpression(Expression expression)
        {
            _expression = expression;
        }

        [Json("number")]
        public int Number;

        [Json("expression")]
        public string ExpressionText
        {
            get { return _expression == null ? null : _expression.OriginalExpression; }
            set { _expression = value == null ? null : new Expression(value); }
        }

        [Json("name")]
        public string Name
        {
            get;
            set;
        }

        public Expression Expression
        {
            get { return _expression; }
            set { _expression = value; }
        }

        public string EvalAndFormat(DebuggerCore debugger)
        {
            try
            {
                return debugger.ExpressionContext.EvalAndFormat(_expression);
            }
            catch (Exception x)
            {
                return "err:" + x.Message;
            }
        }

        Expression _expression;

        public override string ToString()
        {
            return string.Format("#{0} - {1}", Number, _expression.OriginalExpression);
        }
    }
}
