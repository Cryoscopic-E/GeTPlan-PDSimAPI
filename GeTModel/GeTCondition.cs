using Proto;
namespace GeTPlanModel
{
    public class GeTCondition
    {
        public GeTExpression Condition { get; set; }

        public GeTTimeInterval? Span { get; set; }

        public GeTCondition(GeTExpression condition, GeTTimeInterval? span = null)
        {
            Condition = condition;
            Span = span;
        }

        public override string ToString()
        {
            return Span != null ? $"{Span} {Condition}" : $"{Condition}";
        }
    }

    public class ConditionModelFactory : IGeTModelFactory<GeTCondition, Condition>, IProtoFactory<GeTCondition, Condition>
    {
        public GeTCondition FromProto(Condition proto)
        {
            var emf = new ExpressionModelFactory();
            var timf = new TimeIntervalModelFactory();

            var condition = emf.FromProto(proto.Cond);
            if (proto.Span == null)
            {
                return new GeTCondition(condition);
            }

            var span = timf.FromProto(proto.Span);
            return new GeTCondition(condition, span);
        }

        public Condition ToProto(GeTCondition model)
        {
            var proto = new Condition();
            var emf = new ExpressionModelFactory();
            var timf = new TimeIntervalModelFactory();

            proto.Cond = emf.ToProto(model.Condition);
            if (model.Span != null)
            {
                proto.Span = timf.ToProto(model.Span);
            }
            return proto;
        }
    }
}
