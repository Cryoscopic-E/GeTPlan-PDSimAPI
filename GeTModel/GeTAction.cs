using System.Collections.Generic;
using Proto;
namespace GeTPlanModel
{
    public class GeTAction
    {
        public string actionName;
        public List<GeTParameter> parameters;
        public GeTDuration? duration; // Make duration nullable
        public List<GeTCondition> conditions;
        public List<GeTEffect> effects;

        //Definition example: stack(x - block, y - block)
        //Action Instance example: stack(A,B)
        //Effect Definition: on(x,y) [0,1]
        //Effect Definition: clear(x) [0]
        //Effect Definition: ¬clear(y) [1]
        public GeTAction(string actionName,
                        List<GeTParameter> parameters,
                        List<GeTCondition> conditions,
                        List<GeTEffect> effects,
                        GeTDuration? duration = null) // Make duration nullable
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