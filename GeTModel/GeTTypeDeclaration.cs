using Proto;
namespace GeTPlanModel
{
    [System.Serializable]
    public class GeTTypeDeclaration
    {
        public string TypeName { get; set; }
        public string TypeParent { get; set; }


        public GeTTypeDeclaration(string typeName, string typeParent)
        {
            this.TypeName = typeName;
            this.TypeParent = typeParent;
        }

        public override string ToString()
        {
            return $"{TypeName} - {TypeParent}";
        }
    }

    public class TypeDeclarationModelFactory : IGeTModelFactory<GeTTypeDeclaration, TypeDeclaration>, IProtoFactory<GeTTypeDeclaration, TypeDeclaration>
    {
        public GeTTypeDeclaration FromProto(TypeDeclaration typeDeclaration)
        {
            var typeName = typeDeclaration.TypeName;
            var parentName = typeDeclaration.ParentType;
            parentName = parentName == string.Empty ? "object" : parentName;
            return new GeTTypeDeclaration(typeName, parentName);
        }

        public TypeDeclaration ToProto(GeTTypeDeclaration typeDeclaration)
        {
            var typeName = typeDeclaration.TypeName;
            var parentName = typeDeclaration.TypeParent;
            parentName = parentName == string.Empty ? "object" : parentName;
            return new TypeDeclaration() { ParentType = parentName, TypeName = typeName, };
        }
    }
}
