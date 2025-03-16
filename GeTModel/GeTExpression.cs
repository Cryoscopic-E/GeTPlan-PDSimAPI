using System.Collections.Generic;
using Proto;
namespace GeTPlanModel
{
    public class GeTExpression
    {
        public GeTAtom? Atom { get; private set; }  // For constants
        public List<GeTExpression> SubExpressions { get; private set; }  // For function applications or state variables
        public GeTParameter? Parameter { get; private set; }  // For parameters
        public string? FluentName { get; private set; }  // For fluent symbols
        public string? VariableName { get; private set; }  // For variables
        public ExpressionKind Kind { get; private set; }

        // Constructor for atom-based expressions
        public GeTExpression(GeTAtom atom, ExpressionKind kind)
        {
            Atom = atom;
            Kind = kind;
            SubExpressions = new List<GeTExpression>();
        }

        // Constructor for parameter-based expressions
        public GeTExpression(GeTParameter parameter, ExpressionKind kind)
        {
            Parameter = parameter;
            Kind = kind;
            SubExpressions = new List<GeTExpression>();
        }

        // Constructor for fluent or function-based expressions
        public GeTExpression(string fluentOrFunctionName, List<GeTExpression> subExpressions, ExpressionKind kind)
        {
            FluentName = fluentOrFunctionName;
            SubExpressions = subExpressions ?? new List<GeTExpression>();
            Kind = kind;
        }

        // Constructor for variable-based expressions
        public GeTExpression(string variableName, ExpressionKind kind, bool isVariable)
        {
            VariableName = variableName;
            Kind = kind;
            SubExpressions = new List<GeTExpression>();
        }

        public override string ToString()
        {
            switch (Kind)
            {
                case ExpressionKind.Constant:
                    return Atom != null ? Atom.ToString() : "Unknown";
                case ExpressionKind.Parameter:
                    return $"{Parameter}";
                case ExpressionKind.FluentSymbol:
                    return $"{FluentName}";
                case ExpressionKind.FunctionApplication:
                    switch (FluentName)
                    {
                        case "up:and":
                            return $"{string.Join(" & ", SubExpressions)}";
                        case "up:or":
                            return $"{string.Join(" | ", SubExpressions)}";
                        case "up:not":
                            return $"¬({SubExpressions[0]})";
                        case "up:implies":
                            return $"{SubExpressions[0]} -> {SubExpressions[1]}";
                        case "up:exists":
                            return $"∃{SubExpressions[0]}.{SubExpressions[1]}";
                        case "up:forall":
                            return $"∀{SubExpressions[0]}.{SubExpressions[1]}";
                        case "up:plus":
                            return $"{SubExpressions[0]} + {SubExpressions[1]}";
                        case "up:minus":
                            return $"{SubExpressions[0]} - {SubExpressions[1]}";
                        case "up:times":
                            return $"{SubExpressions[0]} * {SubExpressions[1]}";
                        case "up:div":
                            return $"{SubExpressions[0]} / {SubExpressions[1]}";
                        case "up:equals":
                            return $"{SubExpressions[0]} == {SubExpressions[1]}";
                        case "up:le":
                            return $"{SubExpressions[0]} ≤ {SubExpressions[1]}";
                        case "up:lt":
                            return $"{SubExpressions[0]} < {SubExpressions[1]}";
                        default:
                            return $"{FluentName}({string.Join(", ", SubExpressions)})";
                    }
                case ExpressionKind.Variable:
                    return $"{VariableName}";
                case ExpressionKind.StateVariable:
                    return $"{FluentName}({string.Join(", ", SubExpressions)})";
                default:
                    return "Unknown Expression";
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GeTExpression other))
                return false;

            if (Kind != other.Kind)
                return false;

            // Check based on kind
            switch (Kind)
            {
                case ExpressionKind.Constant:
                    return Atom?.Equals(other.Atom) == true;
                case ExpressionKind.Parameter:
                    return Parameter?.Equals(other.Parameter) == true;
                case ExpressionKind.FluentSymbol:
                    return FluentName == other.FluentName;
                case ExpressionKind.FunctionApplication:
                case ExpressionKind.StateVariable:
                    return FluentName == other.FluentName && AreSubExpressionsEqual(SubExpressions, other.SubExpressions);
                case ExpressionKind.Variable:
                    return VariableName == other.VariableName;
                default:
                    return false;
            }
        }

