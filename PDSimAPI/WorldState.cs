using GeTPlanModel;
using Proto;
using System;
using System.Collections.Generic;
using PDSimAPI.Utils;
using System.Data;


namespace PDSimAPI
{
    public struct WorldStateChange
    {
        public GeTEffect AppliedEffect { get; set; }
        public GeTActionInstance AppliedAction { get; set; }
        public GeTStateVariable OldStateVar { get; set; }
        public GeTStateVariable NewStateVar { get; set; }
        public WorldStateChange(GeTEffect effect, GeTActionInstance action, GeTStateVariable oldStateVar, GeTStateVariable newStateVar)
        {
            AppliedEffect = effect;
            AppliedAction = action;
            OldStateVar = oldStateVar;
            NewStateVar = newStateVar;
        }

        public override string ToString()
        {
            return $"Applied Effect: {AppliedEffect}\nApplied Action: {AppliedAction}\nOld State Variable: {OldStateVar}\nNew State Variable: {NewStateVar}";
        }
    }
    public class WorldState
    {

        public HashSet<GeTStateVariable> State { get; set; }

        public WorldState()
        {
            State = new HashSet<GeTStateVariable>();
        }

        public WorldState(List<GeTStateVariable> state)
        {
            State = new HashSet<GeTStateVariable>(state);
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();

            foreach (var stateVariable in State)
            {
                sb.AppendLine(stateVariable.ToString());
            }
            return sb.ToString();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            WorldState other = (WorldState)obj;
            return State.SetEquals(other.State);
        }

        public static bool operator ==(WorldState left, WorldState right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WorldState left, WorldState right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return State.GetHashCode();
        }

        public int Length()
        {
            return State.Count;
        }

        public void Add(GeTStateVariable stateVariable)
        {
            State.Add(stateVariable);
        }
        public WorldState Update(GeTStateVariable stateVariable)
        {
            // Remove the old state variable
            State.RemoveWhere(sv => sv.Fluent.Equals(stateVariable.Fluent));
            // Add the new state variable
            State.Add(stateVariable);

            return this;
        }

        public HashSet<GeTStateVariable> Difference(WorldState other)
        {
            var difference = new HashSet<GeTStateVariable>(State);
            difference.ExceptWith(other.State);
            return difference;
        }

        public bool Contains(GeTStateVariable stateVariable)
        {
            return State.Contains(stateVariable);
        }

        public GeTAtom Query(GeTExpression expression)
        {
            // Find the state variable that matches the expression
            foreach (var stateVariable in State)
            {
                if (stateVariable.Fluent.Equals(expression))
                {
                    return stateVariable.Value.Atom;
                }
            }
            return AtomModelFactory.NONE();
        }

        /// <summary>
        /// Use the action parameters to evaluate the effect of an action
        /// The effect contains the mapping of parameters to the action instance
        /// 
        /// - If it's an assignment effect, the fluent is updated with the new value
        /// - If it's a function effect, the fluent is updated with the function value
        /// 
        /// </summary>
        /// <param name="effect">The effect to evaluate</param>
        /// <param name="actionInstance">The plan action to execute</param>

