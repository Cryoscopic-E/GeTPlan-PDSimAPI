using System.Collections.Generic;
using Proto;
namespace GeTPlanModel
{
    [System.Serializable]
    public class GeTPlan
    {
        //  For now supporting only seq plan no hierachy nor schedule

        public List<GeTActionInstance> Actions { get; set; }
        public bool IsTemporal { get; set; }
        public GeTPlan(List<GeTActionInstance> actions, bool temporal)
        {
            this.Actions = actions;
            this.IsTemporal = temporal;
        }

        public override string ToString()
        {
            var planString = string.Empty;
            foreach (var action in Actions)
            {
                planString += $"{action}\n";
            }
            return planString;
        }
    }

    public class PlanModelFactory : IGeTModelFactory<GeTPlan, Plan>, IProtoFactory<GeTPlan, Plan>
    {
        public GeTPlan FromProto(Plan plan)
        {
            var amf = new ActionInstanceModelFactory();
            var actions = new List<GeTActionInstance>();
            var temporal = plan.Actions[0].StartTime != null;
            foreach (var action in plan.Actions)
            {
                var a = amf.FromProto(action);
                actions.Add(a);
            }
            return new GeTPlan(actions, temporal);
        }

        public Plan ToProto(GeTPlan plan)
        {
            var actions = new List<ActionInstance>();
            foreach (var action in plan.Actions)
            {
                var amf = new ActionInstanceModelFactory();
                var a = amf.ToProto(action);
                actions.Add(a);
            }
            return new Plan { Actions = { actions } };
        }
    }
}
