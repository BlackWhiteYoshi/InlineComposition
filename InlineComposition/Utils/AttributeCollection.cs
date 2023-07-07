using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace InlineComposition;

public readonly struct AttributeCollection : IEquatable<AttributeCollection> {
    public readonly AttributeSyntax inlineAttribute;
    public readonly TypeDeclarationSyntax inlineClass;
    public readonly ImmutableArray<AttributeSyntax?> baseAttributes;
    public readonly ImmutableArray<TypeDeclarationSyntax?> baseClasses;

    public AttributeCollection(AttributeSyntax inlineAttribute, AttributeSyntax?[] baseAttributes, TypeDeclarationSyntax?[] baseClasses) {
        this.inlineAttribute = inlineAttribute;
        inlineClass = (TypeDeclarationSyntax)inlineAttribute.Parent!.Parent!;
        this.baseAttributes = Unsafe.As<AttributeSyntax?[], ImmutableArray<AttributeSyntax?>>(ref baseAttributes);
        this.baseClasses = Unsafe.As<TypeDeclarationSyntax?[], ImmutableArray<TypeDeclarationSyntax?>>(ref baseClasses);
    }


    public readonly override bool Equals(object? obj) {
        if (obj is not AttributeCollection collection)
            return false;

        return Equals(collection);
    }

    public readonly bool Equals(AttributeCollection other) {
        if (baseAttributes != other.baseAttributes)
            return false;

        if (inlineClass != other.inlineClass)
            return false;

        if (!baseAttributes.SequenceEqual(other.baseAttributes))
            return false;

        if (!baseClasses.SequenceEqual(other.baseClasses))
            return false;

        return true;
    }

    public static bool operator ==(AttributeCollection left, AttributeCollection right) => left.Equals(right);

    public static bool operator !=(AttributeCollection left, AttributeCollection right) => !(left == right);

    public readonly override int GetHashCode() {
        int hashCode = inlineAttribute.GetHashCode();

        hashCode = Combine(hashCode, inlineClass.GetHashCode());

        foreach (AttributeSyntax? attribute in baseAttributes)
            hashCode = Combine(hashCode, attribute?.GetHashCode() ?? 0);

        foreach (TypeDeclarationSyntax? baseClass in baseClasses)
            hashCode = Combine(hashCode, baseClass?.GetHashCode() ?? 0);

        return hashCode;


        static int Combine(int h1, int h2) {
            uint rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
            return ((int)rol5 + h1) ^ h2;
        }
    }
}
