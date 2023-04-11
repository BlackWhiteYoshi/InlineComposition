using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace InlineComposition;

internal static class NodeExtensions
{
    /// <summary>
    /// Finds the first node of type T by traversing the parent nodes.
    /// </summary>
    /// <typeparam name="T">the type of </typeparam>
    /// <param name="syntaxNode"></param>
    /// <returns>The first node of type T, otherwise null.</returns>
    internal static T? GetParent<T>(this SyntaxNode syntaxNode) where T : SyntaxNode
    {
        SyntaxNode? currentNode = syntaxNode.Parent;
        while (currentNode != null)
        {
            if (currentNode is T t)
                return t;

            currentNode = currentNode.Parent;
        }

        return null;
    }

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
}
