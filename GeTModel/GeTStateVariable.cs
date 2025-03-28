using Proto;
using System.Collections.Generic;
namespace GeTPlanModel
{
    [System.Serializable]
    public class GeTStateVariable
    {
        public GeTExpression Fluent { get; set; }
        public GeTExpressionString FluentString { get; set; }
        public GeTExpression Value { get; set; }
        public GeTExpressionString ValueString { get; set; }
        public GeTStateVariable(GeTExpression fluent, GeTExpression value)
        {
            Fluent = fluent;
            Value = value;
            FluentString = new GeTExpressionString(fluent);
            ValueString = new GeTExpressionString(value);
        }

        public List<string> GetParameters()
        {
            var p = new List<string>();
            foreach (var parameter in Fluent.SubExpressions)
            {
                p.Add(parameter.ToString());
            }
            return p;
        }
        public override string ToString()
        {
            return $"{Fluent} := {Value}";
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            GeTStateVariable other = (GeTStateVariable)obj;
            return Fluent.Equals(other.Fluent) && Value.Equals(other.Value);
        }

        public override int GetHashCode()
        {
            return Fluent.GetHashCode() + Value.GetHashCode();
        }
    }

    public class StateVariableFactory : IGeTModelFactory<GeTStateVariable, Assignment>, IProtoFactory<GeTStateVariable, Assignment>
    {
        public static GeTStateVariable CreateAssignment(string fluentName, List<string> parameters, object value)
        {
            var subExpressions = new List<GeTExpression>();
            foreach (var parameter in parameters)
            {
                var paramExpression = new GeTExpression(AtomModelFactory.Create(parameter), ExpressionKind.Constant);
                subExpressions.Add(paramExpression);
            }

            var fluentExpression = new GeTExpression(fluentName, subExpressions, ExpressionKind.StateVariable);

            var valueExpression = new GeTExpression(AtomModelFactory.Create(value), ExpressionKind.Constant);

            return new GeTStateVariable(fluentExpression, valueExpression);
        }
        public GeTStateVariable FromProto(Assignment assignment)
        {
            if (assignment == null) return null;
            var emf = new ExpressionModelFactory();

            // Convert the Fluent part of the assignment
            GeTExpression fluentModel = emf.FromProto(assignment.Fluent);

            // Convert the Value part of the assignment
            GeTExpression valueModel = emf.FromProto(assignment.Value);

            // Return the FluentAssignmentModel
            return new GeTStateVariable(fluentModel, valueModel);
        }

        public Assignment ToProto(GeTStateVariable model)
        {
            if (model == null) return null;
            var emf = new ExpressionModelFactory();
            // Convert the Fluent part of the assignment
            Expression fluentProto = emf.ToProto(model.Fluent);
            // Convert the Value part of the assignment
            Expression valueProto = emf.ToProto(model.Value);
            // Return the FluentAssignmentModel
            return new Assignment { Fluent = fluentProto, Value = valueProto };
        }
    }
}
