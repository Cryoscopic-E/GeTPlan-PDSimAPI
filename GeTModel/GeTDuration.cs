using Proto;
namespace GeTPlanModel
{
    [System.Serializable]
    public class GeTDuration
    {
        public GeTInterval Interval { get; set; }

        public GeTDuration(GeTInterval timeInterval)
        {
            Interval = timeInterval;
        }

        public override string ToString()
        {
            return $"{Interval}";
        }
    }

    public class DurationModelFactory : IGeTModelFactory<GeTDuration, Duration>, IProtoFactory<GeTDuration, Duration>
    {
        public GeTDuration FromProto(Duration duration)
        {
            var imf = new IntervalModelFactory();
            var interval = imf.FromProto(duration.ControllableInBounds);
            return new GeTDuration(interval);
        }

        public Duration ToProto(GeTDuration duration)
        {
            var imf = new IntervalModelFactory();
            var proto = new Duration();

            proto.ControllableInBounds = imf.ToProto(duration.Interval);
            return proto;
        }
    }
}
