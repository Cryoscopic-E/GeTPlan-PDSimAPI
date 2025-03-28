using Proto;
using System;

namespace GeTPlanModel
{
    [System.Serializable]
    public enum TimepointKind
    {
        GLOBAL_START,
        GLOBAL_END,
        START,
        END
    }

    [System.Serializable]
    public class GeTTimePoint
    {
        public TimepointKind type;
        public string containerID;

        public GeTTimePoint(TimepointKind type, string containerID)
        {
            this.type = type;
            this.containerID = containerID;
        }

        public override string ToString()
        {
            return $"{type}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            GeTTimePoint other = (GeTTimePoint)obj;

            if (containerID == null)
            {
                return type == other.type;
            }
            return type == other.type && containerID == other.containerID;
        }

        public override int GetHashCode()
        {
            return type.GetHashCode() + containerID.GetHashCode();
        }

        public static TimepointKind GetTimepointKind(Timepoint.Types.TimepointKind timepointKind)
        {
            switch (timepointKind)
            {
                default:
                case Timepoint.Types.TimepointKind.GlobalStart:
                    return TimepointKind.GLOBAL_START;
                case Timepoint.Types.TimepointKind.GlobalEnd:
                    return TimepointKind.GLOBAL_END;
                case Timepoint.Types.TimepointKind.Start:
                    return TimepointKind.START;
                case Timepoint.Types.TimepointKind.End:
                    return TimepointKind.END;
            }
        }

        public static Timepoint.Types.TimepointKind GetTimepointKind(TimepointKind timepointKind)
        {
            switch (timepointKind)
            {
                default:
                case TimepointKind.GLOBAL_START:
                    return Timepoint.Types.TimepointKind.GlobalStart;
                case TimepointKind.GLOBAL_END:
                    return Timepoint.Types.TimepointKind.GlobalEnd;
                case TimepointKind.START:
                    return Timepoint.Types.TimepointKind.Start;
                case TimepointKind.END:
                    return Timepoint.Types.TimepointKind.End;
            }
        }
    }

    public class TimePointModelFactory : IGeTModelFactory<GeTTimePoint, Timepoint>
    {
        public GeTTimePoint FromProto(Timepoint proto)
        {
            var containerID = proto.ContainerId != string.Empty ? proto.ContainerId : null;
            return new GeTTimePoint(GeTTimePoint.GetTimepointKind(proto.Kind), containerID);
        }

        public Timepoint ToProto(GeTTimePoint timePoint)
        {
            
            return new Timepoint
            {
                Kind = GeTTimePoint.GetTimepointKind(timePoint.type),
                ContainerId = timePoint.containerID ?? string.Empty
            };
        }
    }
}
