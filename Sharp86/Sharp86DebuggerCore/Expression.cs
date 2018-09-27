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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using PetaJson;

namespace Sharp86
{
    public class ExpressionContext
    {
        public ExpressionContext(CPU cpu)
        {
            _cpu = cpu;

            PushSymbolScope(new CpuSymbolScope(cpu));
        }

        List<ISymbolScope> _symbolScopes = new List<ISymbolScope>();

        public void PushSymbolScope(ISymbolScope scope)
        {
            _symbolScopes.Add(scope);
        }

        public void PopSymbolScope()
        {
            _symbolScopes.RemoveAt(_symbolScopes.Count - 1);
        }

        public Symbol ResolveSymbol(string name)
        {
            for (int i=_symbolScopes.Count-1; i>=0; i--)
            {
                var sym = _symbolScopes[i].ResolveSymbol(name);
                if (sym != null)
                    return sym;
            }

            return null;
        }

        CPU _cpu;
        public CPU CPU
        {
            get { return _cpu; }
        }

        public IMemoryBus MemoryBus
        {
            get { return _cpu.MemoryBus; }
        }

        public string GenerateDisassemblyAnnotations(string disassembled, string implicitOperands)
        {
            // Strip of prefixes
            if (disassembled.StartsWith("rep "))
                disassembled = disassembled.Substring(4);
            if (disassembled.StartsWith("repne "))
                disassembled = disassembled.Substring(6);

            // Split after op name
            int firstSpace = disassembled.IndexOf(' ');
            string op;
            List<string> operands;
            if (firstSpace < 0)
            {
                op = disassembled;
                operands = new List<string>();
            }
            else
            {
                op = disassembled.Substring(0, firstSpace);
                operands = disassembled.Substring(firstSpace + 1).Split(',').ToList();
            }

            if (implicitOperands!=null)
            {
                operands.AddRange(implicitOperands.Split(','));
            }

            // Evaluate all operands
            List<string> referencedSymbols = new List<string>();
            List<KeyValuePair<string, Expression>> evaluatedExpressions = new List<KeyValuePair<string, Expression>>();
            foreach (var o in operands)
            {
                try
                {
                    var expr = new Expression();
                    expr.ParseSimple(o);
                    if (expr.IsConstant)
                        continue;

                    // Add all reference symbols too (unless the expression itself is a plain symbol)
                    referencedSymbols.AddRange(expr.AllReferencedSymbols);

                    evaluatedExpressions.Add(new KeyValuePair<string, Expression>(o, expr));
                }
                catch
                {
                    // Don't care
                }
            }


            var displayed = new HashSet<string>();
            var sb = new StringBuilder();
            var first = true;
            
            foreach (var val in evaluatedExpressions)
            {
                if (displayed.Contains(val.Key))
                    continue;
                displayed.Add(val.Key);

                object exprValue;
                try
                {
                    exprValue = FormatWithValueType(val.Value.Eval(this));
                }
                catch
                {
                    continue;
                }

                // Append separator
                if (!first)
                    sb.Append(", ");
                first = false;

                var exprText = val.Key;
                if (exprText.StartsWith("word ptr ") || exprText.StartsWith("byte ptr"))
                {
                    exprText = exprText.Substring(9);
                }
                else if (exprText.StartsWith("dword ptr "))
                {
                    exprText = exprText.Substring(10);
                }

                sb.AppendFormat("{0}={1}", exprText, exprValue);
            }

            foreach (var s in referencedSymbols)
            {
                // Already displayed?
                if (displayed.Contains(s))
                    continue;
                displayed.Add(s);

                // Resolve the symbol
                var symbol = ResolveSymbol(s);
                if (symbol == null)
                    continue;

                // Append separator
                if (!first)
                    sb.Append(", ");
                first = false;

                // Format it
                sb.AppendFormat("{0}={1}", s, FormatWithValueType(symbol.GetValue()));
            }

            return sb.ToString();
        }

        public string EvalAndFormat(Expression expr)
        {
            return FormatWithValueType(expr.Eval(this));
        }

        public string EvalAndFormat(string expr)
        {
            var exprObj = new Expression(expr);
            return EvalAndFormat(exprObj);
        }

        public FarPointer ResolveFarPointer(Expression expr)
        {
            return Expression.ResolveFarPointer(this, expr);
        }


        public static string FormatWithValueType(object value)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte:
                    return string.Format("0x{0:X2}", value);

                case TypeCode.UInt16:
                    return string.Format("0x{0:X4}", value);

                case TypeCode.UInt32:
                    return string.Format("0x{0:X8}", value);

                case TypeCode.UInt64:
                    return string.Format("0x{0:X16}", value);
            }

