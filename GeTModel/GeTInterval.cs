using Proto;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeTPlanModel
{
    [System.Serializable]
    public class GeTInterval
    {
        public bool LeftOpen { get; set; }
        public bool RightOpen { get; set; }
        public GeTExpression LowerBound { get; set; }
        public GeTExpressionString LowerBoundString { get; set; }
        public GeTExpression UpperBound { get; set; }
        public GeTExpressionString UpperBoundString { get; set; }


        public GeTInterval(bool leftOpen, bool rightOpen, GeTExpression lowerBound, GeTExpression upperBound)
        {
            this.LeftOpen = leftOpen;
            this.RightOpen = rightOpen;
            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;

            LowerBoundString = new GeTExpressionString(lowerBound);
            UpperBoundString = new GeTExpressionString(upperBound);
        }

        public override string ToString()
        {
            return $"{(LeftOpen ? "(" : "[")}{LowerBound}, {UpperBound}{(RightOpen ? ")" : "]")}";
        }

    }

    public class IntervalModelFactory : IGeTModelFactory<GeTInterval, Interval>, IProtoFactory<GeTInterval, Interval>
    {
        public GeTInterval FromProto(Interval proto)
        {
            var emf = new ExpressionModelFactory();
            var leftOpen = proto.IsLeftOpen;
            var rightOpen = proto.IsRightOpen;
            var lowerBound = emf.FromProto(proto.Lower);
            var upperBound = emf.FromProto(proto.Upper);
            return new GeTInterval(leftOpen, rightOpen, lowerBound, upperBound);
        }
        public Interval ToProto(GeTInterval model)
        {
            var proto = new Interval();
            var emf = new ExpressionModelFactory();
            proto.IsLeftOpen = model.LeftOpen;
            proto.IsRightOpen = model.RightOpen;
            proto.Lower = emf.ToProto(model.LowerBound);
            proto.Upper = emf.ToProto(model.UpperBound);
            return proto;
        }
    }
}
