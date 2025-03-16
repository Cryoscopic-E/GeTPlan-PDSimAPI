using Proto;
using System.Collections.Generic;
namespace GeTPlanModel
{

    public enum PlanGenStatus
    {
        SolvedSatisficing,
        SolvedOptimally,
        UnsolvableProven,
        UnsolvableIncompletely,
        Timeout,
        Memout,
        InternalError,
        UnsupportedProblem,
        Intermediate
    }

    public class GeTPlanGenResult
    {
        public PlanGenStatus PlanGenStatus { get; set; }
        public string Engine { get; set; }
        public List<string> Metrics { get; set; }
        public List<GeTLogMessage> Logs { get; set; }
        public GeTPlan Plan { get; set; }

        public GeTPlanGenResult (PlanGenStatus planGenStatus, string engine, List<string> metrics, List<GeTLogMessage> logs, GeTPlan plan)
        {
            this.PlanGenStatus = planGenStatus;
            this.Engine = engine;
            this.Metrics = metrics;
            this.Logs = logs;
            this.Plan = plan;
        }

        public static PlanGenStatus GetPlanGenStatus(PlanGenerationResult.Types.Status status)
        {
            switch (status)
            {
                case PlanGenerationResult.Types.Status.SolvedSatisficing:
                    return PlanGenStatus.SolvedSatisficing;
                case PlanGenerationResult.Types.Status.SolvedOptimally:
                    return PlanGenStatus.SolvedOptimally;
                case PlanGenerationResult.Types.Status.UnsolvableProven:
                    return PlanGenStatus.UnsolvableProven;
                case PlanGenerationResult.Types.Status.UnsolvableIncompletely:
                    return PlanGenStatus.UnsolvableIncompletely;
                case PlanGenerationResult.Types.Status.Timeout:
                    return PlanGenStatus.Timeout;
                case PlanGenerationResult.Types.Status.Memout:
                    return PlanGenStatus.Memout;
                case PlanGenerationResult.Types.Status.InternalError:
                    return PlanGenStatus.InternalError;
                case PlanGenerationResult.Types.Status.UnsupportedProblem:
                    return PlanGenStatus.UnsupportedProblem;
                default:
                case PlanGenerationResult.Types.Status.Intermediate:
                    return PlanGenStatus.Intermediate;
            }
        }
    }

    public class PlanGenResultModelFactory : IGeTModelFactory<GeTPlanGenResult, PlanGenerationResult>
    {
        public GeTPlanGenResult FromProto(PlanGenerationResult planGenerationResult)
        {
            var logs = new List<GeTLogMessage>();
            var lmf = new LogMessageModelFactory();
            foreach (var log in planGenerationResult.LogMessages)
            {
                var l = lmf.FromProto(log);
                logs.Add(l);
            }
            var metrics = new List<string>();
            foreach (var metric in planGenerationResult.Metrics)
            {
                metrics.Add($"{metric.Key}: {metric.Value}");
            }
            var plan = new GeTPlan(new List<GeTActionInstance>());
            if (planGenerationResult.Plan != null)
            {
                var pmf = new PlanModelFactory();
                plan = pmf.FromProto(planGenerationResult.Plan);
            }
            return new GeTPlanGenResult(GeTPlanGenResult.GetPlanGenStatus(planGenerationResult.Status), 
                                        planGenerationResult.Engine.Name, 
                                        metrics, 
                                        logs, 
                                        plan);
        }
    }
}
