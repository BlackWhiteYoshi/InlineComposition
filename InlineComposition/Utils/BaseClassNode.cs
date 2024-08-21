using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace InlineComposition;

/// <summary>
/// Holds a reference to a class/struct node and its generic arguments as string array
/// </summary>
internal struct BaseClassNode() {
    public TypeDeclarationSyntax? baseClass;
    public bool mapBaseType = false;
    public bool ignoreInheritenceAndImplements = false;
    public string[] genericArguments = [];
}
