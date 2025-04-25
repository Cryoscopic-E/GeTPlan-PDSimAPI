using GeTPlanModel;
using Proto;
using ValueType = GeTPlanModel.ValueType;

namespace GeTModelTests
{
    public class GeTFluentsTests
    {
        [Fact]
        public void Constructor_InitializesPropertiesCorrectly()
        {
            // Arrange
            string name = "at";
            ValueType valueType = ValueType.Boolean;
            var defaultValue = ExpressionModelFactory.CreateValue(false);
            var parameters = new List<GeTParameter> {
                new GeTParameter("obj", "locatable"),
                new GeTParameter("loc", "waypoint")
            };

            var fluent = new GeTFluent(name, valueType, parameters, defaultValue);

            // Assert
            Assert.Equal(name, fluent.Name);
            Assert.Equal(valueType, fluent.FluentValueType);
            Assert.Equal(parameters, fluent.Parameters);
            Assert.Equal(defaultValue, fluent.DefaultValue);
            Assert.NotNull(fluent.DefaultString);
        }

        [Fact]
        public void Constructor_WithNullParameters_InitializesEmptyParametersList()
        {
            // Act
            var fluent = new GeTFluent("test", ValueType.Int, null);

            // Assert
            Assert.NotNull(fluent.Parameters);
            Assert.Empty(fluent.Parameters);
        }

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var param = new GeTParameter("x", "int");
            var parameters = new List<GeTParameter> { param };
            var defaultValue = new GeTExpression(); // Mock with a simple implementation
            var fluent = new GeTFluent("position", ValueType.Int, parameters, defaultValue);

            // Act
            string result = fluent.ToString();

            // Assert
            Assert.Contains("position", result);
            Assert.Contains(param.ToString(), result);
            Assert.Contains("Int", result);
            Assert.Contains(defaultValue.ToString(), result);
        }

        [Fact]
        public void ToString_WithNullDefaultValue_HandlesNullCorrectly()
        {
            // Arrange
            var fluent = new GeTFluent("test", ValueType.Boolean, new List<GeTParameter>());

            // Act
            string result = fluent.ToString();

            // Assert
            Assert.Contains("None", result); // Default value is "None" when null
        }
    }

    public class FluentModelFactoryTests
    {
        [Fact]
        public void FromProto_FluentNull_ReturnsNull()
        {
            // Arrange
            var factory = new FluentModelFactory();

            // Act
            var result = factory.FromProto((Fluent)null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromProto_ValidFluent_ReturnsCorrectGeTFluent()
        {
            // Arrange
            var factory = new FluentModelFactory();
            var fluent = new Fluent
            {
                Name = "position",
                ValueType = "up:integer",
                Parameters = { new Parameter { Name = "obj", Type = "object" } }
            };

            // Act
            var result = factory.FromProto(fluent);

            // Assert
            Assert.Equal("position", result.Name);
            Assert.Equal(ValueType.Int, result.FluentValueType);
            Assert.Single(result.Parameters);
            Assert.Equal("obj", result.Parameters[0].Name);
            Assert.Equal("object", result.Parameters[0].Type);
        }

        [Theory]
        [InlineData("up:integer", ValueType.Int)]
        [InlineData("up:real", ValueType.Real)]
        [InlineData("up:bool", ValueType.Boolean)]
        [InlineData("up:symbol", ValueType.Symbol)]
        [InlineData("unknown", ValueType.Symbol)]
        public void FromProto_Fluent_HandlesAllValueTypes(string protoType, ValueType expectedType)
        {
            // Arrange
            var factory = new FluentModelFactory();
            var fluent = new Fluent
            {
                Name = "test",
                ValueType = protoType
            };

            // Act
            var result = factory.FromProto(fluent);

            // Assert
            Assert.Equal(expectedType, result.FluentValueType);
        }

        [Fact]
        public void ToProto_GeTFluentNull_ReturnsNull()
        {
            // Arrange
            var factory = new FluentModelFactory();

            // Act
            var result = factory.ToProto(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ToProto_ValidGeTFluent_ReturnsCorrectProto()
        {
            // Arrange
            var factory = new FluentModelFactory();
            var parameters = new List<GeTParameter> { new GeTParameter("obj", "object") };
            var fluent = new GeTFluent("position", ValueType.Int, parameters);

            // Act
            var result = factory.ToProto(fluent);

            // Assert
            Assert.Equal("position", result.Name);
            Assert.Equal("Int", result.ValueType);
            Assert.Single(result.Parameters);
            Assert.Equal("obj", result.Parameters[0].Name);
            Assert.Equal("object", result.Parameters[0].Type);
        }

        [Fact]
        public void FromProto_ExpressionNull_ReturnsNull()
        {
            // Arrange
            var factory = new FluentModelFactory();

            // Act
            var result = factory.FromProto((Expression)null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromProto_ExpressionWithAtom_ReturnsCorrectGeTFluent()
        {
            // Arrange
            var factory = new FluentModelFactory();
            var expression = new Expression
            {
                Atom = new Atom { Symbol = "distance" }
            };

            // Act
            var result = factory.FromProto(expression);

            // Assert
            Assert.Equal("distance", result.Name);
            Assert.Equal(ValueType.Symbol, result.FluentValueType);
            Assert.Empty(result.Parameters);
        }

        [Fact]
        public void FromProto_ExpressionWithList_ReturnsCorrectGeTFluent()
        {
            // Arrange
            var factory = new FluentModelFactory();
            var expression = new Expression
            {
                List = {
                    new Expression { Atom = new Atom { Symbol = "distance" } },
                    new Expression { Atom = new Atom { Symbol = "loc1" } },
                    new Expression { Atom = new Atom { Symbol = "loc2" } }
                }
            };

            // Act
            var result = factory.FromProto(expression);

            // Assert
            Assert.Equal("distance", result.Name);
            Assert.Equal(ValueType.Symbol, result.FluentValueType);
            Assert.Equal(2, result.Parameters.Count);
            Assert.Equal("loc1", result.Parameters[0].Name);
            Assert.Equal("loc2", result.Parameters[1].Name);
        }
    }
}
