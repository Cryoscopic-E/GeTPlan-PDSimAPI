using Proto;

namespace GeTPlanModel
{
    [System.Serializable]
    public class GeTTimeInterval
    {
        public bool leftOpen { get; set; }
        public bool rightOpen { get; set; }
        public GeTTiming lowerBound { get; set; }
        public GeTTiming upperBound { get; set; }

        public GeTTimeInterval(bool leftOpen, bool rightOpen, GeTTiming lowerBound, GeTTiming upperBound)
        {
            this.leftOpen = leftOpen;
            this.rightOpen = rightOpen;
            this.lowerBound = lowerBound;
            this.upperBound = upperBound;
        }

        public override string ToString()
        {
            if (lowerBound.Equals(upperBound))
                return $"{(leftOpen ? "(" : "[")}{lowerBound}{(rightOpen ? ")" : "]")}";
            return $"{(leftOpen ? "(" : "[")}{lowerBound}, {upperBound}{(rightOpen ? ")" : "]")}";
        }

    }

    public class TimeIntervalModelFactory : IGeTModelFactory<GeTTimeInterval, TimeInterval>, IProtoFactory<GeTTimeInterval, TimeInterval>
    {
        public GeTTimeInterval FromProto(TimeInterval proto)
        {
            var tmf = new TimingModelFactory();

            var leftOpen = proto.IsLeftOpen;
            var rightOpen = proto.IsRightOpen;
            var lowerBound = tmf.FromProto(proto.Lower);
            var upperBound = tmf.FromProto(proto.Upper);
            return new GeTTimeInterval(leftOpen, rightOpen, lowerBound, upperBound);
        }

        public TimeInterval ToProto(GeTTimeInterval model)
        {
            var proto = new TimeInterval();
            var tmf = new TimingModelFactory();

            proto.IsLeftOpen = model.leftOpen;
            proto.IsRightOpen = model.rightOpen;
            proto.Lower = tmf.ToProto(model.lowerBound);
            proto.Upper = tmf.ToProto(model.upperBound);
            return proto;
        }
    }
}
