using System.Collections.Generic;
using Proto;
namespace GeTPlanModel
{
    /// <summary>
    /// GeT Action
    /// 
    /// Represents an action in the planning problem.
    /// <example>
    /// Definition example: 
    /// <code>
    ///     stack(?x - block, ?y - block)
    ///         preconditions: (clear ?x) & (holding ?y)
    ///         effects: (clear ?y) & (on ?x ?y)
    /// </code>
    /// </example>
    /// </summary>
    [System.Serializable]
    public class GeTAction
    {
        public string actionName;
        public List<GeTParameter> parameters;
        public GeTDuration? duration;
        public List<GeTCondition> conditions;
        public List<GeTEffect> effects;
        
        public GeTAction(string actionName,
                        List<GeTParameter> parameters,
                        List<GeTCondition> conditions,
                        List<GeTEffect> effects,
                        GeTDuration? duration = null)
        {
            this.actionName = actionName;
            this.parameters = parameters;
            this.conditions = conditions;
            this.effects = effects;
            this.duration = duration;
        }

        public override string ToString()
        {
            var outString = string.Empty;

            outString += $"{actionName} ({string.Join(", ", parameters)})\n";
            outString += "-- CONDITIONS --\n";
            foreach (var condition in conditions)
            {
                outString += $"{condition}\n";
            }
            outString += "-- EFFECTS --\n";
            foreach (var effect in effects)
            {
                outString += $"{effect}\n";
            }
            if (duration != null)
                outString += $"Duration: {duration}";
            return outString;
        }
    }

    public class ActionModelFactory : IGeTModelFactory<GeTAction, Action>, IProtoFactory<GeTAction, Proto.Action>
    {
        public GeTAction FromProto(Action action)
        {
            var pmf = new ParameterModelFactory();
            var cmf = new ConditionModelFactory();
            var emf = new EffectModelFactory();
            var dmf = new DurationModelFactory();

            var actionName = action.Name;
            var parameters = new List<GeTParameter>();

            foreach (var parameter in action.Parameters)
            {
                var p = pmf.FromProto(parameter);
                parameters.Add(p);
            }

            var conditions = new List<GeTCondition>();
            foreach (var condition in action.Conditions)
            {
                var c = cmf.FromProto(condition);
                conditions.Add(c);
            }

            var effects = new List<GeTEffect>();
            foreach (var effect in action.Effects)
            {
                var e = emf.FromProto(effect);
                e.SetParametersMap(parameters);
                effects.Add(e);
            }

            if (action.Duration == null)
                return new GeTAction(actionName, parameters, conditions, effects);

            var duration = dmf.FromProto(action.Duration);
            return new GeTAction(actionName, parameters, conditions, effects, duration);
        }

        public Action ToProto(GeTAction model)
        {
            var pmf = new ParameterModelFactory();
            var cmf = new ConditionModelFactory();
            var emf = new EffectModelFactory();
            var dmf = new DurationModelFactory();

            var proto = new Action
            {
                Name = model.actionName,
                Duration = model.duration != null ? dmf.ToProto(model.duration) : null
            };
            foreach (var parameter in model.parameters)
            {
                proto.Parameters.Add(pmf.ToProto(parameter));
            }
            foreach (var condition in model.conditions)
            {
                proto.Conditions.Add(cmf.ToProto(condition));
            }
            foreach (var effect in model.effects)
            {
                proto.Effects.Add(emf.ToProto(effect));
            }
            return proto;
        }
    }
}