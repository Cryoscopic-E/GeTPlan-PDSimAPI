using System.Collections.Generic;
using Proto;
namespace GeTPlanModel
{
    public enum EffectExpressionKind
    {
        ASSIGNMENT,
        INCREMENT,
        DECREMENT
    }

    public class GeTEffectExpression
    {
        public EffectExpressionKind Kind { get; set; }
        public GeTExpression Fluent { get; set; }
        public GeTExpression Value { get; set; }
        // If the effect is conditional
        public GeTExpression Condition { get; set; }
        // we shouldn't receive nor need this in PDSim as the server compiles the problem to relax the adls
        public List<GeTExpression> Forall { get; set; }


        public GeTEffectExpression(EffectExpressionKind kind, GeTExpression fluent, GeTExpression value, GeTExpression condition, List<GeTExpression> forall)
        {
            Kind = kind;
            Fluent = fluent;
            Value = value;
            Condition = condition;
            Forall = forall;
        }

        public override string ToString()
        {
            var result = "";
            switch (Kind)
            {
                case EffectExpressionKind.ASSIGNMENT:
                    result += "Assign ";
                    break;
                case EffectExpressionKind.INCREMENT:
                    result += "Increment ";
                    break;
                case EffectExpressionKind.DECREMENT:
                    result += "Decrement ";
                    break;
            }

            result += Fluent.ToString() + ":=" + Value.ToString();

            if (Forall.Count > 0)
            {
                result += " for all ";
                foreach (var f in Forall)
                {
                    result += f.ToString() + ", ";
                }
            }

            return result;
        }

        public List<GeTParameter> GetParameters()
        {
            var effectParameters = new List<GeTParameter>();
            foreach (var f in Fluent.SubExpressions)
            {
                if (f.Kind == ExpressionKind.Parameter)
                {
                    effectParameters.Add(f.Parameter);
                }
            }
            return effectParameters;
        }
    }



    public class EffectExpressionModelFactory : IGeTModelFactory<GeTEffectExpression, EffectExpression>, IProtoFactory<GeTEffectExpression, EffectExpression>
    {
        public GeTEffectExpression FromProto(EffectExpression proto)
        {
            var emf = new ExpressionModelFactory();

            var kind = EffectExpressionKind.ASSIGNMENT;
            switch (proto.Kind)
            {
                case EffectExpression.Types.EffectKind.Increase:
                    kind = EffectExpressionKind.INCREMENT;
                    break;
                case EffectExpression.Types.EffectKind.Decrease:
                    kind = EffectExpressionKind.DECREMENT;
                    break;
            }

            var value = emf.FromProto(proto.Value);
            var condition = emf.FromProto(proto.Condition);
            var fluent = emf.FromProto(proto.Fluent);
            var forall = new List<GeTExpression>();
            foreach (var f in proto.Forall)
            {
                forall.Add(emf.FromProto(f));
            }

            return new GeTEffectExpression(kind, fluent, value, condition, forall);
        }

        public EffectExpression ToProto(GeTEffectExpression expression)
        {
            var emf = new ExpressionModelFactory();
            var proto = new EffectExpression();

            proto.Kind = expression.Kind switch
            {
                EffectExpressionKind.ASSIGNMENT => EffectExpression.Types.EffectKind.Assign,
                EffectExpressionKind.INCREMENT => EffectExpression.Types.EffectKind.Increase,
                EffectExpressionKind.DECREMENT => EffectExpression.Types.EffectKind.Decrease,
                _ => EffectExpression.Types.EffectKind.Assign
            };
            proto.Fluent = emf.ToProto(expression.Fluent);
            proto.Value = emf.ToProto(expression.Value);
            proto.Condition = emf.ToProto(expression.Condition);
            foreach (var f in expression.Forall)
            {
                proto.Forall.Add(emf.ToProto(f));
            }
            return proto;
        }
    }

}