            return value.ToString();
        }

    }

    public class Expression
    {
        public Expression(string expr = null)
        {
            if (!string.IsNullOrWhiteSpace(expr))
            {
                Parse(expr);
            }
        }

        public string OriginalExpression
        {
            get { return _expression; }
        }

        public object Eval(ExpressionContext ctx, ISymbolScope scope)
        {
            ctx.PushSymbolScope(scope);
            try
            {
                return Eval(ctx);
            }
            finally
            {
                ctx.PopSymbolScope();
            }
        }

        public object Eval(ExpressionContext ctx)
        {
            // Prevent recursive
            if (_evaluating)
                throw new InvalidOperationException("Recursive expression evaluation");

            // Evaluate
            try
            {
                _evaluating = true;
                return _rootNode.Eval(ctx);
            }
            finally
            {
                _evaluating = false;
            }
        }

        public void ResolveImmediateNodes(ExpressionContext ctx)
        {
            _rootNode = _rootNode.ResolveImmediateNodes(ctx);
        }

        List<Node> _allNodes;
        IEnumerable<Node> AllNodes
        {
            get
            {
                if (_allNodes == null)
                {
                    _allNodes  = new List<Node>();
                    _rootNode.GetAllNodes(_allNodes);
                }
                return _allNodes;
            }
        }

        public IEnumerable<string> AllReferencedSymbols
        {
            get
            {
                return AllNodes.OfType<SymbolNode>().Select(x => x.GetSymbolName());
            }
        }

        public bool IsPlainSymbol
        {
            get
            {
                return _rootNode is SymbolNode;
            }
        }

        public bool IsConstant
        {
            get
            {
                return _rootNode.IsConstant();
            }
        }

        public void Parse(string expression)
        {
            _expression = expression;
            using (var r = new StringReader(expression))
            {
                var tokenizer = new Tokenizer(r);
                _rootNode = ParseTop(tokenizer);
                tokenizer.Check(Token.EOF);
            }
        }

        public void ParseSimple(string expression)
        {
            _expression = expression;
            using (var r = new StringReader(expression))
            {
                var tokenizer = new Tokenizer(r);
                _rootNode = ParseCast(tokenizer);
                tokenizer.Check(Token.EOF);
            }
        }

        public void Parse(Tokenizer tokenizer, string underlying = null)
        {
            // Capture start position
            int posStart = tokenizer.CurrentTokenPosition;

            // Parse expression
            _rootNode = ParseTop(tokenizer);

            // Capture the expression text from underlying string (if supplied)
            if (underlying!=null)
            {
                _expression = underlying.Substring(posStart, tokenizer.CurrentTokenPosition - posStart);
            }
        }


        Type ParseTypeCode(string typeName)
        {
            switch (typeName.ToLowerInvariant())
            {
                case "dword":
                case "uint":
                    return typeof(uint);

                case "long":
                    return typeof(long);

                case "qword":
                case "ulong":
                    return typeof(ulong);

                case "word":
                case "ushort":
                    return typeof(ushort);

                case "byte":
                    return typeof(byte);

                case "int":
                    return typeof(short);

                case "short":
                    return typeof(short);

                case "char":
                    return typeof(char);

                case "sbyte":
                    return typeof(sbyte);

                case "ptr":
                    return typeof(FarPointer);
            }

            return null;
        }

 
        Node ParseLeaf(Tokenizer tokenizer)
        {
            Node node = null;
            switch (tokenizer.CurrentToken)
            {
                case Token.Literal:
                    node = new LiteralNode(tokenizer.LiteralValue);
                    tokenizer.NextToken();
                    return node;

                case Token.OpenSquare:
                    tokenizer.NextToken();
                    node = ParseTop(tokenizer);
                    tokenizer.Skip(Token.CloseSquare);
                    return new DereferenceNode(node);

                case Token.OpenRound:
                    tokenizer.NextToken();
                    node = ParseTop(tokenizer);
                    tokenizer.Skip(Token.CloseRound);
                    return node;

                case Token.OpenBrace:
                    tokenizer.NextToken();
                    node = new ImmediateResolveNode(ParseTop(tokenizer));
                    tokenizer.Skip(Token.CloseBrace);
                    return node;

                case Token.Identifier:
                    {
                        var name = tokenizer.String;
                        tokenizer.NextToken();

                        if (name == "ptr")
                        {
                            var deref = ParseSegmentOffset(tokenizer) as DereferenceNode;
                            if (deref == null)
                                throw new InvalidDataException("Syntax error, expected dereference operator to right of 'ptr'");
                            return deref;
                        }

                        return new SymbolNode(name);
                    }
            }

            throw new InvalidDataException(string.Format("Syntax error, unexpected token: {0}", tokenizer.CurrentToken.ToString()));
        }

        Node ParseSegmentOffset(Tokenizer tokenizer)
        {
            if (tokenizer.SkipIf(Token.Colon))
            {
                var rhs = ParseLeaf(tokenizer);
                return new SegmentOffsetNode(null, rhs);
            }
            else
            {
                var lhs = ParseLeaf(tokenizer);
                if (!tokenizer.SkipIf(Token.Colon))
                    return lhs;

                var rhs = ParseLeaf(tokenizer);
                var deref = rhs as DereferenceNode;
                if (rhs is DereferenceNode)
                {
                    deref.Expression = new SegmentOffsetNode(lhs, deref.Expression);
                    return rhs;
                }

                return new SegmentOffsetNode(lhs, rhs);
            }
        }

        Node ParseCast(Tokenizer tokenizer)
        {
            if (tokenizer.CurrentToken == Token.Identifier)
            {
                // Is it a type cast?
                var CastType = ParseTypeCode(tokenizer.String);
                if (CastType != null)
                {
                    tokenizer.NextToken();

                    var rhs = ParseCast(tokenizer);
                    var deref = rhs as DereferenceNode;
                    if (deref != null)
                    {
                        deref.CastType = CastType;
                        return deref;
                    }
                    else
                    {
                        return new TypeCastNode(rhs, CastType);
                    }
                }
            }

            return ParseSegmentOffset(tokenizer);
        }

        Node ParseUnary(Tokenizer tokenizer)
        {
            while (true)
            {
                switch (tokenizer.CurrentToken)
                {
                    case Token.Subtract:
                        tokenizer.NextToken();
                        var operand = ParseUnary(tokenizer);
                        if (operand is LiteralNode)
                        {
                            ((LiteralNode)operand).Negate();
                            return operand;
                        }
                        else
                        {
                            return new UnaryNode(operand, Operators.negate);
                        }

                    case Token.LogicalNot:
                        tokenizer.NextToken();
                        return new UnaryNode(ParseUnary(tokenizer), Operators.logical_not);

                    case Token.BitwiseNot:
                        tokenizer.NextToken();
                        return new UnaryNode(ParseUnary(tokenizer), Operators.bitwise_not);

                    default:
                        return ParseCast(tokenizer);
                }
            }
        }

        Node ParseMulDiv(Tokenizer tokenizer)
        {
            var lhs = ParseUnary(tokenizer);

            UnorderedNode unordered = null;

            while (true)
            {
                // Work out operator
                Func<object, object, object> op = null;
                switch (tokenizer.CurrentToken)
                {
                    case Token.Multiply:
                        op = Operators.multiply;
                        break;
                    case Token.Divide:
                        op = Operators.divide;
                        break;
                }

                // Finished?
                if (op == null)
                    return lhs;

                // First, create the unordered node
                if (unordered == null)
                {
                    unordered = new UnorderedNode(1);
                    unordered.AddOperand(lhs, Operators.multiply);
                    lhs = unordered;
                }

                // Parse the operand
                tokenizer.NextToken();
                unordered.AddOperand(ParseUnary(tokenizer), op);
            }
        }

        Node ParseAddSub(Tokenizer tokenizer)
        {
            var lhs = ParseMulDiv(tokenizer);

            UnorderedNode unordered = null;

            while (true)
            {
                // Work out operator
                Func<object, object, object> op = null;
                switch (tokenizer.CurrentToken)
                {
                    case Token.Add:
                        op = Operators.add;
                        break;
                    case Token.Subtract:
                        op = Operators.subtract;
                        break;
                }

                // Finished?
                if (op == null)
                    return lhs;

                // First, create the unordered node
                if (unordered == null)
                {
                    unordered = new UnorderedNode(0);
                    unordered.AddOperand(lhs, Operators.add);
                    lhs = unordered;
                }

                // Parse the operand
                tokenizer.NextToken();
                unordered.AddOperand(ParseMulDiv(tokenizer), op);
            }
        }

        Node ParseShift(Tokenizer tokenizer)
        {
            var lhs = ParseAddSub(tokenizer);

            while (true)
            {
                switch (tokenizer.CurrentToken)
                {
                    case Token.ShiftLeft:
                        tokenizer.NextToken();
                        lhs = new BinaryNode(lhs, ParseAddSub(tokenizer), Operators.shl);
                        break;

                    case Token.ShiftRight:
                        tokenizer.NextToken();
                        lhs = new BinaryNode(lhs, ParseAddSub(tokenizer), Operators.shr);
                        break;

                    default:
                        return lhs;
                }
            }
        }

        Node ParseComparison(Tokenizer tokenizer)
        {
            var lhs = ParseShift(tokenizer);

            switch (tokenizer.CurrentToken)
            {
                case Token.CompareGT: tokenizer.NextToken(); return new BinaryNode(lhs, ParseShift(tokenizer), Operators.compare_gt);
                case Token.CompareGE: tokenizer.NextToken(); return new BinaryNode(lhs, ParseShift(tokenizer), Operators.compare_ge);
                case Token.CompareLT: tokenizer.NextToken(); return new BinaryNode(lhs, ParseShift(tokenizer), Operators.compare_lt);
                case Token.CompareLE: tokenizer.NextToken(); return new BinaryNode(lhs, ParseShift(tokenizer), Operators.compare_le);
            }

            return lhs;
        }

        Node ParseEquality(Tokenizer tokenizer)
        {
            var lhs = ParseComparison(tokenizer);

            switch (tokenizer.CurrentToken)
            {
                case Token.CompareEQ: tokenizer.NextToken(); return new BinaryNode(lhs, ParseComparison(tokenizer), Operators.compare_eq);
                case Token.CompareNE: tokenizer.NextToken(); return new BinaryNode(lhs, ParseComparison(tokenizer), Operators.compare_ne);
            }

            return lhs;
        }


        Node ParseBitwiseAnd(Tokenizer tokenizer)
        {
            var lhs = ParseEquality(tokenizer);

            while (true)
            {
                switch (tokenizer.CurrentToken)
                {
                    case Token.BitwiseOr:
                        tokenizer.NextToken();
                        lhs = new BinaryNode(lhs, ParseEquality(tokenizer), Operators.bitwise_or);
                        break;

                    default:
                        return lhs;
                }
            }
        }

        Node ParseBitwiseXor(Tokenizer tokenizer)
        {
            var lhs = ParseBitwiseAnd(tokenizer);

            while (true)
            {
                switch (tokenizer.CurrentToken)
                {
                    case Token.BitwiseOr:
                        tokenizer.NextToken();
                        lhs = new BinaryNode(lhs, ParseBitwiseAnd(tokenizer), Operators.bitwise_or);
                        break;

                    default:
                        return lhs;
                }
            }
        }

        Node ParseBitwiseOr(Tokenizer tokenizer)
        {
            var lhs = ParseBitwiseXor(tokenizer);

            while (true)
            {
                switch (tokenizer.CurrentToken)
                {
                    case Token.BitwiseOr:
                        tokenizer.NextToken();
                        lhs = new BinaryNode(lhs, ParseBitwiseXor(tokenizer), Operators.bitwise_or);
                        break;

                    default:
                        return lhs;
                }
            }
        }


        Node ParseLogicalAnd(Tokenizer tokenizer)
        {
            var lhs = ParseBitwiseOr(tokenizer);

            while (true)
            {
                switch (tokenizer.CurrentToken)
                {
                    case Token.LogicalAnd:
                        tokenizer.NextToken();
                        lhs = new LogicalAndNode(lhs, ParseBitwiseOr(tokenizer));
                        break;

                    default:
                        return lhs;
                }
            }
        }

        Node ParseLogicalOr(Tokenizer tokenizer)
        {
            var lhs = ParseLogicalAnd(tokenizer);

            while (true)
            {
                switch (tokenizer.CurrentToken)
                {
                    case Token.LogicalOr:
                        tokenizer.NextToken();
                        lhs = new LogicalOrNode(lhs, ParseLogicalAnd(tokenizer));
                        break;

                    default:
                        return lhs;
                }
            }
        }

        Node ParseTop(Tokenizer tokenizer)
        {
            return ParseLogicalOr(tokenizer);
        }

        string _expression;
        bool _evaluating;
        Node _rootNode;

        static string ResolveSelector(ExpressionContext ctx, Node node)
        {
            // Assume expression isn't qualified with a segment so deduce from operands
            var indexBase = node.GetIndexBase() as SymbolNode;
            if (indexBase != null)
            {
                var baseRegister = indexBase.GetSymbolName();
                if (baseRegister != null)
                {
                    switch (baseRegister.ToLowerInvariant())
                    {
                        case "sp":
                        case "bp":
                            return "ss";

                        case "ip":
                            return "cs";
                    }
                }
            }

            return "ds";
        }

        internal static FarPointer ResolveFarPointer(ExpressionContext ctx, Expression expression)
        {
            return ResolveFarPointer(ctx, expression._rootNode);
        }

        static FarPointer ResolveFarPointer(ExpressionContext ctx, Node node)
        {
            // Evaluate the pointer
            var ptr = node.Eval(ctx);

            if (ptr is FarPointer)
            {
                return (FarPointer)ptr;
            }
            else
            {
                return new FarPointer((ushort)ctx.ResolveSymbol(ResolveSelector(ctx, node)).GetValue(), (ushort)Convert.ChangeType(ptr, typeof(ushort)));
            }
        }


abstract class Node
        {
            public Node()
            {
            }

            public abstract object Eval(ExpressionContext ctx);
            public abstract Node GetIndexBase();
            public abstract Node ResolveImmediateNodes(ExpressionContext ctx);
            public abstract bool IsConstant();
            public virtual object ResolveConstant()
            {
                return null;
            }
            public virtual void GetAllNodes(List<Node> nodes)
            {
                nodes.Add(this);
            }
        }

        class LiteralNode : Node
        {
            public LiteralNode(object val)
            {
                _val = val;
            }

            public override object Eval(ExpressionContext ctx)
            {
                return _val;
            }

            public override Node GetIndexBase()
            {
                return this;
            }

            public override Node ResolveImmediateNodes(ExpressionContext ctx)
            {
                return this;
            }

            public override bool IsConstant()
            {
                return true;
            }

            public override object ResolveConstant()
            {
                return _val;
            }

            public void Negate()
            {
                _val = Operators.negate(_val);
            }

            object _val;
        }

        class UnaryNode : Node
        {
            public UnaryNode(Node operand, Func<object, object> op)
            {
                _operand = operand;
                _op = op;
            }

            Node _operand;
            Func<object, object> _op;

            public override object Eval(ExpressionContext ctx)
            {
                return _op(_operand.Eval(ctx));
            }

            public override Node GetIndexBase()
            {
                return _operand.GetIndexBase();
            }


            public override Node ResolveImmediateNodes(ExpressionContext ctx)
            {
                _operand = _operand.ResolveImmediateNodes(ctx);
                return this;
            }

            public override bool IsConstant()
            {
                return _operand.IsConstant();
            }

            /*
            public static object Negate(object a)
            {
                return Operators.negate(a);
            }

            public static object LogicalNot(object a)
            {
                return IsTrue(a) ? 0 : 1;
            }
            */
            public override void GetAllNodes(List<Node> nodes)
            {
                base.GetAllNodes(nodes);
                _operand.GetAllNodes(nodes);
            }
        }

        class UnorderedNode : Node
        {
            public UnorderedNode(object baseValue)
            {
                _baseValue = baseValue;
            }

            public void AddOperand(Node rhs, Func<object, object, object> op)
            {
                _operands.Add(new Operand()
                {
                    _operand = rhs,
                    _op = op,
                });
            }

            public override object Eval(ExpressionContext ctx)
            {
                var val = _operands[0]._operand.Eval(ctx);

                for (int i = 1; i < _operands.Count; i++)
                {
                    val = _operands[i]._op(val, _operands[i]._operand.Eval(ctx));
                }

                return val;
            }

            public override Node GetIndexBase()
            {
                return _operands[0]._operand.GetIndexBase();
            }

            public override Node ResolveImmediateNodes(ExpressionContext ctx)
            {
                for (int i=0; i<_operands.Count; i++)
                {
                    _operands[i]._operand = _operands[i]._operand.ResolveImmediateNodes(ctx);
                }
                return this;
            }

            public override bool IsConstant()
            {
                return _operands.All(x => x._operand.IsConstant());
            }


            object _baseValue;
            List<Operand> _operands = new List<Operand>();

            class Operand
            {
                public Node _operand;
                public Func<object, object, object> _op;
            }

            public override void GetAllNodes(List<Node> nodes)
            {
                base.GetAllNodes(nodes);
                foreach (var o in _operands.Select(x => x._operand))
                {
                    o.GetAllNodes(nodes);
                }
            }
        }

        class BinaryNode : Node
        {
            public BinaryNode(Node lhs, Node rhs, Func<object, object, object> op)
            {
                _lhs = lhs;
                _rhs = rhs;
                _op = op;
            }

            Node _lhs;
            Node _rhs;
            Func<object, object, object> _op;

            public override object Eval(ExpressionContext ctx)
            {
                return _op(_lhs.Eval(ctx), _rhs.Eval(ctx));
            }

            public override Node GetIndexBase()
            {
                return _lhs.GetIndexBase();
            }


            public override Node ResolveImmediateNodes(ExpressionContext ctx)
            {
                _lhs = _lhs.ResolveImmediateNodes(ctx);
                _rhs = _rhs.ResolveImmediateNodes(ctx);
                return this;
            }

            public override bool IsConstant()
            {
                return _lhs.IsConstant() && _rhs.IsConstant();
            }

            public override void GetAllNodes(List<Node> nodes)
            {
                base.GetAllNodes(nodes);
                _lhs.GetAllNodes(nodes);
                _rhs.GetAllNodes(nodes);
            }
        }

        // Short Circuit Logical AND
        class LogicalAndNode : Node
        {
            public LogicalAndNode(Node lhs, Node rhs)
            {
                _lhs = lhs;
                _rhs = rhs;
            }

            Node _lhs;
            Node _rhs;

            public override object Eval(ExpressionContext ctx)
            {
                return (IsTrue(_lhs.Eval(ctx)) && IsTrue(_rhs.Eval(ctx))) ? 1 : 0;
            }

            public override Node GetIndexBase()
            {
                return _lhs.GetIndexBase();
            }

            public override Node ResolveImmediateNodes(ExpressionContext ctx)
            {
                _lhs = _lhs.ResolveImmediateNodes(ctx);
                _rhs = _rhs.ResolveImmediateNodes(ctx);
                return this;
            }

            public override bool IsConstant()
            {
                return _lhs.IsConstant() && _rhs.IsConstant();
            }

            public override void GetAllNodes(List<Node> nodes)
            {
                base.GetAllNodes(nodes);
                _lhs.GetAllNodes(nodes);
                _rhs.GetAllNodes(nodes);
            }

        }

        // Short Circuit Logical OR
        class LogicalOrNode : Node
        {
            public LogicalOrNode(Node lhs, Node rhs)
            {
                _lhs = lhs;
                _rhs = rhs;
            }

            Node _lhs;
            Node _rhs;

            public override object Eval(ExpressionContext ctx)
            {
                return (IsTrue(_lhs.Eval(ctx)) || IsTrue(_rhs.Eval(ctx))) ? 1 : 0;
            }

            public override Node GetIndexBase()
            {
                return _lhs.GetIndexBase();
            }


            public override Node ResolveImmediateNodes(ExpressionContext ctx)
            {
                _lhs = _lhs.ResolveImmediateNodes(ctx);
                _rhs = _rhs.ResolveImmediateNodes(ctx);
                return this;
            }

            public override bool IsConstant()
            {
                return _lhs.IsConstant() && _rhs.IsConstant();
            }

            public override void GetAllNodes(List<Node> nodes)
            {
                base.GetAllNodes(nodes);
                _lhs.GetAllNodes(nodes);
                _rhs.GetAllNodes(nodes);
            }
        }

        class ImmediateResolveNode : Node
        {
            public ImmediateResolveNode(Node expression)
            {
                _expression = expression;
            }

            Node _expression;

            public override object Eval(ExpressionContext ctx)
            {
                return _expression.Eval(ctx);
            }

            public override Node GetIndexBase()
            {
                return _expression.GetIndexBase();
            }

            public override Node ResolveImmediateNodes(ExpressionContext ctx)
            {
                var val = _expression.Eval(ctx);
                var node = new LiteralNode(val);
                return node;
            }

            public override bool IsConstant()
            {
                return _expression.IsConstant();
            }
        }

        class DereferenceNode : Node
        {
            public DereferenceNode(Node expression)
            {
                Expression = expression;
            }


            public override object Eval(ExpressionContext ctx)
            {
                var fp = ResolveFarPointer(ctx, Expression);


                if (CastType == typeof(FarPointer))
                {
                    return new FarPointer((uint)ctx.MemoryBus.ReadDWord(fp.Segment, fp.Offset));
                }

                switch (Type.GetTypeCode(CastType))
                {
                    case TypeCode.Byte:
                        return ctx.MemoryBus.ReadByte(fp.Segment, fp.Offset);

                    case TypeCode.SByte:
                        return (sbyte)ctx.MemoryBus.ReadByte(fp.Segment, fp.Offset);

                    case TypeCode.Int16:
                        return (short)ctx.MemoryBus.ReadWord(fp.Segment, fp.Offset);

                    case TypeCode.UInt32:
                        return (uint)ctx.MemoryBus.ReadDWord(fp.Segment, fp.Offset);

                    case TypeCode.Int32:
                        return (int)ctx.MemoryBus.ReadDWord(fp.Segment, fp.Offset);

                    case TypeCode.UInt16:
                    case TypeCode.Empty:
                        return (ushort)ctx.MemoryBus.ReadWord(fp.Segment, fp.Offset);

                    default:
                        throw new InvalidOperationException(string.Format("Don't know how to dereference ptr of type: {0}", CastType));
                }
            }

            public override Node GetIndexBase()
            {
                return this;
            }

            public override Node ResolveImmediateNodes(ExpressionContext ctx)
            {
                Expression = Expression.ResolveImmediateNodes(ctx);
                return this;
            }

            public override bool IsConstant()
            {
                return false;
            }

            public override void GetAllNodes(List<Node> nodes)
            {
                base.GetAllNodes(nodes);
                Expression.GetAllNodes(nodes);
            }

            public Node Expression;
            public Type CastType;
        }

        class SegmentOffsetNode : Node
        {
            public SegmentOffsetNode(Node segment, Node offset)
            {
                SegmentExpression = segment;
                OffsetExpression = offset;
            }

            public override object Eval(ExpressionContext ctx)
            {
                ushort seg;
                if (SegmentExpression != null)
                {
                    seg = (ushort)Convert.ChangeType(SegmentExpression.Eval(ctx), typeof(ushort));
                }
                else
                {
                    seg = (ushort)ctx.ResolveSymbol(ResolveSelector(ctx, OffsetExpression)).GetValue();
                }


                var ofs = OffsetExpression.Eval(ctx);
                if (ofs is FarPointer)
                {
                    ofs = ((FarPointer)ofs).Offset;
                }
                else
                {
                    ofs = (ushort)Convert.ChangeType(ofs, typeof(ushort));
                }

                return new FarPointer(seg, (ushort)ofs);
            }


            public override Node GetIndexBase()
            {
                return OffsetExpression.GetIndexBase();
            }

            public override Node ResolveImmediateNodes(ExpressionContext ctx)
            {
                if (SegmentExpression!=null)
                    SegmentExpression = SegmentExpression.ResolveImmediateNodes(ctx);
                OffsetExpression = OffsetExpression.ResolveImmediateNodes(ctx);
                return this;
            }

            public override bool IsConstant()
            {
                return (SegmentExpression == null || SegmentExpression.IsConstant()) && OffsetExpression.IsConstant();
            }

            public override void GetAllNodes(List<Node> nodes)
            {
                base.GetAllNodes(nodes);
                if (SegmentExpression!=null)
                    SegmentExpression.GetAllNodes(nodes);
                OffsetExpression.GetAllNodes(nodes);
            }

            public Node SegmentExpression;
            public Node OffsetExpression;
        }

        class TypeCastNode : Node
        {
            public TypeCastNode(Node rhs, Type castType)
            {
                _rhs = rhs;
                _castType = castType;
            }

            public override object Eval(ExpressionContext ctx)
            {
                var rhsVal = _rhs.Eval(ctx);

                if (_castType == typeof(FarPointer))
                {
                    if (rhsVal is FarPointer)
                        return rhsVal;

                    switch (Type.GetTypeCode(rhsVal.GetType()))
                    {
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                            return new FarPointer((uint)(Convert.ChangeType(rhsVal, typeof(uint))));
                    }
                }

                TypeCode typeCode = Type.GetTypeCode(_castType);
                return Convert.ChangeType(rhsVal, typeCode);
            }

            public override Node GetIndexBase()
            {
                return _rhs.GetIndexBase();
            }

            public override Node ResolveImmediateNodes(ExpressionContext ctx)
            {
                _rhs = _rhs.ResolveImmediateNodes(ctx);
                return this;
            }

            public override bool IsConstant()
            {
                return _rhs.IsConstant();
            }

            public override void GetAllNodes(List<Node> nodes)
            {
                base.GetAllNodes(nodes);
                _rhs.GetAllNodes(nodes);
            }

            Node _rhs;
            Type _castType;
        }

        static bool IsTrue(object val)
        {
            return (bool)Convert.ChangeType(val, typeof(bool));
        }

        class SymbolNode : Node
        {
            public SymbolNode(string name)
            {
                _name = name;
            }

            string _name;

            public string GetSymbolName()
            {
                return _name;
            }

            public override object Eval(ExpressionContext ctx)
            {
                var sym = ctx.ResolveSymbol(_name);
                if (sym != null)
                    return sym.GetValue();

                throw new InvalidOperationException(string.Format("Unknown symbol '{0}'", _name));
            }

            public override Node GetIndexBase()
            {
                return this;
            }

            public override Node ResolveImmediateNodes(ExpressionContext ctx)
            {
                return this;
            }

            public override bool IsConstant()
            {
                return false;
            }
        }


        [Obfuscation(Exclude = true)]
        public enum Token
        {
            EOF,
            Identifier,
            Literal,
            OpenRound,
            CloseRound,
            OpenSquare,
            CloseSquare,
            OpenBrace,
            CloseBrace,
            Add,
            Subtract,
            Multiply,
            Divide,
            Colon,
            Comma,
            LogicalNot,
            LogicalAnd,
            LogicalOr,
            BitwiseNot,
            BitwiseAnd,
            BitwiseOr,
            BitwiseXor,
            CompareEQ,
            CompareNE,
            CompareLT,
            CompareLE,
            CompareGT,
            CompareGE,
            ShiftLeft,
            ShiftRight,
            Redirect,
            RedirectAppend,
        }

        public class Tokenizer
        {
            public Tokenizer(TextReader r)
            {
                _underlying = r;
                FillBuffer();
                NextChar();
                NextToken();
            }

            private StringBuilder _sb = new StringBuilder();
            private TextReader _underlying;
            private char[] _buf = new char[4096];
            private int _pos;
            private int _bufUsed;
            private int _currentCharPos;
            private char _currentChar;
            private object _literal;

            public int CurrentTokenPosition;
            public Token CurrentToken;
            public string String;

            public object LiteralValue
            {
                get
                {
                    if (CurrentToken != Token.Literal)
                        throw new InvalidOperationException("token is not a literal");
                    return _literal;
                }
            }

            // Fill buffer by reading from underlying TextReader
            void FillBuffer()
            {
                _bufUsed = _underlying.Read(_buf, 0, _buf.Length);
                _pos = 0;
            }

            // Get the next character from the input stream
            // (this function could be extracted into a few different methods, but is mostly inlined
            //  for performance - yes it makes a difference)
            public char NextChar()
            {
                if (_pos >= _bufUsed)
                {
                    if (_bufUsed > 0)
                    {
                        FillBuffer();
                    }
                    if (_bufUsed == 0)
                    {
                        return _currentChar = '\0';
                    }
                }

                // Next
                _currentCharPos++;
                return _currentChar = _buf[_pos++];
            }

            // Read the next token from the input stream
            // (Mostly inline for performance)
            public void NextToken()
            {
                while (true)
                {
                    // Skip whitespace and handle line numbers
                    while (char.IsWhiteSpace(_currentChar))
                    {
                        NextChar();
                    }

                    // Remember position of token
                    CurrentTokenPosition = _currentCharPos-1;

                    // Handle common characters first
                    switch (_currentChar)
                    {
                        case '/':
                            // Process comment
                            NextChar();
                            switch (_currentChar)
                            {
                                case '/':
                                    NextChar();
                                    while (_currentChar != '\0' && _currentChar != '\r' && _currentChar != '\n')
                                    {
                                        NextChar();
                                    }
                                    break;

                                case '*':
                                    bool endFound = false;
                                    while (!endFound && _currentChar != '\0')
                                    {
                                        if (_currentChar == '*')
                                        {
                                            NextChar();
                                            if (_currentChar == '/')
                                            {
                                                endFound = true;
                                            }
                                        }
                                        NextChar();
                                    }
                                    break;

                                default:
                                    CurrentToken = Token.Divide;
                                    return;
                            }
                            continue;

                        case '&':
                            NextChar();
                            if (_currentChar == '&')
                            {
                                NextChar();
                                CurrentToken = Token.LogicalAnd;
                                return;
                            }
                            else
                            {
                                NextChar();
                                CurrentToken = Token.BitwiseAnd;
                                return;
                            }

                        case '|':
                            NextChar();
                            if (_currentChar == '|')
                            {
                                NextChar();
                                CurrentToken = Token.LogicalAnd;
                                return;
                            }
                            else
                            {
                                NextChar();
                                CurrentToken = Token.BitwiseOr;
                                return;
                            }

                        case '^':
                            NextChar();
                            CurrentToken = Token.BitwiseXor;
                            return;

                        case '=':
                            NextChar();
                            if (_currentChar == '=')
                            {
                                NextChar();
                                CurrentToken = Token.CompareEQ;
                                return;
                            }
                            else
                                throw new InvalidDataException("syntax error, expected second '='");

                        case '<':
                            NextChar();
                            if (_currentChar == '=')
                            {
                                NextChar();
                                CurrentToken = Token.CompareLE;
                            }
                            else
                            {
                                CurrentToken = Token.CompareLT;
                            }
                            return;


                        case '>':
                            NextChar();
                            if (_currentChar == '>')
                            {
                                NextChar();
                                if (_currentChar == '>')
                                {
                                    NextChar();
                                    CurrentToken =Token.RedirectAppend;
                                }
                                else
                                {
                                    CurrentToken = Token.Redirect;
                                }
                            }
                            else if (_currentChar == '=')
                            {
                                NextChar();
                                CurrentToken = Token.CompareGE;
                            }
                            else
                            {
                                CurrentToken = Token.CompareGT;
                            }
                            return;

                        case '!':
                            NextChar();
                            if (_currentChar == '=')
                            {
                                NextChar();
                                CurrentToken = Token.CompareNE;
                            }
                            else
                            {
                                CurrentToken = Token.LogicalNot;
                            }
                            return;

                        case '~':
                            NextChar();
                            CurrentToken = Token.BitwiseNot;
                            return;

                        case '(': CurrentToken = Token.OpenRound; NextChar(); return;
                        case ')': CurrentToken = Token.CloseRound; NextChar(); return;
                        case '[': CurrentToken = Token.OpenSquare; NextChar(); return;
                        case ']': CurrentToken = Token.CloseSquare; NextChar(); return;
                        case '{': CurrentToken = Token.OpenBrace; NextChar(); return;
                        case '}': CurrentToken = Token.CloseBrace; NextChar(); return;
                        case '+': CurrentToken = Token.Add; NextChar(); return;
                        case '-': CurrentToken = Token.Subtract; NextChar(); return;
                        case '*': CurrentToken = Token.Multiply; NextChar(); return;
                        case ':': CurrentToken = Token.Colon; NextChar(); return;
                        case ',': CurrentToken = Token.Comma; NextChar(); return;
                        case '\0': CurrentToken = Token.EOF; CurrentTokenPosition = _currentCharPos;  return;

                        case '\"':
                        case '\'':
                            {
                                _sb.Length = 0;
                                var quoteKind = _currentChar;
                                NextChar();
                                while (_currentChar != '\0')
                                {
                                    if (_currentChar == '\\')
                                    {
                                        NextChar();
                                        var escape = _currentChar;
                                        switch (escape)
                                        {
                                            case '\"': _sb.Append('\"'); break;
                                            case '\\': _sb.Append('\\'); break;
                                            case '/': _sb.Append('/'); break;
                                            case 'b': _sb.Append('\b'); break;
                                            case 'f': _sb.Append('\f'); break;
                                            case 'n': _sb.Append('\n'); break;
                                            case 'r': _sb.Append('\r'); break;
                                            case 't': _sb.Append('\t'); break;
                                            case 'u':
                                                var sbHex = new StringBuilder();
                                                for (int i = 0; i < 4; i++)
                                                {
                                                    NextChar();
                                                    sbHex.Append(_currentChar);
                                                }
                                                _sb.Append((char)Convert.ToUInt16(sbHex.ToString(), 16));
                                                break;

                                            default:
                                                throw new InvalidDataException(string.Format("Invalid escape sequence in string literal: '\\{0}'", _currentChar));
                                        }
                                    }
                                    else if (_currentChar == quoteKind)
                                    {
                                        String = _sb.ToString();
                                        CurrentToken = Token.Literal;
                                        _literal = _sb.ToString();
                                        NextChar();
                                        return;
                                    }
                                    else
                                    {
                                        _sb.Append(_currentChar);
                                    }

                                    NextChar();
                                }
                                throw new InvalidDataException("syntax error, unterminated string literal");
                            }


                    }

                    // Number?
                    if (char.IsDigit(_currentChar) || _currentChar == '-')
                    {
                        TokenizeNumber();
                        return;
                    }

                    // Identifier?  (checked for after everything else as identifiers are actually quite rare in valid json)
                    if (Char.IsLetter(_currentChar) || _currentChar == '_' || _currentChar == '$')
                    {
                        // Find end of identifier
                        _sb.Length = 0;
                        while (Char.IsLetterOrDigit(_currentChar) || _currentChar == '_' || _currentChar == '$')
                        {
                            _sb.Append(_currentChar);
                            NextChar();
                        }
                        String = _sb.ToString();

                        // Handle special identifiers
                        switch (String)
                        {
                            case "true":
                                _literal = 1;
                                CurrentToken = Token.Literal;
                                return;

                            case "false":
                                _literal = 0;
                                CurrentToken = Token.Literal;
                                return;

                            case "shl":
                                CurrentToken = Token.ShiftLeft;
                                return;

                            case "shr":
                                CurrentToken = Token.ShiftRight;
                                return;
                        }

                        CurrentToken = Token.Identifier;
                        return;
                    }

                    // What the?
                    throw new InvalidDataException(string.Format("syntax error, unexpected character '{0}'", _currentChar));
                }
            }

            // Parse a sequence of characters that could make up a valid number
            // For performance, we don't actually parse it into a number yet.  When using PetaJsonEmit we parse
            // later, directly into a value type to avoid boxing
            private void TokenizeNumber()
            {
                _sb.Length = 0;

                // Leading negative sign
                bool signed = false;
                if (_currentChar == '-')
                {
                    signed = true;
                    _sb.Append(_currentChar);
                    NextChar();
                }

                // Hex prefix?
                bool hexPrefix = false;
                bool mightBeHex = false;
                if (_currentChar == '0')
                {
                    mightBeHex = true;
                    _sb.Append(_currentChar);
                    NextChar();
                    if (_currentChar == 'x' || _currentChar == 'X')
                    {
                        _sb.Append(_currentChar);
                        NextChar();
                        hexPrefix = true;
                    }
                }

                // Process characters, but vaguely figure out what type it is
                bool cont = true;
                bool fp = false;
                while (cont)
                {
                    switch (_currentChar)
                    {
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            _sb.Append(_currentChar);
                            NextChar();
                            break;

                        case 'A':
                        case 'a':
                        case 'B':
                        case 'b':
                        case 'C':
                        case 'c':
                        case 'D':
                        case 'd':
                        case 'F':
                        case 'f':
                            if (!mightBeHex)
                                cont = false;
                            else
                            {
                                _sb.Append(_currentChar);
                                NextChar();
                            }
                            break;

                        case '.':
                            if (hexPrefix)
                            {
                                cont = false;
                            }
                            else
                            {
                                fp = true;
                                _sb.Append(_currentChar);
                                NextChar();
                            }
                            break;

                        case 'E':
                        case 'e':
                            if (!hexPrefix)
                            {
                                fp = true;
                                _sb.Append(_currentChar);
                                NextChar();
                                if (_currentChar == '+' || _currentChar == '-')
                                {
                                    _sb.Append(_currentChar);
                                    NextChar();
                                }
                            }
                            else
                            {
                                _sb.Append(_currentChar);
                                NextChar();
                            }
                            break;

                        default:
                            cont = false;
                            break;
                    }
                }

                // Hex suffix?
                bool hexSuffix = false;
                if (_currentChar == 'h' || _currentChar == 'H')
                {
                    hexSuffix = true;
                    NextChar();
                }

                if (char.IsLetter(_currentChar))
                    throw new InvalidDataException(string.Format("syntax error, invalid character following number '{0}'", _sb.ToString()));

                // Setup token
                String = _sb.ToString();
                CurrentToken = Token.Literal;

                // Setup literal kind
                if (fp)
                {
                    _literal = double.Parse(String, CultureInfo.InvariantCulture);
                }
                else
                {
                    if (hexPrefix)
                    {
                        _literal = Convert.ToUInt64(String.Substring(2), 16);
                    }
                    else if (hexSuffix)
                    {
                        _literal = Convert.ToUInt64(String, 16);
                    }
                    else if (signed)
                    {
                        _literal = long.Parse(String, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        _literal = ulong.Parse(String, CultureInfo.InvariantCulture);
                    }
                }

            }

            // Check the current token, throw exception if mismatch
            public void Check(Token tokenRequired)
            {
                if (tokenRequired != CurrentToken)
                {
                    throw new InvalidDataException(string.Format("syntax error, expected {0} found {1}", tokenRequired, CurrentToken));
                }
            }

            public void SkipToEnd()
            {
                while (_currentChar != '\0')
                    NextChar();
                CurrentToken = Token.EOF;
            }

            // Skip token which must match
            public void Skip(Token tokenRequired)
            {
                Check(tokenRequired);
                NextToken();
            }

            // Skip token if it matches
            public bool SkipIf(Token tokenRequired)
            {
                if (tokenRequired == CurrentToken)
                {
                    NextToken();
                    return true;
                }
                return false;
            }
        }

    }
}

