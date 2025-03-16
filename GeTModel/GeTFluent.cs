using System.Collections.Generic;
using Proto;
namespace GeTPlanModel
{
    public enum ValueType
    {
        Int,
        Real,
        Boolean,
        Symbol
    }

    public class GeTFluent
    {
        public string Name { get; private set; }
        public ValueType FluentValueType { get; private set; }  // Changed to use ValueType enum
        public List<GeTParameter> Parameters { get; private set; }
        public GeTExpression? DefaultValue { get; private set; }

        public GeTFluent(string name, ValueType valueType, List<GeTParameter> parameters, GeTExpression? defaultValue = null)
        {
            Name = name;
            FluentValueType = valueType;
            Parameters = parameters ?? new List<GeTParameter>();
            DefaultValue = defaultValue;
        }

        public override string ToString()
        {
            string paramStr = Parameters != null ? string.Join(", ", Parameters) : "None";
            string defaultValueStr = DefaultValue != null ? DefaultValue.ToString() : "None";
            return $"{Name} ({paramStr}) [{FluentValueType}] Default Value: {defaultValueStr}";
        }
    }

    public class FluentModelFactory : IGeTModelFactory<GeTFluent, Fluent>, IGeTModelFactory<GeTFluent, Expression>, IProtoFactory<GeTFluent, Fluent>
    {
        public GeTFluent FromProto(Fluent fluent)
        {
            if (fluent == null) return null;
            
            var emf = new ExpressionModelFactory();

            // Convert the value type based on the provided logic
            ValueType fluentValueType = ConvertValueType(fluent.ValueType);

            // Convert parameters
            var parameters = new List<GeTParameter>();
            foreach (var param in fluent.Parameters)
            {
                parameters.Add(new GeTParameter(param.Name, param.Type));
            }

            // Convert default value
            GeTExpression defaultValue = null;
            if (fluent.DefaultValue != null)
            {
                defaultValue = emf.FromProto(fluent.DefaultValue);
            }

            // Create and return the FluentModel
            return new GeTFluent(fluent.Name, fluentValueType, parameters, defaultValue);
        }

        public Fluent ToProto(GeTFluent fluent)
        {
            if (fluent == null) return null;

            var protoFluent = new Fluent
            {
                Name = fluent.Name,
                ValueType = fluent.FluentValueType.ToString(),
            };

            var emf = new ExpressionModelFactory();

            foreach (var param in fluent.Parameters)
            {
                protoFluent.Parameters.Add(new Parameter
                {
                    Name = param.Name,
                    Type = param.Type
                });
            }
            if (fluent.DefaultValue != null)
            {
                protoFluent.DefaultValue = emf.ToProto(fluent.DefaultValue);
            }
            return protoFluent;
        }
        public GeTFluent FromProto(Expression fluentExpression)
        {
            if (fluentExpression == null) return null;

            // Handle cases where the Expression is an atom (simple fluent)
            if (fluentExpression.Atom != null)
            {
                // Fluent is represented by an atomic symbol
                return new GeTFluent(fluentExpression.Atom.Symbol, ValueType.Symbol, new List<GeTParameter>());
            }

            // Handle cases where the Expression is a list (e.g., Fluent with parameters)
            if (fluentExpression.List != null && fluentExpression.List.Count > 0)
            {
                // First expression in the list is the fluent name
                var fluentNameExpr = fluentExpression.List[0];
                string fluentName = fluentNameExpr.Atom?.Symbol;

                // Remaining expressions are parameters
                var parameters = new List<GeTParameter>();
                for (int i = 1; i < fluentExpression.List.Count; i++)
                {
                    var paramExpr = fluentExpression.List[i];
                    if (paramExpr.Atom != null)
                    {
                        parameters.Add(new GeTParameter(paramExpr.Atom.Symbol, string.Empty));
                    }
                }

                return new GeTFluent(fluentName, ValueType.Symbol, parameters);
            }

            return null;
        }

        // Method to convert string valueType to enum ValueType
        private static ValueType ConvertValueType(string type)
        {
            if (type == "up:integer")
                return ValueType.Int;
            else if (type == "up:real")
                return ValueType.Real;
            else if (type == "up:bool")
                return ValueType.Boolean;
            else
                return ValueType.Symbol;
        }
    }
}
