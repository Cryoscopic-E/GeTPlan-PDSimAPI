using GeTPlanModel;
using Proto;
using System;
using System.Collections.Generic;

namespace PDSimAPI
{
    public class Visualisation
    {
        public static readonly GeTAtom NULL = AtomModelFactory.NONE();
        public static readonly GeTAtom TRUE = AtomModelFactory.TRUE();
        public static readonly GeTAtom FALSE = AtomModelFactory.FALSE();

        // Visualisation Components
        public VisualisationState VisState { get; private set; }
        public GeTPlanGenResult PlanGeneration { get; private set; }
        public Dictionary<string, GeTObjectDeclaration> Objects { get; private set; }
        public Dictionary<string, List<GeTObjectDeclaration>> TypeToObjects { get; private set; }
        public Dictionary<string, GeTAction> ActionsDefinitions { get; private set; }
        public Dictionary<string, GeTFluent> FluentsDefinitions { get; private set; }
        public Dictionary<string, List<GeTEffect>> ActionsEffects { get; private set; }
        public WorldState CurrentWorldState { get; private set; }
        public TimeLine TimeLine { get; private set; }

        // Visualisation Events

        public event EventHandler<WorldStateChange> WorldStateChanged;
        public event EventHandler<GeTActionInstance> ActionStarted;
        public event EventHandler<GeTActionInstance> ActionCompleted;
        public event EventHandler<TimelineEventArgs> TimeLineAdvance;
        public event EventHandler VisualisationStart;
        public event EventHandler VisualisationEnd;
        public event EventHandler VisualisationPaused;



        public Visualisation(byte[] protoProblem, byte[] protoPlan)
        {
            VisState = new VisualisationState();
            Objects = new Dictionary<string, GeTObjectDeclaration>();
            TypeToObjects = new Dictionary<string, List<GeTObjectDeclaration>>();
            ActionsDefinitions = new Dictionary<string, GeTAction>();
            FluentsDefinitions = new Dictionary<string, GeTFluent>();
            ActionsEffects = new Dictionary<string, List<GeTEffect>>();

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

                foreach (var effect in actionModel.effects)
                {
                    if (!ActionsEffects.ContainsKey(actionModel.actionName))
                    {
                        ActionsEffects.Add(actionModel.actionName, new List<GeTEffect>());
                    }
                    ActionsEffects[actionModel.actionName].Add(effect);
                }
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

            TimeLine = new TimeLine(PlanGeneration.Plan.Actions, PlanGeneration.Plan.IsTemporal, ActionsEffects);
            TimeLine.ActionStarted += OnActionStarted;
            TimeLine.ActionCompleted += OnActionCompleted;
            TimeLine.EffectOccurred += OnEffectOccured;
            TimeLine.TimelineAdvanced += OnTimelineAdvanced;
        }

        // TimeLine Listeners

        private void OnActionStarted(object sender, ActionEventArgs args)
        {
            //// Add action to current actions
            //VisState.CurrentActions.Add(args.Action);
            //// Apply effects
            //var actionDefinition = ActionsDefinitions[args.Action.ActionName];
            //var actionInstance = args.Action;

            //foreach (var effect in actionDefinition.effects)
            //{
            //    var stateChange = CurrentWorldState.Apply(effect, actionDefinition, args.Action);
            //    WorldStateChanged?.Invoke(this, stateChange); // Maybe move this to action completed? need to think about this
            //}
            ActionStarted?.Invoke(this, args.Action);
        }

        private void OnEffectOccured(object sender, EffectEventArgs args)
        {
            var actionDefinition = ActionsDefinitions[args.Action.ActionName];
            var stateChange = CurrentWorldState.Apply(args.Effect, actionDefinition, args.Action);
            WorldStateChanged?.Invoke(this, stateChange);
        }

        private void OnActionCompleted(object sender, ActionEventArgs args)
        {
            ActionCompleted?.Invoke(this, args.Action);
        }

        private void OnTimelineAdvanced(object sender, TimelineEventArgs args)
        {
            TimeLineAdvance?.Invoke(this, args);
        }


        public void Advance()
        {
            // If visualisation has ended, do nothing
            if (VisState.IsEnded)
            {
                return;
            }

            // If Visualisation has not started, start it
            if (!VisState.HasStarted)
            {
                VisState.HasStarted = true;
                VisState.IsRunning = true;
                VisualisationStart?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // Id Visualisation is paused, skip
                if (VisState.IsPaused)
                {
                    return;
                }
                // If Visualisation is running, advance the timeline
                if (VisState.IsRunning)
                {
                    if (!TimeLine.MoveNext())
                    {
                        VisState.IsEnded = true;
                        VisState.IsRunning = false;
                        VisualisationEnd?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        public void Pause()
        {
            VisState.IsPaused = true;
            VisualisationPaused?.Invoke(this, EventArgs.Empty);
        }

        public void Resume()
        {
            VisState.IsPaused = false;
        }

        public void Reset()
        {
            VisState.HasStarted = false;
            VisState.IsRunning = false;
            VisState.IsPaused = false;
            VisState.IsEnded = false;
            VisState.CurrentActions.Clear();
            VisState.CurrentEffects.Clear();
            TimeLine.Reset();
        }

        public class VisualisationState
        {
            public bool HasStarted { get; set; }
            public bool IsRunning { get; set; }
            public bool IsPaused { get; set; }
            public bool IsEnded { get; set; }

            public HashSet<GeTActionInstance> CurrentActions { get; set; }
            public List<GeTStateVariable> CurrentEffects { get; set; }

            public VisualisationState()
            {
                HasStarted = false;
                IsRunning = false;
                IsPaused = false;
                IsEnded = false;
                CurrentActions = new HashSet<GeTActionInstance>();
                CurrentEffects = new List<GeTStateVariable>();
            }
        }
    }
}

