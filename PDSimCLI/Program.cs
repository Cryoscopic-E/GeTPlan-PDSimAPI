using PDSimAPI;
using GeTPlanModel;
using Proto;
using System.Diagnostics;
namespace PDSimCLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var request = new BackendTestConnectionRequest();
            //var response = request.Connect();


            var problem = new ProtobufRequest("problem");
            var problemResponse = problem.Connect();



            var plan = new ProtobufRequest("plan");
            var planResponse = plan.Connect();

            TestProblem(problemResponse, planResponse);

        }

        public static void TestProblem(byte[] problemResponse, byte[] planResponse)
        {
            var problem = Problem.Parser.ParseFrom(problemResponse);

            Console.WriteLine("### DEFINED PREDICATES ###\n");
            var fluents = new List<GeTFluent>();
            var fmf = new FluentModelFactory();
            foreach (var f in problem.Fluents)
            {
                var fluent = fmf.FromProto(f);
                fluents.Add(fluent);
                Console.WriteLine(fluent);
            }

            Console.WriteLine("\n\n### INITIAL STATE ###\n");

            var init = new List<GeTStateVariable>();
            var smf = new StateVariableFactory();
            foreach (var s in problem.InitialState)
            {
                var state = smf.FromProto(s);
                init.Add(state);
                //Console.WriteLine(state);
            }

            var worldState = new WorldState(init);

            //var query = ExpressionModelFactory.CreateGroundFluent("distance", new List<string> { "city0", "city0" });
            //Console.WriteLine(query);

            //var queryResult = worldState.Query(query);
            //Console.WriteLine(queryResult);

            Console.WriteLine("\n\n### ACTIONS ###\n");
            var actionDict = new Dictionary<string, GeTAction>();
            var actions = new List<GeTAction>();
            foreach (var a in problem.Actions)
            {
                var amf = new ActionModelFactory();
                var convertedAction = amf.FromProto(a);
                actionDict.Add(convertedAction.actionName, convertedAction);
                actions.Add(convertedAction);
            }


            Console.WriteLine("\n\n### PLAN ###\n");
            var planParser = PlanGenerationResult.Parser.ParseFrom(planResponse);
            var pgrmf = new PlanGenResultModelFactory();
            var genResult = pgrmf.FromProto(planParser);

            var plan = genResult.Plan.Actions;
            var timed = genResult.Plan.IsTemporal;

            var timeLine = new TimeLine(plan, timed);

            timeLine.ActionStarted += OnActionStart;
            timeLine.ActionCompleted += OnActionComplete;
            timeLine.TimelineAdvanced += OnTimelineAdvanced;


            while (timeLine.MoveNext())
            {
                Debug.WriteLine("Timeline advanced");
            }
            ////check if the plan is temporal
            //if (genResult.Plan.IsTemporal)
            //{
            //    Console.WriteLine("Temporal Plan");
            //    foreach (var actionInstance in planParser.Plan.Actions)
            //    {

            //    }
            //}
            //else
            //{
            //    foreach (var actionInstance in genResult.Plan.Actions)
            //    {
            //        var actionModel = actionDict[actionInstance.ActionName];

            //        foreach (var effect in actionModel.effects)
            //        {
            //            var worldChange = worldState.Apply(effect, actionModel, actionInstance);
            //            Console.WriteLine(worldChange);
            //        }
            //    }
            //}

        }

        // Timeline events
        static void OnActionStart(object sender, ActionEventArgs args)
        {
            Console.WriteLine($"Action started: {args.Action}");
        }

        static void OnActionComplete(object sender, ActionEventArgs args)
        {
            Console.WriteLine($"Action completed: {args.Action}");
        }

        static void OnTimelineAdvanced(object sender, TimelineEventArgs args)
        {
            Console.WriteLine($"Timeline advanced to: {args.Time}");
        }
    }
}