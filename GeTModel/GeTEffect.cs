using System.Collections.Generic;
using Proto;
namespace GeTPlanModel
{
    [System.Serializable]
    public class GeTEffect
    {
        public GeTEffectExpression EffectExpression { get; set; }
        public GeTTiming OccurrenceTime { get; set; }
        public List<int> ParametersMap { get; set; }

        public GeTEffect(GeTEffectExpression effect, GeTTiming occurrenceTime)
        {
            EffectExpression = effect;
            OccurrenceTime = occurrenceTime;
        }

        public override string ToString()
        {
            if (OccurrenceTime == null)
                return EffectExpression.ToString();
            return OccurrenceTime.ToString() + " " + EffectExpression.ToString();
        }

        public void SetParametersMap(List<GeTParameter> actionParameters)
        {
            if (actionParameters.Count == 0)
            {
                ParametersMap = new List<int>();
                return;
            }
            ParametersMap = new List<int>();
            var effectParameters = EffectExpression.GetParameters();

            foreach (var effectParameter in effectParameters)
            {
                var index = actionParameters.FindIndex(p => p.Name == effectParameter.Name);
                ParametersMap.Add(index);
            }
        }
    }

    public class EffectModelFactory : IGeTModelFactory<GeTEffect, Effect>, IProtoFactory<GeTEffect, Effect>
    {
        public GeTEffect FromProto(Effect proto)
        {
            var tmf = new TimingModelFactory();
            var eemf = new EffectExpressionModelFactory();

            var effect = eemf.FromProto(proto.Effect_);

            if (proto.OccurrenceTime == null)
            {
                var GeTeffect = new GeTEffect(effect, null);
                return GeTeffect;
            }

            var occurrenceTime = tmf.FromProto(proto.OccurrenceTime);

            return new GeTEffect(effect, occurrenceTime);
        }

        public Effect ToProto(GeTEffect model)
        {
            var proto = new Effect();
            var tmf = new TimingModelFactory();
            var eemf = new EffectExpressionModelFactory();

            proto.Effect_ = eemf.ToProto(model.EffectExpression);
            if (model.OccurrenceTime != null)
            {
                proto.OccurrenceTime = tmf.ToProto(model.OccurrenceTime);
            }
            return proto;
        }
    }
}
