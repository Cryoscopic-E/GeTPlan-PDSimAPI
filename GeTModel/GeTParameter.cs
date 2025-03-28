using Proto;
namespace GeTPlanModel
{
    [System.Serializable]
    public class GeTParameter
    {
        public string Name { get; private set; }
        public string Type { get; private set; }

        public GeTParameter(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public override string ToString()
        {
            return $"{Name} - {Type}";
        }

        public override bool Equals(object? obj)
        {
            if (!(obj is GeTParameter other)) return false;

            return Name == other.Name && Type == other.Type;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() + Type.GetHashCode();
        }
    }

    public class ParameterModelFactory : IGeTModelFactory<GeTParameter, Parameter>, IProtoFactory<GeTParameter, Parameter>
    {
        public static GeTParameter Create(string name)
        {
            return new GeTParameter(name, "object");
        }

        public static GeTParameter Create(string name, string type)
        {
            return new GeTParameter(name, type);
        }


        public GeTParameter FromProto(Parameter parameter)
        {
            return new GeTParameter(parameter.Name, parameter.Type); 
        }

        public Parameter ToProto(GeTParameter model)
        {
            return new Parameter
            {
                Name = model.Name,
                Type = model.Type
            };
        }
    }
}