        public WorldStateChange Apply(GeTEffect effect, GeTAction actionDefinition, GeTActionInstance actionInstance)
        {
            var fluent = GroundExpressionFromEffect(effect, actionInstance);
            var newValue = effect.EffectExpression.Value;

            // Case grounded gives NONE when compiling ADL just used the grounded version or planner not considering defaults
            if (Query(fluent).Equals(AtomModelFactory.NONE()))
            {
                var fluentChange = effect.EffectExpression.Fluent;
                var oldValue = new GeTExpression(Query(fluentChange), ExpressionKind.Constant);

                var newStateVariable = new GeTStateVariable(fluent, newValue);
                Update(newStateVariable);

                return new WorldStateChange(effect, actionInstance, new GeTStateVariable(fluentChange, oldValue), newStateVariable);
            }
            var oldStateVariable = new GeTStateVariable(fluent, new GeTExpression(Query(fluent), ExpressionKind.Constant));

            if (newValue.Kind == ExpressionKind.FunctionApplication)
            {
                var expValue = new List<string>();
                foreach (var exp in newValue.SubExpressions)
                {
                    expValue.Add(exp.ToString());
                }
                var t = VisitFunctionApplication(newValue, actionDefinition, actionInstance);
                var result = PrefixToInfixConverter.Convert(t[0]);
                var resultValue = Convert.ToDouble(new DataTable().Compute(result, null));
                var currentValue = Query(fluent).GetValue();


                if (effect.EffectExpression.Kind == EffectExpressionKind.INCREMENT)
                    resultValue = Convert.ToDouble(currentValue) + resultValue;

                else if (effect.EffectExpression.Kind == EffectExpressionKind.DECREMENT)
                    resultValue = Convert.ToDouble(currentValue) - resultValue;

                var resultValueAtom = AtomModelFactory.Create(resultValue);
                var resultValueExpression = new GeTExpression(resultValueAtom, ExpressionKind.Constant);
                var stateVariable = new GeTStateVariable(fluent, resultValueExpression);
                Update(stateVariable);

                return new WorldStateChange(effect, actionInstance, oldStateVariable, stateVariable);
            }
            else if (newValue.Kind == ExpressionKind.StateVariable)
            {
                var paramList = new List<string>();
                foreach (var exp in newValue.SubExpressions)
                {
                    // infer the value of the action instance parameter checking the index of the parameter in the action model
                    var parameter = actionDefinition.parameters.FindIndex(p => p.Name == exp.Parameter.Name);
                    paramList.Add(actionInstance.parameters[parameter].Symbol);
                }
                var f = ExpressionModelFactory.CreateGroundFluent(newValue.FluentName, paramList);
                var value = Query(f);
                newValue = new GeTExpression(value, ExpressionKind.Constant);

                var stateVariable = new GeTStateVariable(fluent, newValue);
                Update(stateVariable);

                return new WorldStateChange(effect, actionInstance, oldStateVariable, stateVariable);
            }
            else // Constant
            {

                // Check if assignment or increment/decrement
                if (effect.EffectExpression.Kind == EffectExpressionKind.ASSIGNMENT)
                {
                    var stateVariable = new GeTStateVariable(fluent, newValue);
                    Update(stateVariable);
                    return new WorldStateChange(effect, actionInstance, oldStateVariable, stateVariable);
                }
                else
                {
                    var currentValue = Query(fluent).GetValue();
                    var resultValue = 0.0;
                    if (effect.EffectExpression.Kind == EffectExpressionKind.INCREMENT)
                        resultValue = Convert.ToDouble(currentValue) + Convert.ToDouble(newValue.Atom.GetValue());
                    else if (effect.EffectExpression.Kind == EffectExpressionKind.DECREMENT)
                        resultValue = Convert.ToDouble(currentValue) - Convert.ToDouble(newValue.Atom.GetValue());
                    var resultValueAtom = AtomModelFactory.Create(resultValue);
                    var resultValueExpression = new GeTExpression(resultValueAtom, ExpressionKind.Constant);

                    var stateVariable = new GeTStateVariable(fluent, resultValueExpression);
                    Update(stateVariable);
                    return new WorldStateChange(effect, actionInstance, oldStateVariable, stateVariable);
                }
            }

        }

        private GeTExpression GroundExpressionFromEffect(GeTEffect effect, GeTActionInstance actionInstance)
        {
            // Create an expression with the action parameters
            var parametersList = new List<string>();
            foreach (var parameter in effect.ParametersMap)
            {

                parametersList.Add(actionInstance.parameters[parameter].ToString());
            }

            var fluent = ExpressionModelFactory.CreateGroundFluent(effect.EffectExpression.Fluent.FluentName, parametersList);
            return fluent;
        }

        private List<string> VisitFunctionApplication(GeTExpression expression, GeTAction actionModel, GeTActionInstance actionInstance)
        {
            List<string> result = new List<string>();

            // If expression is null, return empty list
            if (expression == null)
                return result;

            // Handle function application by evaluating each subexpression
            if (expression.FluentName != null)
            {
                string functionName = FunctorMap(expression.FluentName);
                List<string> arguments = new List<string>();

                // Recursively evaluate each subexpression
                foreach (var subExpr in expression.SubExpressions)
                {
                    if (subExpr.Kind == ExpressionKind.FunctionApplication)
                    {
                        // Recursively handle nested function applications
                        var nestedResults = VisitFunctionApplication(subExpr, actionModel, actionInstance);
                        arguments.AddRange(nestedResults);
                    }
                    else if (subExpr.Atom != null)
                    {
                        // Handle atom expressions
                        arguments.Add(subExpr.Atom.ToString());
                    }
                    else if (subExpr.FluentName != null)
                    {
                        // Create fluent to query with action model and action instance
                        var paramList = new List<string>();
                        foreach (var exp in subExpr.SubExpressions)
                        {
                            // infer the value of the action instance parameter checking the index of the parameter in the action model
                            var parameter = actionModel.parameters.FindIndex(p => p.Name == exp.Parameter.Name);
                            paramList.Add(actionInstance.parameters[parameter].Symbol);
                        }
                        var fluent = ExpressionModelFactory.CreateGroundFluent(subExpr.FluentName, paramList);
                        var value = Query(fluent);
                        arguments.Add(value.ToString());
                    }
                }

                // Create the function expression
                string functionExpression = $"{functionName}({string.Join(", ", arguments)})";
                result.Add(functionExpression);
            }
            return result;
        }

        private string FunctorMap(string functionExpression)
        {
            switch (functionExpression)
            {
                case "up:plus":
                    return "+";
                case "up:minus":
                    return "-";
                case "up:times":
                    return "*";
                case "up:div":
                    return "/";
                default:
                    return functionExpression;
            }
        }



        public void Empty()
        {
            State.Clear();
        }
    }

}
