using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace XamlX.TypeSystem
{
    partial class CecilTypeSystem
    {
        public class CecilField : IXamlField
        {
            private readonly FieldDefinition _def;
            public CecilTypeSystem TypeSystem { get; }
            public FieldReference Field { get; }

            public CecilField(CecilTypeSystem typeSystem, FieldDefinition def, TypeReference declaringType)
            {
                TypeSystem = typeSystem;
                _def = def;
                Field = new FieldReference(def.Name, def.FieldType, declaringType);
            }

            public string Name => Field.Name;
            private IXamlType _type;
            public IXamlType FieldType => _type ?? (_type = TypeSystem.Resolve(Field.FieldType.TransformGeneric(Field.DeclaringType)));
            public bool IsPublic => _def.IsPublic;
            public bool IsStatic => _def.IsStatic;
            public bool IsLiteral => _def.IsLiteral;
            
            private IReadOnlyList<IXamlCustomAttribute> _attributes;
            public IReadOnlyList<IXamlCustomAttribute> CustomAttributes =>
                _attributes ?? (_attributes =
                    Field.Resolve().CustomAttributes.Select(ca => new CecilCustomAttribute(TypeSystem, ca)).ToList());
            
            public object GetLiteralValue()
            {
                if (IsLiteral && _def.HasConstant)
                    return _def.Constant;
                return null;
            }

            public bool Equals(IXamlField other) =>
                other is CecilField cf
                && TypeReferenceEqualityComparer.AreEqual(Field.DeclaringType, cf.Field.DeclaringType)
                && cf.Field.FullName == Field.FullName;

            public override bool Equals(object other) => Equals(other as IXamlField); 

            public override int GetHashCode() =>
                (TypeReferenceEqualityComparer.GetHashCodeFor(Field.DeclaringType), Field.FullName).GetHashCode();

            public override string ToString() => Field.ToString();
        }
    }
}
