using Proto;

namespace GeTPlanModel
{
    [System.Serializable]
    public class GeTTiming
    {
        public GeTTimePoint timePoint { get; set; }
        public GeTReal delay { get; set; }
        public GeTTiming(GeTTimePoint timePoint, GeTReal delay)
        {
            this.timePoint = timePoint;
            this.delay = delay;
        }

        public override string ToString()
        {
            var delayString = delay.ToDouble() > 0 ? $" Delay: {delay.ToDouble()}" : "";

            return $"{timePoint}{delayString}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            GeTTiming other = (GeTTiming)obj;

            return timePoint.Equals(other.timePoint) && delay.Equals(other.delay);
        }

        public override int GetHashCode()
        {
            return timePoint.GetHashCode() + delay.GetHashCode();
        }
    }

    public class TimingModelFactory : IGeTModelFactory<GeTTiming, Timing>, IProtoFactory<GeTTiming, Timing>
    {

        public GeTTiming FromProto(Timing proto)
        {
            var tpmf = new TimePointModelFactory();
            var rmf = new RealModelFactory();

            var timePoint = tpmf.FromProto(proto.Timepoint);
            var delay = rmf.FromProto(proto.Delay);
            return new GeTTiming(timePoint, delay);
        }

        public Timing ToProto(GeTTiming model)
        {
            var tpmf = new TimePointModelFactory();
            var rmf = new RealModelFactory();

            var proto = new Timing();
            proto.Timepoint = tpmf.ToProto(model.timePoint);
            proto.Delay = rmf.ToProto(model.delay);
            return proto;
        }
    }
   
}