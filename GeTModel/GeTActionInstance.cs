using System.Collections.Generic;
using Proto;
namespace GeTPlanModel
{

    public class GeTActionInstance
    {
        public string Id { get; set; }
        public string ActionName { get; set; }
        public List<GeTAtom> parameters { get; set; }
        public GeTReal? StartTime { get; set; }
        public GeTReal? EndTime { get; set; }

        public GeTActionInstance(string id,
                                string actionName,
                                List<GeTAtom> parameters,
                                GeTReal? startTime = null,
                                GeTReal? endTime = null)
        {
            this.Id = id;
            this.ActionName = actionName;
            this.parameters = parameters;
            this.StartTime = startTime;
            this.EndTime = endTime;
        }

        public override string ToString()
        {
            var timeString = string.Empty;
            if (StartTime != null)
            {
                timeString += $"From: {StartTime} To: {EndTime}";
                return $"{timeString} {ActionName}({string.Join(", ", parameters)})";
            }
            return $"{ActionName}({string.Join(", ", parameters)})";
        }
    }

    public class ActionInstanceModelFactory : IGeTModelFactory<GeTActionInstance, ActionInstance> , IProtoFactory<GeTActionInstance, ActionInstance>
    {
        public GeTActionInstance FromProto(ActionInstance actionInstance)
        {
            var amf = new AtomModelFactory();
            var rmf = new RealModelFactory();

            var id = actionInstance.Id;
            var actionName = actionInstance.ActionName;
            var parameters = new List<GeTAtom>();
            foreach (var parameter in actionInstance.Parameters)
            {
                var p = amf.FromProto(parameter);
                parameters.Add(p);
            }
            if (actionInstance.StartTime == null)
                return new GeTActionInstance(id, actionName, parameters);

            var startTime = rmf.FromProto(actionInstance.StartTime);
            var endTime = rmf.FromProto(actionInstance.EndTime);

            return new GeTActionInstance(id, actionName, parameters, startTime, endTime);
        }

        public ActionInstance ToProto(GeTActionInstance model)
        {
            var amf = new AtomModelFactory();
            var rmf = new RealModelFactory();

            var actionInstance = new ActionInstance
            {
                Id = model.Id,
                ActionName = model.ActionName,
                Parameters = { },
                StartTime = null,
                EndTime = null
            };
            foreach (var parameter in model.parameters)
            {
                var p = amf.ToProto(parameter);
                actionInstance.Parameters.Add(p);
            }
            if (model.StartTime != null)
            {
                actionInstance.StartTime = rmf.ToProto(model.StartTime);
                actionInstance.EndTime = rmf.ToProto(model.EndTime);
            }
            return actionInstance;
        }
    }
}
