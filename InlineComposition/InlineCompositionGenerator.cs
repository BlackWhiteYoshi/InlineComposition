using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;

namespace InlineComposition;

[Generator]
public sealed class InlineCompositionGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        // register attribute marker
        context.RegisterPostInitializationOutput((IncrementalGeneratorPostInitializationContext context) => {
            context.AddSource("InlineBaseAttribute.g.cs", Attributes.InlineBaseAttribute);
            context.AddSource("NoInlineAttribute.g.cs", Attributes.NoInlineAttribute);
            context.AddSource("InlineAttribute.g.cs", Attributes.InlineAttribute);
            context.AddSource("InlineMethodAttribute.g.cs", Attributes.InlineMethodAttribute);
            context.AddSource("InlineConstructorAttribute.g.cs", Attributes.InlineConstructorAttribute);
            context.AddSource("InlineFinalizerAttribute.g.cs", Attributes.InlineFinalizerAttribute);
        });

        // all classes/structs with InlineBase attribute
        IncrementalValueProvider<ImmutableArray<AttributeSyntax>> inlineBaseListProvider = context.SyntaxProvider.CreateSyntaxProvider(PredicateInlineBase, Identity<AttributeSyntax>)
            .Collect();

        // Inline attributes
        IncrementalValuesProvider<AttributeSyntax> inlineProvider = context.SyntaxProvider.CreateSyntaxProvider(PredicateInline, Identity<AttributeSyntax>);

        // Inline attributes with baseClasses
        IncrementalValuesProvider<AttributeCollection> inlineAndInlineBaseListProvider = inlineProvider.Combine(inlineBaseListProvider)
            .Select(((AttributeSyntax inlineAttribute, ImmutableArray<AttributeSyntax> baseAttributes) tuple, CancellationToken _) => {
                SeparatedSyntaxList<TypeSyntax> arguments = ((GenericNameSyntax)tuple.inlineAttribute.Name).TypeArgumentList.Arguments;
                AttributeSyntax?[] resultAttribute = new AttributeSyntax[arguments.Count];
                TypeDeclarationSyntax?[] resultClass = new TypeDeclarationSyntax[arguments.Count];

                for (int i = 0; i < arguments.Count; i++) {
                    string name = arguments[i] switch {
                        PredefinedTypeSyntax predefinedType => predefinedType.Keyword.ValueText,
                        SimpleNameSyntax simpleNameSyntax => simpleNameSyntax.Identifier.ValueText,
                        QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.ValueText,
                        _ => throw new Exception("No matching SyntaxType for generic argument: cannot identify value of generic argument")
                    };

                    // linear search for listed class/struct
                    foreach (AttributeSyntax baseAttribute in tuple.baseAttributes) {
                        TypeDeclarationSyntax baseClass = (TypeDeclarationSyntax)baseAttribute.Parent!.Parent!;
                        if (name == baseClass.Identifier.ValueText) {
                            resultAttribute[i] = baseAttribute;
                            resultClass[i] = baseClass;
                            break;
                        }
                    }
                }

                return new AttributeCollection(tuple.inlineAttribute, resultAttribute, resultClass);
            });

        context.RegisterSourceOutput(inlineAndInlineBaseListProvider, Execute);
    }


    #region Predicate

    private bool PredicateInlineBase(SyntaxNode syntaxNode, CancellationToken _) => PredicateInlineCore(syntaxNode, "InlineBase", "InlineBaseAttribute");
    private bool PredicateInline(SyntaxNode syntaxNode, CancellationToken _) => PredicateInlineCore(syntaxNode, "Inline", "InlineAttribute");
    private bool PredicateInlineCore(SyntaxNode syntaxNode, string name, string nameAttribute) {
        if (syntaxNode is not AttributeSyntax attributeSyntax)
            return false;

        if (attributeSyntax.Parent?.Parent is not (ClassDeclarationSyntax or StructDeclarationSyntax))
            return false;


        string identifier = attributeSyntax.Name switch {
            SimpleNameSyntax simpleName => simpleName.Identifier.ValueText,
            QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.ValueText,
            _ => string.Empty
        };

        if (identifier != name && identifier != nameAttribute)
            return false;


        return true;
    }

    #endregion


    #region Transform

    private T Identity<T>(GeneratorSyntaxContext syntaxContext, CancellationToken _) where T : SyntaxNode => (T)syntaxContext.Node;
    
    //private TypeDeclarationSyntax AttributeToClass(GeneratorSyntaxContext syntaxContext, CancellationToken _) => (TypeDeclarationSyntax)syntaxContext.Node.Parent!.Parent!;

    #endregion


    private void Execute(SourceProductionContext context, AttributeCollection attributeProvider) {
        AttributeSyntax inlineAttribute = attributeProvider.inlineAttribute;
        TypeDeclarationSyntax derivedClass = attributeProvider.inlineClass;
        ImmutableArray<AttributeSyntax?> baseAttributes = attributeProvider.baseAttributes;
        ImmutableArray<TypeDeclarationSyntax?> baseClasses = attributeProvider.baseClasses;

        TypeDeclarationSyntax inlineClass = (TypeDeclarationSyntax)inlineAttribute.Parent!.Parent!;
        string inlineClassName = inlineClass.Identifier.ValueText;

        HashSet<string> usingStatementList = new();
        HashSet<string> baseList = new();
        Dictionary<string, string> fieldList = new();
        Dictionary<string, string> propertyList = new();
        Dictionary<string, string> eventList = new();
        Dictionary<string, MethodEntry> methodList = new();


        // usings of inlineClass
        {
            CompilationUnitSyntax? compilationUnit = inlineClass.GetParent<CompilationUnitSyntax>();
            if (compilationUnit != null)
                foreach (UsingDirectiveSyntax usingSyntax in compilationUnit.Usings)
                    usingStatementList.Add(usingSyntax.Name.ToFullString());
        }

        if (context.CancellationToken.IsCancellationRequested)
            return;

        // InlineMethods
        foreach (MemberDeclarationSyntax node in derivedClass.Members) {
            AttributeSyntax? inlineMethodAttribute = node.GetAttribute("InlineMethod", "InlineMethodAttribute");
            if (inlineMethodAttribute?.ArgumentList is AttributeArgumentListSyntax attributeArgumentList) {
                foreach (AttributeArgumentSyntax attributeArgument in attributeArgumentList.Arguments)
                    if (attributeArgument.NameEquals?.Name.Identifier.ValueText == "MethodName") {
                        if (attributeArgument.Expression is LiteralExpressionSyntax literal) {
                            string name = literal.Token.ValueText;
                            MethodDeclarationSyntax methodDeclaration = (MethodDeclarationSyntax)node;
                            string methodName = CreateMethodName(name, methodDeclaration.ParameterList);
                            CaseCore(name, methodName, AddMethodHead, inlineMethodAttribute, methodList);
                        }
                        break;
                    }
            }
            else {
                AttributeSyntax? inlineConstructorAttribute = node.GetAttribute("InlineConstructor", "InlineConstructorAttribute");
                if (inlineConstructorAttribute != null) {
                    string name = inlineClassName;
                    MethodDeclarationSyntax derivedConstructorMethod = (MethodDeclarationSyntax)node;
                    string methodName = CreateMethodName(name, derivedConstructorMethod.ParameterList);
                    CaseCore(name, methodName, AddConDestructorHead, inlineConstructorAttribute, methodList);
                }
                else {
                    AttributeSyntax? inlineFinalizerAttribute = node.GetAttribute("InlineFinalizer", "InlineFinalizerAttribute");
                    if (inlineFinalizerAttribute != null) {
                        string name = $"~{inlineClassName}";
                        CaseCore(name, name, AddConDestructorHead, inlineFinalizerAttribute, methodList, string.Empty);
                    }
                }
            }

            static void CaseCore(string name, string methodName, Action<MethodDeclarationSyntax, List<string>, string, string?> AddHeadMethod, AttributeSyntax attribute, Dictionary<string, MethodEntry> methodList, string? modifiers = null) {
                bool first = false;
                if (attribute.ArgumentList is AttributeArgumentListSyntax attributeArgumentList)
                    foreach (AttributeArgumentSyntax attributeArgument in attributeArgumentList.Arguments) {
                        if (attributeArgument.NameEquals?.Name.Identifier.ValueText == "Modifiers")
                            if (attributeArgument.Expression is LiteralExpressionSyntax stringliteral)
                                modifiers = stringliteral.Token.ValueText;

                        if (attributeArgument.NameEquals?.Name.Identifier.ValueText == "First")
                            if (attributeArgument.Expression is LiteralExpressionSyntax boolLiteral)
                                first = boolLiteral.Token.Value as bool? ?? false;
                    }

                MethodDeclarationSyntax method = (MethodDeclarationSyntax)attribute.Parent!.Parent!;
                MethodEntry entry = new();

                AddHeadMethod(method, entry.headList, name, modifiers);

                if (first)
                    entry.blockList.Add(GetMethodBody(method));
                else
                    entry.lastBlock = GetMethodBody(method);

                methodList.Add(methodName, entry);
            }
        }

        if (context.CancellationToken.IsCancellationRequested)
            return;

        BaseClassNode[] baseClassNodes;
        {
            SeparatedSyntaxList<TypeSyntax> arguments = ((GenericNameSyntax)inlineAttribute.Name).TypeArgumentList.Arguments;
            baseClassNodes = new BaseClassNode[arguments.Count];
            for (int i = 0; i < arguments.Count; i++) {
                baseClassNodes[i] = new();

                string name = arguments[i] switch {
                    PredefinedTypeSyntax predefinedType => predefinedType.Keyword.ValueText,
                    SimpleNameSyntax simpleNameSyntax => simpleNameSyntax.Identifier.ValueText,
                    QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.ValueText,
                    _ => throw new Exception("No matching SyntaxType for generic argument: cannot identify value of generic argument")
                };

                TypeDeclarationSyntax? baseClass = baseClasses[i];
                if (name == baseClass?.Identifier.ValueText) {
                    baseClassNodes[i].baseClass = baseClass;

                    // check for IgnoreInheritenceAndImplements
                    AttributeSyntax? attribute = baseAttributes[i];
                    if (attribute!.ArgumentList?.Arguments.Count > 0)
                        if (attribute!.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax literalExpression)
                            baseClassNodes[i].ignoreInheritenceAndImplements = (bool)literalExpression.Token.Value!;

                    // extract generic arguments
                    if (arguments[i] is GenericNameSyntax genericNameSyntax) {
                        SeparatedSyntaxList<TypeSyntax> genericArguments = genericNameSyntax.TypeArgumentList.Arguments;
                        baseClassNodes[i].genericArguments = new string[genericArguments.Count];
                        for (int j = 0; j < genericArguments.Count; j++)
                            baseClassNodes[i].genericArguments[j] = genericArguments[j] switch {
                                PredefinedTypeSyntax predefinedType => predefinedType.Keyword.ValueText,
                                SimpleNameSyntax simpleNameSyntax => simpleNameSyntax.Identifier.ValueText,
                                QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.ValueText,
                                _ => throw new Exception("No matching SyntaxType for generic argument: cannot identify value of generic argument")
                            };
                    }
                }
            }
        }

        if (context.CancellationToken.IsCancellationRequested)
            return;

        foreach (BaseClassNode baseClassNode in baseClassNodes) {
            TypeDeclarationSyntax? classType = baseClassNode.baseClass;
            if (classType == null)
                continue;

            // usings
            CompilationUnitSyntax? compilationUnit = classType.GetParent<CompilationUnitSyntax>();
            if (compilationUnit != null)
                foreach (UsingDirectiveSyntax usingSyntax in compilationUnit.Usings)
                    usingStatementList.Add(usingSyntax.Name.ToFullString());

            // baseclasses and interfaces
            if (!baseClassNode.ignoreInheritenceAndImplements && classType.BaseList != null)
                foreach (BaseTypeSyntax typeSyntax in classType.BaseList.Types)
                    baseList.Add(((IdentifierNameSyntax)typeSyntax.Type).Identifier.ValueText);

            // generic parameters
            string[] genericParameters;
            if (classType.TypeParameterList == null)
                genericParameters = Array.Empty<string>();
            else {
                SeparatedSyntaxList<TypeParameterSyntax> genericParameterList = classType.TypeParameterList.Parameters;
                genericParameters = new string[genericParameterList.Count];
                for (int i = 0; i < genericParameterList.Count; i++) {
                    genericParameters[i] = genericParameterList[i].Identifier.ValueText;
                }
            }

            // members
            foreach (MemberDeclarationSyntax node in classType.Members) {
                if (node.GetAttribute("NoInline", "NoInlineAttribute") != null)
                    continue;
                
                switch (node) {
                    case FieldDeclarationSyntax fieldDeclarationSyntax: {
                        if (fieldDeclarationSyntax.Declaration.Variables.Count > 1)
                            throw new ArgumentException("multiple variable declaration is not supported.");
                        string name = fieldDeclarationSyntax.Declaration.Variables.First().Identifier.ValueText;

                        if (!fieldList.ContainsKey(name)) {
                            string source = ReplaceGeneric(fieldDeclarationSyntax.ToFullString(), genericParameters, baseClassNode.genericArguments);
                            fieldList.Add(name, source);
                        }
                        break;
                    }

                    case PropertyDeclarationSyntax propertyDeclarationSyntax: {
                        string name = propertyDeclarationSyntax.Identifier.ValueText;

                        if (!propertyList.ContainsKey(name)) {
                            string source = ReplaceGeneric(propertyDeclarationSyntax.ToFullString(), genericParameters, baseClassNode.genericArguments);
                            propertyList.Add(name, source);
                        }
                        break;
                    }

                    case EventFieldDeclarationSyntax eventFieldDeclarationSyntax: {
                        if (eventFieldDeclarationSyntax.Declaration.Variables.Count > 1)
                            throw new ArgumentException("multiple variable declaration is not supported.");
                        string name = eventFieldDeclarationSyntax.Declaration.Variables.First().Identifier.ValueText;

                        if (!eventList.ContainsKey(name)) {
                            string source = ReplaceGeneric(eventFieldDeclarationSyntax.ToFullString(), genericParameters, baseClassNode.genericArguments);
                            eventList.Add(name, source);
                        }
                        break;
                    }

                    case MethodDeclarationSyntax methodDeclarationSyntax: {
                        string name = methodDeclarationSyntax.Identifier.ValueText;
                        string methodName = CreateMethodName(name, methodDeclarationSyntax.ParameterList);
                        CaseMethod(name, methodName, methodDeclarationSyntax, methodList, genericParameters, baseClassNode.genericArguments);
                        break;
                    }

                    case ConstructorDeclarationSyntax constructorDeclarationSyntax: {
                        string name = inlineClassName;
                        string methodName = CreateMethodName(name, constructorDeclarationSyntax.ParameterList);
                        CaseMethod(name, methodName, constructorDeclarationSyntax, methodList, genericParameters, baseClassNode.genericArguments);
                        break;
                    }

                    case DestructorDeclarationSyntax destructorDeclarationSyntax: {
                        string name = $"~{inlineClassName}";
                        CaseMethod(name, name, destructorDeclarationSyntax, methodList, genericParameters, baseClassNode.genericArguments);
                        break;
                    }

                    static void CaseMethod(string name, string methodName, BaseMethodDeclarationSyntax method, Dictionary<string, MethodEntry> methodList, string[] genericParameters, string[] genericArguments) {
                        if (!methodList.TryGetValue(methodName, out MethodEntry entry)) {
                            entry = new MethodEntry();
                            if (method is MethodDeclarationSyntax normalMethod)
                                AddMethodHead(normalMethod, entry.headList, name);
                            else
                                AddConDestructorHead(method, entry.headList, name);

                            methodList.Add(methodName, entry);
                        }

                        string rawSource = GetMethodBody(method);
                        string source = ReplaceGeneric(rawSource, genericParameters, genericArguments);
                        entry.blockList.Add(source);
                    }
                }
            }
        }


        if (context.CancellationToken.IsCancellationRequested)
            return;


        #region writing SourceCode

        StringBuilder builder = new(65536);
        builder.Append("""
            // <auto-generated>
            #pragma warning disable
            #nullable enable annotations



            """);

        // usingStatements
        if (usingStatementList.Count > 0) {
            foreach (string usingStatement in usingStatementList) {
                builder.Append("using ");
                builder.Append(usingStatement);
                builder.Append(';');
                builder.Append('\n');
            }

            builder.Append('\n');
        }

        // namespace
        BaseNamespaceDeclarationSyntax? namespaceSyntax = inlineClass.GetParent<BaseNamespaceDeclarationSyntax>();
        if (namespaceSyntax != null) {
            builder.Append("namespace ");
            builder.Append(namespaceSyntax.Name.ToString());
            builder.Append(';');
            builder.Append('\n');
            builder.Append('\n');
        }

        // class/struct declaration
        foreach (SyntaxToken token in inlineClass.Modifiers) {
            builder.Append(token.ValueText);
            builder.Append(' ');
        }
        builder.Append(inlineClass.Keyword.ValueText);
        builder.Append(' ');
        builder.Append(inlineClassName);
        builder.Append(inlineClass.TypeParameterList?.ToFullString());
        if (baseList.Count > 0) {
            builder.Append(" :");
            foreach (string baseIdentifier in baseList) {
                builder.Append(' ');
                builder.Append(baseIdentifier);
                builder.Append(',');
            }
            builder.Length--;
        }
        builder.Append(" {");
        builder.Append('\n');

        // body
        foreach (KeyValuePair<string, string> pair in fieldList) {
            builder.Append(pair.Value);
            builder.Append('\n');
        }
        foreach (KeyValuePair<string, string> pair in propertyList) {
            builder.Append(pair.Value);
            builder.Append('\n');
        }
        foreach (KeyValuePair<string, string> pair in eventList) {
            builder.Append(pair.Value);
            builder.Append('\n');
        }
        foreach (KeyValuePair<string, MethodEntry> pair in methodList) {
            foreach (string chunk in pair.Value.headList)
                builder.Append(chunk);
            foreach (string block in pair.Value.blockList) {
                builder.Append("        {");
                builder.Append('\n');
                builder.Append(block);
                builder.Append("        }");
                builder.Append('\n');
            }
            if (pair.Value.lastBlock != string.Empty) {
                builder.Append("        {");
                builder.Append('\n');
                builder.Append(pair.Value.lastBlock);
                builder.Append("        }");
                builder.Append('\n');
            }
            // close method
            builder.Append("    }");
            builder.Append('\n');
            builder.Append('\n');
        }

        // close class/struct
        builder.Append('}');
        builder.Append('\n');


        context.AddSource($"{inlineClassName}.g.cs", builder.ToString());

        #endregion
    }


    /// <summary>
    /// <para>Create a unique string identifier for overloaded methods.</para>
    /// <para>e.g.<br/>
    /// MyMethod -> MyMethod<br/>
    /// MyMethod() -> MyMethod<br/>
    /// MyMethod(int a, string b) -> MyMethod(int,string)
    /// </para>
    /// </summary>
    /// <param name="functionName"></param>
    /// <param name="parameterList"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="Exception"></exception>
    private static string CreateMethodName(string functionName, ParameterListSyntax parameterList) {
        if (parameterList.Parameters.Count == 0)
            return functionName;

        string[] parameters = new string[parameterList.Parameters.Count];
        for (int i = 0; i < parameterList.Parameters.Count; i++) {
            TypeSyntax typeSyntax = parameterList.Parameters[i].Type!;
            while (true) {
                if (typeSyntax is PredefinedTypeSyntax predefinedType) {
                    parameters[i] = predefinedType.Keyword.ValueText;
                    break;
                }
                if (typeSyntax is IdentifierNameSyntax identifierName) {
                    parameters[i] = identifierName.Identifier.ValueText;
                    break;
                }
                if (typeSyntax is TupleTypeSyntax tupleTypeSyntax) {
                    parameters[i] = tupleTypeSyntax.Elements.ToFullString();
                    break;
                }

                typeSyntax = typeSyntax switch {
                    ArrayTypeSyntax arrayType => arrayType.ElementType,
                    NullableTypeSyntax nullableType => nullableType.ElementType,
                    RefTypeSyntax refType => refType.Type,
                    PointerTypeSyntax pointerType => pointerType.ElementType,
                    FunctionPointerTypeSyntax => throw new NotSupportedException("function pointers are not supported"),
                    _ => throw new Exception($"No Match for parameterType {typeSyntax}: {typeSyntax.GetType()}")
                };
            }
        }

        int length = functionName.Length + 1;
        foreach (string parameter in parameters)
            length += parameter.Length;
        length += parameters.Length;
        Span<char> result = (length < 1024) switch {
            true => stackalloc char[length],
            false => new char[length]
        };
        {
            int i = 0;
            functionName.AsSpan().CopyTo(result);
            i += functionName.Length;
            result[i] = '(';
            i++;
            foreach (string parameter in parameters) {
                parameter.AsSpan().CopyTo(result[i..]);
                i += parameter.Length;
                result[i] = ',';
                i++;
            }
            result[i - 1] = ')';
        }

        return result.ToString();
    }

    private static void AddMethodHead(MethodDeclarationSyntax method, List<string> list, string name, string? modifiers = null) {
        list.Add(method.AttributeLists.ToFullString());
        
        list.Add("    ");
        if (modifiers != null) {
            if (modifiers != string.Empty) {
                list.Add(modifiers);
                list.Add(" ");
            }
        }
        else
            foreach (SyntaxToken token in method.Modifiers) {
                list.Add(token.ValueText);
                list.Add(" ");
            }

        list.Add(method.ReturnType switch {
            PredefinedTypeSyntax predefinedType => predefinedType.Keyword.ValueText,
            SimpleNameSyntax simpleNameSyntax => simpleNameSyntax.Identifier.ValueText,
            _ => throw new Exception("No matching SyntaxType for returnType: cannot identify value of returnType")
        });
        
        list.Add(" ");
        if (method.ExplicitInterfaceSpecifier != null) {
            list.Add(((IdentifierNameSyntax)method.ExplicitInterfaceSpecifier.Name).Identifier.ValueText);
            list.Add(".");
        }
        list.Add(name);
        
        if (method.TypeParameterList != null)
            list.Add(method.TypeParameterList.ToFullString());
        list.Add(method.ParameterList.ToFullString());
        list.Add("{\n");
    }

    private static void AddConDestructorHead(BaseMethodDeclarationSyntax method, List<string> list, string name, string? modifiers = null) {
        list.Add(method.AttributeLists.ToFullString());
        list.Add("    ");
        if (modifiers != null) {
            if (modifiers != string.Empty) {
                list.Add(modifiers);
                list.Add(" ");
            }
        }
        else
            foreach (SyntaxToken token in method.Modifiers) {
                list.Add(token.ValueText);
                list.Add(" ");
            }
        list.Add(name);
        list.Add(method.ParameterList.ToFullString());
        list.Add("{\n");
    }

    private static string GetMethodBody(BaseMethodDeclarationSyntax method) {
        if (method.Body != null)
            return method.Body.Statements.ToFullString();
        
        if (method.ExpressionBody != null) {
            if (method is MethodDeclarationSyntax normalMethod && ((PredefinedTypeSyntax)normalMethod.ReturnType).Keyword.ValueText != "void")
                return $"""
                                    return {method.ExpressionBody.Expression.ToFullString()};

                        """;
            else
                return $"""
                                    {method.ExpressionBody.Expression.ToFullString()};

                        """;
        }
        
        return string.Empty;
    }

    private static unsafe string ReplaceGeneric(string source, string[] parameters, string[] arguments) {
        if (parameters.Length == 0)
            return source;

        List<(int index, int paraIndex)> replaceIndexList = new();

        for (int i = 0; i < parameters.Length; i++) {
            int state = 1;
            int index = 0;
            for (int j = 0; j < source.Length; j++)
                switch (state) {
                    case 0:
                        if (!char.IsLetterOrDigit(source[j]))
                            state = 1;
                        break;
                    case 1:
                        if (source[j] == parameters[i][index]) {
                            index++;
                            if (index == parameters[i].Length) {
                                index = 0;
                                state = 2;
                            }
                            break;
                        }
                        else {
                            index = 0;
                            state = 0;
                            goto case 0;
                        }
                    case 2:
                        if (!char.IsLetterOrDigit(source[j])) {
                            replaceIndexList.Add((j - parameters[i].Length, i));
                            state = 1;
                        }
                        else
                            state = 0;
                        break;
                }
        }

        if (replaceIndexList.Count == 0)
            return source;
        replaceIndexList.Sort(((int index, int paraIndex) a, (int index, int paraIndex) b) => a.index.CompareTo(b.index));


        int extraSpace = 0;
        foreach ((_, int paraIndex) in replaceIndexList)
            extraSpace += (arguments[paraIndex].Length - parameters[paraIndex].Length);

        string result = new('\0', source.Length + extraSpace);
        int currentIndex = 0;
        fixed (char* destinationPtr = result) {
            fixed (char* sourcePtr = source) {
                char* d = destinationPtr;
                char* s = sourcePtr;

                foreach ((int index, int paraIndex) in replaceIndexList) {
                    string parameter = parameters[paraIndex];
                    string argument = arguments[paraIndex];

                    int length = index - currentIndex;
                    int byteLength = length * sizeof(char);
                    Buffer.MemoryCopy(s, d, byteLength, byteLength);
                    d += length;
                    s += length;

                    fixed (char* a = argument) {
                        int byteCount = argument.Length * sizeof(char);
                        Buffer.MemoryCopy(a, d, byteCount, byteCount);
                    }
                    d += argument.Length;
                    s += parameter.Length;

                    currentIndex = index + parameter.Length;
                }
                {
                    int byteCount = (source.Length - currentIndex) * sizeof(char);
                    Buffer.MemoryCopy(s, d, byteCount, byteCount);
                    Buffer.MemoryCopy(s, d, byteCount, byteCount);
                }
            }
        }

        return result;
    }
}