        private bool AreSubExpressionsEqual(IReadOnlyList<GeTExpression> sub1, IReadOnlyList<GeTExpression> sub2)
        {
            if (sub1.Count != sub2.Count)
                return false;

            for (int i = 0; i < sub1.Count; i++)
            {
                if (!sub1[i].Equals(sub2[i]))
                    return false;
            }

            return true;
        }

        // GetHashCode should be overridden for consistency with Equals
        public override int GetHashCode()
        {
            int hash = Kind.GetHashCode();

            hash = (hash * 397) ^ (Atom?.GetHashCode() ?? 0);
            hash = (hash * 397) ^ (Parameter?.GetHashCode() ?? 0);
            hash = (hash * 397) ^ (FluentName?.GetHashCode() ?? 0);
            hash = (hash * 397) ^ (VariableName?.GetHashCode() ?? 0);

            foreach (var subExpr in SubExpressions)
            {
                hash = (hash * 397) ^ subExpr.GetHashCode();
            }

            return hash;
        }
    }


    public class ExpressionModelFactory : IGeTModelFactory<GeTExpression, Expression>, IProtoFactory<GeTExpression, Expression>
    {

        public static GeTExpression CreateGroundFluent(string fluentName, List<string> parameters)
        {
            var subExpressions = new List<GeTExpression>();
            foreach (var parameter in parameters)
            {
                var paramExpression = new GeTExpression(AtomModelFactory.Create(parameter), ExpressionKind.Constant);
                subExpressions.Add(paramExpression);
            }

            return new GeTExpression(fluentName, subExpressions, ExpressionKind.StateVariable);
        }

        public GeTExpression FromProto(Expression expression)
        {
            if (expression == null) return null;
            
            var amf = new AtomModelFactory();

            switch (expression.Kind)
            {
                case ExpressionKind.Constant:
                    return new GeTExpression(amf.FromProto(expression.Atom), ExpressionKind.Constant);
                case ExpressionKind.Parameter:
                    return new GeTExpression(new GeTParameter(expression.Atom.Symbol, expression.Type), ExpressionKind.Parameter);
                case ExpressionKind.FluentSymbol:
                    return new GeTExpression(expression.Atom.Symbol, null, ExpressionKind.FluentSymbol);
                case ExpressionKind.FunctionSymbol:
                    return new GeTExpression(expression.Atom.Symbol, null, ExpressionKind.FunctionSymbol);
                case ExpressionKind.StateVariable:
                case ExpressionKind.FunctionApplication:
                    // For fluents, functions, and state variables, the first sub-expression is the name,
                    // and the rest are arguments
                    if (expression.List != null && expression.List.Count > 0)
                    {
                        var subExpressions = new List<GeTExpression>();
                        for (int i = 1; i < expression.List.Count; i++)
                        {
                            subExpressions.Add(FromProto(expression.List[i]));
                        }
                        return new GeTExpression(expression.List[0].Atom.Symbol, subExpressions, expression.Kind);
                    }
                    return null;

                case ExpressionKind.Variable:
                    return new GeTExpression(expression.Atom.Symbol, ExpressionKind.Variable, true);

                default:
                    return null;
            }
        }

        public Expression ToProto(GeTExpression expression)
        {
            if (expression == null) return null;

            var amf = new AtomModelFactory();
            var protoExpression = new Expression();
            
            switch (expression.Kind)
            {
                case ExpressionKind.Constant:
                    protoExpression.Atom = amf.ToProto(expression.Atom);
                    break;
                case ExpressionKind.Parameter:
                    protoExpression.Atom = new Atom { Symbol = expression.Parameter.Name };
                    break;
                case ExpressionKind.FluentSymbol:
                    protoExpression.Atom = new Atom { Symbol = expression.FluentName };
                    break;
                case ExpressionKind.FunctionSymbol:
                    protoExpression.Atom = new Atom { Symbol = expression.FluentName };
                    break;
                case ExpressionKind.FunctionApplication:
                case ExpressionKind.StateVariable:
                    protoExpression.List.Add(new Expression { Atom = new Atom { Symbol = expression.FluentName } });
                    foreach (var subExpression in expression.SubExpressions)
                    {
                        protoExpression.List.Add(ToProto(subExpression));
                    }
                    break;
                case ExpressionKind.Variable:
                    protoExpression.Atom = new Atom { Symbol = expression.VariableName };
                    break;
                default:
                    return null;
            }
            return protoExpression;
        }
    }


}
