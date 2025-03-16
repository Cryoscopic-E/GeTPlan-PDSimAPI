using System;
using System.Collections.Generic;
using System.Text;

namespace PDSimAPI.Utils
{
    public enum TokenType
    {
        Operator,
        Number,
        Comma,
        LParen,
        RParen,
        EOF
    }

    public class Token
    {
        public TokenType Type;
        public string Value;
        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }
    }

    public class Tokenizer
    {
        private readonly string text;
        private int pos;

        public Tokenizer(string text)
        {
            this.text = text;
            pos = 0;
        }

        public Token GetNextToken()
        {
            while (pos < text.Length)
            {
                char current = text[pos];
                if (char.IsWhiteSpace(current))
                {
                    pos++;
                    continue;
                }
                if (current == '(')
                {
                    pos++;
                    return new Token(TokenType.LParen, "(");
                }
                if (current == ')')
                {
                    pos++;
                    return new Token(TokenType.RParen, ")");
                }
                if (current == ',')
                {
                    pos++;
                    return new Token(TokenType.Comma, ",");
                }
                if ("+-*/".IndexOf(current) != -1)
                {
                    // Check for a minus sign that might be part of a number.
                    // If the '-' is followed by a digit or a dot, we treat it as part of a number.
                    if (current == '-' && pos + 1 < text.Length && (char.IsDigit(text[pos + 1]) || text[pos + 1] == '.'))
                    {
                        // Let the number-handling block below process this.
                    }
                    else
                    {
                        pos++;
                        return new Token(TokenType.Operator, current.ToString());
                    }
                }
                // Number handling: supports integers, floats, and negatives.
                if (char.IsDigit(current) || current == '-' || current == '.')
                {
                    int start = pos;
                    // If there is a minus sign, include it
                    if (text[pos] == '-')
                    {
                        pos++;
                    }
                    bool dotFound = false;
                    while (pos < text.Length && (char.IsDigit(text[pos]) || (text[pos] == '.' && !dotFound)))
                    {
                        if (text[pos] == '.')
                        {
                            dotFound = true;
                        }
                        pos++;
                    }
                    string numStr = text.Substring(start, pos - start);
                    return new Token(TokenType.Number, numStr);
                }
                throw new Exception($"Unexpected character: {current}");
            }
            return new Token(TokenType.EOF, "");
        }
    }

    public abstract class ExprNode
    {
        public abstract string ToInfix();
    }

    public class NumberNode : ExprNode
    {
        public string Value;
        public NumberNode(string value)
        {
            Value = value;
        }
        public override string ToInfix() => Value;
    }

    public class OperatorNode : ExprNode
    {
        public string Operator;
        public List<ExprNode> Arguments;
        public OperatorNode(string op)
        {
            Operator = op;
            Arguments = new List<ExprNode>();
        }
        public override string ToInfix()
        {
            // For a single argument, simply return a prefix form.
            if (Arguments.Count == 1)
                return $"{Operator}{Arguments[0].ToInfix()}";
            // For two or more arguments, join them with the operator.
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            for (int i = 0; i < Arguments.Count; i++)
            {
                sb.Append(Arguments[i].ToInfix());
                if (i != Arguments.Count - 1)
                {
                    sb.Append($" {Operator} ");
                }
            }
            sb.Append(")");
            return sb.ToString();
        }
    }

    public class Parser
    {
        private Tokenizer tokenizer;
        private Token currentToken;

        public Parser(string text)
        {
            tokenizer = new Tokenizer(text);
            currentToken = tokenizer.GetNextToken();
        }

        private void Eat(TokenType type)
        {
            if (currentToken.Type == type)
            {
                currentToken = tokenizer.GetNextToken();
            }
            else
            {
                throw new Exception($"Expected token {type} but found {currentToken.Type}");
            }
        }

        // Grammar:
        // expr ::= number | operator '(' expr (',' expr)* ')'
        public ExprNode ParseExpression()
        {
            if (currentToken.Type == TokenType.Number)
            {
                var node = new NumberNode(currentToken.Value);
                Eat(TokenType.Number);
                return node;
            }
            if (currentToken.Type == TokenType.Operator)
            {
                string op = currentToken.Value;
                Eat(TokenType.Operator);
                Eat(TokenType.LParen);
                OperatorNode opNode = new OperatorNode(op);
                opNode.Arguments.Add(ParseExpression());
                while (currentToken.Type == TokenType.Comma)
                {
                    Eat(TokenType.Comma);
                    opNode.Arguments.Add(ParseExpression());
                }
                Eat(TokenType.RParen);
                return opNode;
            }
            throw new Exception("Invalid expression");
        }
    }

    public class PrefixToInfixConverter
    {
        public static string Convert(string prefixExpression)
        {
            Parser parser = new Parser(prefixExpression);
            ExprNode root = parser.ParseExpression();
            return root.ToInfix();
        }
    }
}