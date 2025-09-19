using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace InlineComposition;

public readonly struct AttributeCollection(TypeDeclarationSyntax inlineClass, AttributeData inlineAttribute, TypeDeclarationSyntax?[] baseClassArray, AttributeData?[] baseAttributeArray) : IEquatable<AttributeCollection> {
    public readonly TypeDeclarationSyntax inlineClass = inlineClass;
    public readonly AttributeData inlineAttribute = inlineAttribute;
    public readonly ImmutableArray<TypeDeclarationSyntax?> baseClasses = Unsafe.As<TypeDeclarationSyntax?[], ImmutableArray<TypeDeclarationSyntax?>>(ref baseClassArray);
    public readonly ImmutableArray<AttributeData?> baseAttributes = Unsafe.As<AttributeData?[], ImmutableArray<AttributeData?>>(ref baseAttributeArray);

    public readonly override bool Equals(object? obj) {
        if (obj is not AttributeCollection collection)
            return false;

        return Equals(collection);
    }

    public readonly bool Equals(AttributeCollection other) {
        if (inlineClass != other.inlineClass)
            return false;

        if (baseAttributes != other.baseAttributes)
            return false;

        if (!baseClasses.SequenceEqual(other.baseClasses))
            return false;

        if (!baseAttributes.SequenceEqual(other.baseAttributes))
            return false;

        return true;
    }

    public static bool operator ==(AttributeCollection left, AttributeCollection right) => left.Equals(right);

    public static bool operator !=(AttributeCollection left, AttributeCollection right) => !(left == right);

    public readonly override int GetHashCode() {
        int hashCode = inlineClass.GetHashCode();

        hashCode = Combine(hashCode, inlineAttribute.GetHashCode());

        foreach (TypeDeclarationSyntax? baseClass in baseClasses)
            hashCode = Combine(hashCode, baseClass?.GetHashCode() ?? 0);

        foreach (AttributeData? attribute in baseAttributes)
            hashCode = Combine(hashCode, attribute?.GetHashCode() ?? 0);

        return hashCode;


        static int Combine(int h1, int h2) {
            uint rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
            return ((int)rol5 + h1) ^ h2;
        }
    }
}
