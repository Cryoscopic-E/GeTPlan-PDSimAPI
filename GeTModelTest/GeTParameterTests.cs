using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeTPlanModel;
using Proto;
namespace GeTModelTest
{
    public class GeTParameterTests
    {
        [Fact]
        public void Equals_WithSameValues_ReturnsTrue()
        {
            var parameter1 = new GeTParameter("name", "type");
            var parameter2 = new GeTParameter("name", "type");
            Assert.True(parameter1.Equals(parameter2));
        }

        [Fact]
        public void Equals_WithDifferentValues_ReturnsFalse()
        {
            var parameter1 = new GeTParameter("name", "type");
            var parameter2 = new GeTParameter("name", "type2");
            Assert.False(parameter1.Equals(parameter2));
        }

        [Fact]
        public void ToString_ReturnsCorrectFormat()
        {
            var parameter = new GeTParameter("name", "type");
            Assert.Equal("name - type", parameter.ToString());
        }


        // FACTORY TESTS

        [Fact]
        public void Create_WithName_ReturnsObjectParameter()
        {
            var parameter = ParameterModelFactory.Create("name");
            Assert.Equal("name", parameter.Name);
            Assert.Equal("object", parameter.Type);
        }

        [Fact]
        public void Create_WithNameAndType_ReturnsParameter()
        {
            var parameter = ParameterModelFactory.Create("name", "type");
            Assert.Equal("name", parameter.Name);
            Assert.Equal("type", parameter.Type);
        }

        [Fact]
        public void FromProto_ReturnsParameter()
        {
            var parameter = new Parameter
            {
                Name = "name",
                Type = "type"
            };
            var result = new ParameterModelFactory().FromProto(parameter);
            Assert.Equal("name", result.Name);
            Assert.Equal("type", result.Type);
        }

        [Fact]
        public void ToProto_ReturnsParameter()
        {
            var parameter = new GeTParameter("name", "type");
            var result = new ParameterModelFactory().ToProto(parameter);
            Assert.Equal("name", result.Name);
            Assert.Equal("type", result.Type);
        }
    }
}
