using Proto;
namespace GeTPlanModel
{
    [System.Serializable]
    public class GeTObjectDeclaration
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public GeTObjectDeclaration(string objectName, string objectType)
        {
            this.Name = objectName;
            this.Type = objectType;
        }

        public override string ToString()
        {
            return $"{Name} - {Type}";
        }
    }

    public class ObjectDeclarationModelFactory : IGeTModelFactory<GeTObjectDeclaration, ObjectDeclaration>, IProtoFactory<GeTObjectDeclaration, ObjectDeclaration>
    {
        public GeTObjectDeclaration FromProto(ObjectDeclaration objectDeclaration)
        {
            var objName = objectDeclaration.Name;
            var objectType = objectDeclaration.Type;
            return new GeTObjectDeclaration(objName, objectType);
        }

        public ObjectDeclaration ToProto(GeTObjectDeclaration objectDeclaration)
        {
            var objName = objectDeclaration.Name;
            var objectType = objectDeclaration.Type;
            return new ObjectDeclaration
            {
                Name = objName,
                Type = objectType
            };
        }
    }
}
