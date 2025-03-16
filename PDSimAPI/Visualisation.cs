using GeTPlanModel;
using Proto;
using System.Collections.Generic;

namespace PDSimAPI
{
    public class Visualisation
    {
        public static readonly GeTAtom NULL = AtomModelFactory.NONE();
        public static readonly GeTAtom TRUE = AtomModelFactory.TRUE();
        public static readonly GeTAtom FALSE = AtomModelFactory.FALSE();

        // Visualisation Components
        public VisualisationState State { get; private set; }
        public GeTPlanGenResult PlanGeneration { get; private set; }
        public Dictionary<string, GeTObjectDeclaration> Objects { get; private set; }
        public Dictionary<string, List<GeTObjectDeclaration>> TypeToObjects { get; private set; }
        public Dictionary<string, GeTAction> ActionsDefinitions { get; private set; }
        public Dictionary<string, GeTFluent> FluentsDefinitions { get; private set; }
        public WorldState CurrentWorldState { get; private set; }


        public Visualisation(byte[] protoProblem, byte[] protoPlan)
        {
            State = new VisualisationState();
            Objects = new Dictionary<string, GeTObjectDeclaration>();
            TypeToObjects = new Dictionary<string, List<GeTObjectDeclaration>>();
            ActionsDefinitions = new Dictionary<string, GeTAction>();
            FluentsDefinitions = new Dictionary<string, GeTFluent>();

            var problemParser = Problem.Parser.ParseFrom(protoProblem);
            var planParser = PlanGenerationResult.Parser.ParseFrom(protoPlan);

            // Load Objects
            var odmf = new ObjectDeclarationModelFactory();
            foreach (var obj in problemParser.Objects)
            {
                var objectDeclaration = odmf.FromProto(obj);
                Objects.Add(objectDeclaration.Name, objectDeclaration);
                if (!TypeToObjects.ContainsKey(objectDeclaration.Type))
                {
                    TypeToObjects.Add(objectDeclaration.Type, new List<GeTObjectDeclaration>());
                }
                TypeToObjects[objectDeclaration.Type].Add(objectDeclaration);
            }

            // Load Fluents
            var fmf = new FluentModelFactory();
            foreach (var fluent in problemParser.Fluents)
            {
                var fluentModel = fmf.FromProto(fluent);
                FluentsDefinitions.Add(fluentModel.Name, fluentModel);
            }

            // Load Actions
            var amf = new ActionModelFactory();
            foreach (var action in problemParser.Actions)
            {
                var actionModel = amf.FromProto(action);
                ActionsDefinitions.Add(actionModel.actionName, actionModel);
            }

            // Load Initial State
            var smf = new StateVariableFactory();
            var stateVariables = new List<GeTStateVariable>();
            foreach (var state in problemParser.InitialState)
            {
                var stateVariable = smf.FromProto(state);
                stateVariables.Add(stateVariable);
            }
            CurrentWorldState = new WorldState(stateVariables);

            // Load Plan Generation
            var pgrmf = new PlanGenResultModelFactory();
            PlanGeneration = pgrmf.FromProto(planParser);
        }


        public class VisualisationState
        {
            public bool IsRunning { get; set; }
            public bool IsPaused { get; set; }
            public GeTAction? CurrentAction { get; set; }
            public List<GeTStateVariable> CurrentEffects { get; set; }

            public VisualisationState()
            {
                IsRunning = false;
                IsPaused = false;
                CurrentAction = null;
                CurrentEffects = new List<GeTStateVariable>();
            }
        }
    }
}

