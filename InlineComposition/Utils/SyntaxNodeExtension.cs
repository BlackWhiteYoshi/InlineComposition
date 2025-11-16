using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace InlineComposition;

public static class NodeExtensions {
    /// <summary>
    /// Finds the first node of type T by traversing the parent nodes.
    /// </summary>
    /// <typeparam name="T">the type of </typeparam>
    /// <param name="syntaxNode"></param>
    /// <returns>The first node of type T, otherwise null.</returns>
    public static T? GetParent<T>(this SyntaxNode syntaxNode) where T : SyntaxNode {
        SyntaxNode? currentNode = syntaxNode.Parent;
        while (currentNode != null) {
            if (currentNode is T t)
                return t;

            currentNode = currentNode.Parent;
        }

        return null;
    }

    /// <summary>
    /// Finds the first attribute that matches the given name.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="attributeName"></param>
    /// <param name="attributeNameAttribute"></param>
    /// <returns></returns>
    public static AttributeSyntax? GetAttribute(this MemberDeclarationSyntax node, string attributeName, string attributeNameAttribute) {
        foreach (AttributeListSyntax attributeList in node.AttributeLists)
            foreach (AttributeSyntax attribute in attributeList.Attributes) {
                string identifier = attribute.Name switch {
                    SimpleNameSyntax simpleName => simpleName.Identifier.ValueText,
                    QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.ValueText,
                    _ => string.Empty
                };
                if (identifier == attributeName || identifier == attributeNameAttribute)
                    return attribute;
            }

        return null;
    }

    /// <summary>
    /// <para>Finds the attribute with the given name.</para>
    /// <para>If the given attribute is not present, it returns null.</para>
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="name"></param>
    /// <param name="namspace">namespace-list from back to front: e.g. System.Diagnostics.CodeAnalysis -> ["CodeAnalysis", "Diagnostics", "System"]</param>
    /// <returns></returns>
    public static AttributeData? GetAttribute(this ISymbol symbol, string name, string[] namspace) {
        foreach (AttributeData attributeData in symbol.GetAttributes()) {
            if (attributeData.AttributeClass is not INamedTypeSymbol attribute)
                continue;

            if (attribute.Name != name)
                continue;

            INamespaceSymbol namespaceSymbol = attribute.ContainingNamespace;
            foreach (string namespacePart in namspace) {
                if (namespacePart != namespaceSymbol.Name)
                    goto break_continue;
                namespaceSymbol = namespaceSymbol.ContainingNamespace;
            }
            if (namespaceSymbol.Name != string.Empty)
                continue;

            return attributeData;
            break_continue:;
        }

        return null;
    }

    /// <summary>
    /// <para>Finds the argument with the given name and returns it's value.</para>
    /// <para>If not found or value is not castable, it returns default.</para>
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T? GetArgument<T>(this ImmutableArray<KeyValuePair<string, TypedConstant>> arguments, string name) {
        for (int i = 0; i < arguments.Length; i++)
            if (arguments[i].Key == name)
                return arguments[i].Value switch {
                    TypedConstant { Value: T value } => value,
                    _ => default
                };

        return default;
    }
}
