using System;
using Xunit;
using GeTPlanModel;

namespace GeTModelTest
{
    public class GeTAtomTests
    {
        [Fact]
        public void Equals_WithSameValues_ReturnsTrue()
        {
            var atom1 = new GeTAtom(symbol: "H");
            var atom2 = new GeTAtom(symbol: "H");

            Assert.True(atom1.Equals(atom2));
        }

        [Fact]
        public void Equals_WithDifferentValues_ReturnsFalse()
        {
            var atom1 = new GeTAtom(symbol: "H");
            var atom2 = new GeTAtom(symbol: "He");

            Assert.False(atom1.Equals(atom2));
        }

        [Fact]
        public void ToString_WithSymbol_ReturnsSymbol()
        {
            var atom = new GeTAtom(symbol: "H");

            Assert.Equal("H", atom.ToString());
        }

        [Fact]
        public void ToString_WithIntValue_ReturnsIntValue()
        {
            var atom = new GeTAtom(intValue: 1);

            Assert.Equal("1", atom.ToString());
        }

        [Fact]
        public void ToString_WithRealValue_ReturnsRealValue()
        {
            var realValue = new GeTReal(1, 2);
            var atom = new GeTAtom(realValue: realValue);

            Assert.Equal("1/2", atom.ToString());
        }

        [Fact]
        public void ToString_WithBooleanValue_ReturnsBooleanValue()
        {
            var atom = new GeTAtom(booleanValue: true);

            Assert.Equal("True", atom.ToString());
        }

        [Fact]
        public void GetValue_WithIntValue_ReturnsIntValue()
        {
            var atom = new GeTAtom(intValue: 1);

            Assert.Equal(1L, atom.GetValue());
        }

        [Fact]
        public void GetValue_WithRealValue_ReturnsFloatValue()
        {
            var realValue = new GeTReal(1, 2);
            var atom = new GeTAtom(realValue: realValue);

            Assert.Equal(0.5f, atom.GetValue());
        }

        [Fact]
        public void GetValue_WithBooleanValue_ReturnsBooleanValue()
        {
            var atom = new GeTAtom(booleanValue: true);

            Assert.Equal(true, atom.GetValue());
        }

        [Fact]
        public void GetValue_WithNoValue_ReturnsNull()
        {
            var atom = new GeTAtom();

            Assert.Null(atom.GetValue());
        }

    }

    public class GeTRealTests
    {
        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var real1 = new GeTReal(1, 2);
            var real2 = new GeTReal(3L, 4L);

            // Assert
            Assert.Equal(1, real1.Numerator);
            Assert.Equal(2, real1.Denominator);
            Assert.Equal(3, real2.Numerator);
            Assert.Equal(4, real2.Denominator);
        }

        [Fact]
        public void ToFloat_ReturnsCorrectValue()
        {
            // Arrange
            var real = new GeTReal(1, 2);

            // Act
            var result = real.ToDouble();

            // Assert
            Assert.Equal(0.5f, result);
        }

        [Fact]
        public void ToString_ReturnsCorrectRepresentation()
        {
            // Arrange
            var real = new GeTReal(1, 2);

            // Act
            var result = real.ToString();

            // Assert
            Assert.Equal("1/2", result);
        }

        [Fact]
        public void Equals_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var real1 = new GeTReal(1, 2);
            var real2 = new GeTReal(1, 2);

            // Act & Assert
            Assert.True(real1.Equals(real2));
        }

        [Fact]
        public void Equals_WithDifferentValues_ReturnsFalse()
        {
            // Arrange
            var real1 = new GeTReal(1, 2);
            var real2 = new GeTReal(1, 3);

            // Act & Assert
            Assert.False(real1.Equals(real2));
        }
    }
}
