using Proto;
namespace GeTPlanModel
{
    [System.Serializable]
    public class GeTGoal
    {
        public GeTExpression GoalExpression { get; set; }
        public GeTExpressionString GoalExpressionString { get; set; }
        public GeTTimeInterval? Timing { get; set; }

        public GeTGoal(GeTExpression goalExpression, GeTTimeInterval? timing = null)
        {
            this.GoalExpression = goalExpression;
            this.Timing = timing;

            GoalExpressionString = new GeTExpressionString(goalExpression);
        }

        public override string ToString()
        {
            if (Timing == null)
                return $"{GoalExpression}";
            return $"{Timing} {GoalExpression}";
        }
    }

    public class GoalModelFactory : IGeTModelFactory<GeTGoal, Goal>, IProtoFactory<GeTGoal, Goal>
    {
        public GeTGoal FromProto(Goal goal)
        {
            var emf = new ExpressionModelFactory();
            var timf = new TimeIntervalModelFactory();

            var expression = emf.FromProto(goal.Goal_);
            if (goal.Timing == null)
                return new GeTGoal(expression);
            var timing = timf.FromProto(goal.Timing);
            return new GeTGoal(expression, timing);
        }
        public Goal ToProto(GeTGoal model)
        {
            var proto = new Goal();
            var emf = new ExpressionModelFactory();
            var timf = new TimeIntervalModelFactory();

            proto.Goal_ = emf.ToProto(model.GoalExpression);
            if (model.Timing != null)
                proto.Timing = timf.ToProto(model.Timing);
            return proto;
        }
    }
}
