using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace InlineComposition;

public struct AttributeCollection : IEquatable<AttributeCollection> {
    public AttributeSyntax inlineAttribute;
    public TypeDeclarationSyntax inlineClass;
    public ImmutableArray<AttributeSyntax?> baseAttributes;
    public ImmutableArray<TypeDeclarationSyntax?> baseClasses;

    public AttributeCollection(AttributeSyntax inlineAttribute, AttributeSyntax?[] baseAttributes, TypeDeclarationSyntax?[] baseClasses) {
        this.inlineAttribute = inlineAttribute;
        inlineClass = (TypeDeclarationSyntax)inlineAttribute.Parent!.Parent!;
        this.baseAttributes = ImmutableArray.Create(baseAttributes);
        this.baseClasses = ImmutableArray.Create(baseClasses);
    }

    public override bool Equals(object? obj) {
        if (obj is not AttributeCollection collection)
            return false;

        return Equals(collection);
    }

    public bool Equals(AttributeCollection other) {
        if (inlineClass != other.inlineClass)
            return false;

        for (int i = 0; i < baseClasses.Length; i++)
            if (baseClasses[i] != other.baseClasses[i])
                return false;

        return true;
    }

    public static bool operator ==(AttributeCollection left, AttributeCollection right) => left.Equals(right);

    public static bool operator !=(AttributeCollection left, AttributeCollection right) => !(left == right);

    public override int GetHashCode() {
        int hashCode = inlineClass.GetHashCode();

        foreach (TypeDeclarationSyntax? baseClass in baseClasses)
            hashCode = Combine(hashCode, baseClass?.GetHashCode() ?? 0);

        return hashCode;


        static int Combine(int h1, int h2) {
            uint rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
            return ((int)rol5 + h1) ^ h2;
        }
    }
}
