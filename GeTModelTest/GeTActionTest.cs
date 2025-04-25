// using System.Collections.Generic;
// using GeTPlanModel;
// using Proto;
// using Xunit;

// namespace GeTModelTest
// {
//     public class GeTActionTest
//     {
//         [Fact]
//         public void Constructor_WithValidParameters_ShouldInitializeProperties()
//         {
//             // Arrange
//             string actionName = "move";
//             var parameters = new List<GeTParameter>
//             {
//                 new GeTParameter("robot", "object"),
//                 new GeTParameter("from", "location"),
//                 new GeTParameter("to", "location")
//             };

//             var condition0 = ConditionModelFactory.Create("at", [parameters[0], parameters[1]]);
//             var condition1 = ConditionModelFactory.Create("clear", [parameters[2]]);

//             var conditions = new List<GeTCondition> { condition0, condition1 };


//             var effect0 = EffectModelFactory.Create("at", [parameters[0],parameters[2]], true);
//             var effect1 = EffectModelFactory.Create("at", [parameters[0], parameters[1]], false);
//             var effect2 = EffectModelFactory.Create("clear", [parameters[1]], true);
//             var effects = new List<GeTEffect> { effect0, effect1, effect2 };

//             var action = new GeTAction(actionName, parameters, conditions, effects);

//             // Assert
//             Assert.Equal(actionName, action.actionName);
//             Assert.Same(parameters, action.parameters);
//             Assert.Same(conditions, action.conditions);
//             Assert.Same(effects, action.effects);
//             Assert.Null(action.duration);
//         }

//         [Fact]
//         public void Constructor_WithDuration_ShouldInitializeDuration()
//         {
//             // Arrange
//             string actionName = "move";
//             var parameters = new List<GeTParameter> { new GeTParameter("robot", "object") };
//             var conditions = new List<GeTCondition>();
//             var effects = new List<GeTEffect>();
//             var duration = new GeTDuration(new GeTExpression(10));

//             // Act
//             var action = new GeTAction(actionName, parameters, conditions, effects, duration);

//             // Assert
//             Assert.Equal(actionName, action.actionName);
//             Assert.Equal(parameters, action.parameters);
//             Assert.Equal(conditions, action.conditions);
//             Assert.Equal(effects, action.effects);
//             Assert.Equal(duration, action.duration);
//         }

//         [Fact]
//         public void ToString_ShouldReturnFormattedString()
//         {
//             // Arrange
//             string actionName = "move";
//             var parameters = new List<GeTParameter>
//             {
//                 new GeTParameter("robot", "object"),
//                 new GeTParameter("from", "location"),
//                 new GeTParameter("to", "location")
//             };
//             var conditions = new List<GeTCondition>
//             {
//                 new GeTCondition("at", new List<string> { "robot", "from" }, GeTTimeSpecifier.Start)
//             };
//             var effects = new List<GeTEffect>
//             {
//                 new GeTEffect("at", new List<string> { "robot", "to" }, true, GeTTimeSpecifier.End),
//                 new GeTEffect("at", new List<string> { "robot", "from" }, false, GeTTimeSpecifier.End)
//             };
//             var duration = new GeTDuration(new GeTExpression(10));
//             var action = new GeTAction(actionName, parameters, conditions, effects, duration);

//             // Act
//             string result = action.ToString();

//             // Assert
//             Assert.Contains("move", result);
//             Assert.Contains("CONDITIONS", result);
//             Assert.Contains("EFFECTS", result);
//             Assert.Contains("Duration", result);
//         }
//     }

//     public class ActionModelFactoryTest
//     {
//         [Fact]
//         public void FromProto_WithValidProto_ShouldReturnGeTAction()
//         {
//             // Arrange
//             var factory = new ActionModelFactory();
//             var protoAction = new Proto.Action
//             {
//                 Name = "pickup"
//             };

//             var protoParam = new Parameter { Name = "obj", Type = "object" };
//             protoAction.Parameters.Add(protoParam);

//             var protoCondition = new Condition
//             {
//                 FluentName = "clear",
//                 TimeSpecifier = "start"
//             };
//             protoCondition.Parameters.Add("obj");
//             protoAction.Conditions.Add(protoCondition);

