using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace InlineComposition;

internal static class NodeExtensions {
    /// <summary>
    /// Finds the first node of type T by traversing the parent nodes.
    /// </summary>
    /// <typeparam name="T">the type of </typeparam>
    /// <param name="syntaxNode"></param>
    /// <returns>The first node of type T, otherwise null.</returns>
    internal static T? GetParent<T>(this SyntaxNode syntaxNode) where T : SyntaxNode {
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
    internal static AttributeSyntax? GetAttribute(this MemberDeclarationSyntax node, string attributeName, string attributeNameAttribute) {
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
    /// If QualifiedNameSyntax, only right part is taken<br/>
    /// if it is not a GenericNameSyntax, cast-exception is thrown
    /// </summary>
    /// <param name="nameSyntax"></param>
    /// <returns></returns>
    internal static GenericNameSyntax GetGenericNameSyntax(this NameSyntax nameSyntax) {
        if (nameSyntax is QualifiedNameSyntax qualifiedName)
            nameSyntax = qualifiedName.Right;

        return (GenericNameSyntax)nameSyntax;
    }
}
