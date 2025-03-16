using GeTPlanModel;
using Proto;

namespace GeTModelTests
{
    public class GeTExpressionTests
    {
        [Fact]
        public void VariableExpression_Equality_Test()
        {
            // Assuming ExpressionKind.Variable exists
            ExpressionKind kind = ExpressionKind.Variable;
            string variableName = "x";
            var expr1 = new GeTExpression(variableName, kind, true);
            var expr2 = new GeTExpression(variableName, kind, true);

            Assert.Equal(expr1, expr2);
            Assert.Equal(expr1.GetHashCode(), expr2.GetHashCode());
        }

        [Fact]
        public void VariableExpression_Inequality_Test()
        {
            ExpressionKind kind = ExpressionKind.Variable;
            var expr1 = new GeTExpression("x", kind, true);
            var expr2 = new GeTExpression("y", kind, true);

            Assert.NotEqual(expr1, expr2);
        }

        [Fact]
        public void FluentExpression_WithSubExpressions_Equality_Test()
        {
            // Assuming ExpressionKind.Fluent exists
            ExpressionKind fluentKind = ExpressionKind.FluentSymbol;
            ExpressionKind varKind = ExpressionKind.Variable;

            // Create a subexpression variable
            var subExpr = new GeTExpression("a", varKind, true);
            var subExpressions = new List<GeTExpression> { subExpr };

            string fluentName = "F";
            var expr1 = new GeTExpression(fluentName, subExpressions, fluentKind);
            var expr2 = new GeTExpression(fluentName, new List<GeTExpression> { new GeTExpression("a", varKind, true) }, fluentKind);

            Assert.Equal(expr1, expr2);
            Assert.Equal(expr1.GetHashCode(), expr2.GetHashCode());
        }

        [Fact]
        public void ToString_Returns_Valid_String_Test()
        {
            ExpressionKind kind = ExpressionKind.Variable;
            var expr = new GeTExpression("x", kind, true);

            string result = expr.ToString();
            Assert.False(string.IsNullOrEmpty(result));
        }
    }
}