//             var protoEffect = new Effect
//             {
//                 FluentName = "holding",
//                 IsPositive = true,
//                 TimeSpecifier = "end"
//             };
//             protoEffect.Parameters.Add("obj");
//             protoAction.Effects.Add(protoEffect);

//             // Act
//             var result = factory.FromProto(protoAction);

//             // Assert
//             Assert.Equal("pickup", result.actionName);
//             Assert.Single(result.parameters);
//             Assert.Equal("obj", result.parameters[0].Name);
//             Assert.Equal("object", result.parameters[0].Type);
//             Assert.Single(result.conditions);
//             Assert.Equal("clear", result.conditions[0].FluentName);
//             Assert.Single(result.effects);
//             Assert.Equal("holding", result.effects[0].FluentName);
//             Assert.True(result.effects[0].IsPositive);
//         }

//         [Fact]
//         public void FromProto_WithDuration_ShouldIncludeDuration()
//         {
//             // Arrange
//             var factory = new ActionModelFactory();
//             var protoAction = new Proto.Action
//             {
//                 Name = "move",
//                 Duration = new Duration
//                 {
//                     Value = new Expression
//                     {
//                         Atom = new Atom
//                         {
//                             Number = 5
//                         }
//                     }
//                 }
//             };

//             // Act
//             var result = factory.FromProto(protoAction);

//             // Assert
//             Assert.Equal("move", result.actionName);
//             Assert.NotNull(result.duration);
//             // Assert duration value (depends on how GeTExpression equality is defined)
//         }

//         [Fact]
//         public void ToProto_WithValidGeTAction_ShouldReturnProtoAction()
//         {
//             // Arrange
//             var factory = new ActionModelFactory();
//             var parameters = new List<GeTParameter>
//             {
//                 new GeTParameter("obj", "object")
//             };
//             var conditions = new List<GeTCondition>
//             {
//                 new GeTCondition("clear", new List<string> { "obj" }, GeTTimeSpecifier.Start)
//             };
//             var effects = new List<GeTEffect>
//             {
//                 new GeTEffect("holding", new List<string> { "obj" }, true, GeTTimeSpecifier.End)
//             };
//             var action = new GeTAction("pickup", parameters, conditions, effects);

//             // Act
//             var result = factory.ToProto(action);

//             // Assert
//             Assert.Equal("pickup", result.Name);
//             Assert.Single(result.Parameters);
//             Assert.Equal("obj", result.Parameters[0].Name);
//             Assert.Equal("object", result.Parameters[0].Type);
//             Assert.Single(result.Conditions);
//             Assert.Equal("clear", result.Conditions[0].FluentName);
//             Assert.Single(result.Effects);
//             Assert.Equal("holding", result.Effects[0].FluentName);
//             Assert.True(result.Effects[0].IsPositive);
//         }

//         [Fact]
//         public void ToProto_WithDuration_ShouldIncludeDuration()
//         {
//             // Arrange
//             var factory = new ActionModelFactory();
//             var duration = new GeTDuration(new GeTExpression(5));
//             var action = new GeTAction("move", new List<GeTParameter>(),
//                 new List<GeTCondition>(), new List<GeTEffect>(), duration);

//             // Act
//             var result = factory.ToProto(action);

//             // Assert
//             Assert.Equal("move", result.Name);
//             Assert.NotNull(result.Duration);
//             // Assert duration value (depends on how proto conversion works)
//         }

//         [Fact]
//         public void RoundTrip_FromProtoToProto_ShouldPreserveData()
//         {
//             // Arrange
//             var factory = new ActionModelFactory();
//             var originalProto = new Proto.Action
//             {
//                 Name = "drive"
//             };

//             var protoParam = new Parameter { Name = "vehicle", Type = "car" };
//             originalProto.Parameters.Add(protoParam);

//             // Act
//             var geTAction = factory.FromProto(originalProto);
//             var resultProto = factory.ToProto(geTAction);

//             // Assert
//             Assert.Equal(originalProto.Name, resultProto.Name);
//             Assert.Equal(originalProto.Parameters[0].Name, resultProto.Parameters[0].Name);
//             Assert.Equal(originalProto.Parameters[0].Type, resultProto.Parameters[0].Type);
//         }
//     }
// }