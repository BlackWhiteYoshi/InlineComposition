using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace InlineComposition;

[Generator(LanguageNames.CSharp)]
public sealed class InlineCompositionGenerator : IIncrementalGenerator {
    private readonly ObjectPool<StringBuilder> stringBuilderPool = new DefaultObjectPoolProvider().CreateStringBuilderPool(initialCapacity: 8192, maximumRetainedCapacity: 1024 * 1024);


    public void Initialize(IncrementalGeneratorInitializationContext context) {
        // register attribute marker
        context.RegisterPostInitializationOutput(static (IncrementalGeneratorPostInitializationContext context) => {
            context.AddSource("InlineBaseAttribute.g.cs", Attributes.InlineBaseAttribute);
            context.AddSource("NoInlineAttribute.g.cs", Attributes.NoInlineAttribute);
            context.AddSource("InlineAttribute.g.cs", Attributes.InlineAttribute);
            context.AddSource("InlineMethodAttribute.g.cs", Attributes.InlineMethodAttribute);
            context.AddSource("InlineConstructorAttribute.g.cs", Attributes.InlineConstructorAttribute);
            context.AddSource("InlineFinalizerAttribute.g.cs", Attributes.InlineFinalizerAttribute);
        });

        RegisterProvider(context, "InlineCompositionAttributes.InlineAttribute`1");
        RegisterProvider(context, "InlineCompositionAttributes.InlineAttribute`2");
        RegisterProvider(context, "InlineCompositionAttributes.InlineAttribute`3");
        RegisterProvider(context, "InlineCompositionAttributes.InlineAttribute`4");
        RegisterProvider(context, "InlineCompositionAttributes.InlineAttribute`5");
        RegisterProvider(context, "InlineCompositionAttributes.InlineAttribute`6");
        RegisterProvider(context, "InlineCompositionAttributes.InlineAttribute`7");
        RegisterProvider(context, "InlineCompositionAttributes.InlineAttribute`8");
        RegisterProvider(context, "InlineCompositionAttributes.InlineAttribute`9");
        RegisterProvider(context, "InlineCompositionAttributes.InlineAttribute`10");
        RegisterProvider(context, "InlineCompositionAttributes.InlineAttribute`11");
        RegisterProvider(context, "InlineCompositionAttributes.InlineAttribute`12");

        void RegisterProvider(IncrementalGeneratorInitializationContext context, string inlineAttributeName) {
            IncrementalValuesProvider<AttributeCollection> inlineProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                inlineAttributeName,
                static (SyntaxNode syntaxNode, CancellationToken _) => syntaxNode is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax,
                static AttributeCollection (GeneratorAttributeSyntaxContext syntaxContext, CancellationToken cancellationToken) => {
                    Debug.Assert(syntaxContext.Attributes.Length > 0);
                    Debug.Assert(syntaxContext.Attributes[0].AttributeClass is not null);

                    TypeDeclarationSyntax inlineClassSyntax = (TypeDeclarationSyntax)syntaxContext.TargetNode;
                    INamedTypeSymbol inlineClassSymbol = (INamedTypeSymbol)syntaxContext.TargetSymbol;
                    AttributeData inlineAttribute = syntaxContext.Attributes[0];

                    TypeDeclarationSyntax?[] baseClassArray = new TypeDeclarationSyntax[inlineAttribute.AttributeClass!.TypeArguments.Length];
                    AttributeData?[] baseAttributeArray = new AttributeData[inlineAttribute.AttributeClass!.TypeArguments.Length];
                    for (int i = 0; i < inlineAttribute.AttributeClass!.TypeArguments.Length; i++) {
                        ITypeSymbol baseClass = inlineAttribute.AttributeClass!.TypeArguments[i];
                        if (baseClass.DeclaringSyntaxReferences is [SyntaxReference syntaxReference, ..] && syntaxReference.GetSyntax() is TypeDeclarationSyntax baseClassSyntax) {
                            baseClassArray[i] = baseClassSyntax;
                            baseAttributeArray[i] = baseClass.GetAttribute("InlineBaseAttribute", ["InlineCompositionAttributes"]);
                        }
                    }

                    return new AttributeCollection(inlineClassSyntax, inlineAttribute, baseClassArray, baseAttributeArray);
                }
            );

            context.RegisterSourceOutput(inlineProvider, Execute);
        }
    }

    private void Execute(SourceProductionContext context, AttributeCollection attributeProvider) {
        TypeDeclarationSyntax inlineClass = attributeProvider.inlineClass;
        AttributeData inlineAttribute = attributeProvider.inlineAttribute;
        ImmutableArray<TypeDeclarationSyntax?> baseClasses = attributeProvider.baseClasses;
        ImmutableArray<AttributeData?> baseAttributes = attributeProvider.baseAttributes;

        string inlineClassName = inlineClass.Identifier.ValueText;

        List<string> usingStatementList = [];
        List<string> attributeList = [];
        List<string> primaryArgumentsList = [];
        List<string> baseList = [];
        Dictionary<string, string> typeList = [];
        Dictionary<string, string> fieldList = [];
        Dictionary<string, string> propertyList = [];
        Dictionary<string, string> eventList = [];
        Dictionary<string, MethodEntry> methodList = [];


        // usings of inlineClass
        {
            BaseNamespaceDeclarationSyntax? namspace = inlineClass.GetParent<BaseNamespaceDeclarationSyntax>();
            while (namspace != null) {
                foreach (UsingDirectiveSyntax usingSyntax in namspace.Usings)
                    if (usingSyntax.Name != null)
                        usingStatementList.Add(usingSyntax.Name.ToFullString());

                namspace = namspace.GetParent<BaseNamespaceDeclarationSyntax>();
            }

            CompilationUnitSyntax? compilationUnit = inlineClass.GetParent<CompilationUnitSyntax>();
            if (compilationUnit != null)
                foreach (UsingDirectiveSyntax usingSyntax in compilationUnit.Usings)
                    if (usingSyntax.Name != null)
                        usingStatementList.Add(usingSyntax.Name.ToFullString());
        }

        if (context.CancellationToken.IsCancellationRequested)
            return;

        // InlineMethods
        foreach (MemberDeclarationSyntax node in inlineClass.Members) {
            AttributeSyntax? inlineMethodAttribute = node.GetAttribute("InlineMethod", "InlineMethodAttribute");
            if (inlineMethodAttribute?.ArgumentList is AttributeArgumentListSyntax attributeArgumentList) {
                foreach (AttributeArgumentSyntax attributeArgument in attributeArgumentList.Arguments)
                    if (attributeArgument.NameEquals?.Name.Identifier.ValueText == "MethodName") {
                        if (attributeArgument.Expression is LiteralExpressionSyntax literal) {
                            string name = literal.Token.ValueText;
                            BaseMethodDeclarationSyntax methodDeclaration = (BaseMethodDeclarationSyntax)node;
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
                    BaseMethodDeclarationSyntax derivedConstructorMethod = (BaseMethodDeclarationSyntax)node;
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

            static void CaseCore(string name, string methodName, Action<BaseMethodDeclarationSyntax, List<string>, string, string?, string[], string[]> AddHeadMethod, AttributeSyntax attribute, Dictionary<string, MethodEntry> methodList, string? modifiers = null) {
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

                BaseMethodDeclarationSyntax method = (BaseMethodDeclarationSyntax)attribute.Parent!.Parent!;
                MethodEntry entry = new();

                AddHeadMethod(method, entry.headList, name, modifiers, [], []);

                if (first) {
                    string? bodySource = GetMethodBody(method);
                    if (bodySource != null)
                        entry.blockList.Add(bodySource);
                }
                else
                    entry.lastBlock = GetMethodBody(method);

                methodList.Add(methodName, entry);
            }
        }

        if (context.CancellationToken.IsCancellationRequested)
            return;

        BaseClassNode[] baseClassNodes = new BaseClassNode[baseClasses.Length];
        for (int i = 0; i < baseClassNodes.Length; i++) {
            if (baseClasses[i] is not TypeDeclarationSyntax baseClass)
                continue;

            baseClassNodes[i].baseClass = baseClass;
            if (baseAttributes[i] is AttributeData attribute && attribute.NamedArguments.Length > 0) {
                if (attribute.NamedArguments.GetArgument<bool?>("MapBaseType") is bool mapBaseType)
                    baseClassNodes[i].mapBaseType = mapBaseType;
                if (attribute.NamedArguments.GetArgument<bool?>("IgnoreInheritenceAndImplements") is bool ignoreInheritenceAndImplements)
                    baseClassNodes[i].ignoreInheritenceAndImplements = ignoreInheritenceAndImplements;
                if (attribute.NamedArguments.GetArgument<bool?>("InlineAttributes") is bool inlineAttributes)
                    baseClassNodes[i].inlineAttributes = inlineAttributes;
            }

            // extract generic arguments (prepend mapping to baseType)
            if (inlineAttribute.AttributeClass!.TypeArguments[i] is INamedTypeSymbol { TypeArguments: ImmutableArray<ITypeSymbol> { Length: > 0 } typeArguments }) {
                int offset;
                if (baseClassNodes[i].mapBaseType) {
                    offset = 1;
                    baseClassNodes[i].genericArguments = new string[typeArguments.Length + 1];
                    baseClassNodes[i].genericArguments[0] = $"{inlineClass.Identifier.ValueText}{inlineClass.TypeParameterList?.ToString()}";
                }
                else {
                    offset = 0;
                    baseClassNodes[i].genericArguments = new string[typeArguments.Length];
                }

                for (int j = 0; j < typeArguments.Length; j++)
                    baseClassNodes[i].genericArguments[j + offset] = (typeArguments[j] as INamedTypeSymbol)?.Name ?? string.Empty;
            }
            else if (baseClassNodes[i].mapBaseType)
                baseClassNodes[i].genericArguments = [$"{inlineClass.Identifier.ValueText}{inlineClass.TypeParameterList?.ToString()}"];
        }

        if (context.CancellationToken.IsCancellationRequested)
            return;

        foreach (BaseClassNode baseClassNode in baseClassNodes) {
            TypeDeclarationSyntax? classType = baseClassNode.baseClass;
            if (classType == null)
                continue;

            // generic parameters
            string[] genericParameters;
            switch ((baseClassNode.mapBaseType, classType.TypeParameterList)) {
                case (true, not null): {
                    SeparatedSyntaxList<TypeParameterSyntax> genericParameterList = classType.TypeParameterList.Parameters;
                    genericParameters = new string[genericParameterList.Count + 1];
                    genericParameters[0] = $"{classType.Identifier.ValueText}{classType.TypeParameterList?.ToString()}";
                    for (int i = 0; i < genericParameterList.Count; i++) {
                        genericParameters[i + 1] = genericParameterList[i].Identifier.ValueText;
                    }
                    break;
                }
                case (false, not null): {
                    SeparatedSyntaxList<TypeParameterSyntax> genericParameterList = classType.TypeParameterList.Parameters;
                    genericParameters = new string[genericParameterList.Count];
                    for (int i = 0; i < genericParameterList.Count; i++) {
                        genericParameters[i] = genericParameterList[i].Identifier.ValueText;
                    }
                    break;
                }
                case (true, null): {
                    genericParameters = [classType.Identifier.ValueText];
                    break;
                }
                case (false, null): {
                    genericParameters = [];
                    break;
                }
            }

            // usings
            {
                BaseNamespaceDeclarationSyntax? namspace = classType.GetParent<BaseNamespaceDeclarationSyntax>();
                while (namspace != null) {
                    foreach (UsingDirectiveSyntax usingSyntax in namspace.Usings)
                        if (usingSyntax.Name != null)
                            usingStatementList.Add(usingSyntax.Name.ToFullString());

                    namspace = namspace.GetParent<BaseNamespaceDeclarationSyntax>();
                }

                CompilationUnitSyntax? compilationUnit = classType.GetParent<CompilationUnitSyntax>();
                if (compilationUnit != null)
                    foreach (UsingDirectiveSyntax usingSyntax in compilationUnit.Usings)
                        if (usingSyntax.Name != null)
                            usingStatementList.Add(usingSyntax.Name.ToFullString());
            }

            // attributes
            if (baseClassNode.inlineAttributes)
                foreach (AttributeListSyntax attributeListSyntax in classType.AttributeLists)
                    if (attributeListSyntax.Attributes is [AttributeSyntax { Name: not IdentifierNameSyntax { Identifier.ValueText: "InlineBase" or "InlineBaseAttribute" } }, ..])
                        attributeList.Add(attributeListSyntax.ToString());

            // primary constructor parameters
            if (classType.ParameterList != null)
                foreach (ParameterSyntax parameter in classType.ParameterList.Parameters)
                    primaryArgumentsList.Add(ReplaceGeneric(parameter.ToString(), genericParameters, baseClassNode.genericArguments));

            // baseclasses and interfaces
            if (!baseClassNode.ignoreInheritenceAndImplements && classType.BaseList != null)
                foreach (BaseTypeSyntax baseTypeSyntax in classType.BaseList.Types)
                    baseList.Add(ReplaceGeneric(baseTypeSyntax.ToString(), genericParameters, baseClassNode.genericArguments));

            // members
            foreach (MemberDeclarationSyntax node in classType.Members) {
                if (node.GetAttribute("NoInline", "NoInlineAttribute") != null)
                    continue;

                switch (node) {
                    case TypeDeclarationSyntax typeDeclarationSyntax: {
                        string name = typeDeclarationSyntax.Identifier.ValueText;

                        if (!typeList.ContainsKey(name)) {
                            string source = ReplaceGeneric(typeDeclarationSyntax.ToFullString(), genericParameters, baseClassNode.genericArguments);
                            typeList.Add(name, source);
                        }
                        break;
                    }
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
                        if (constructorDeclarationSyntax.Modifiers.Any((SyntaxToken modifier) => modifier.ValueText == "static"))
                            break;

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

                    case OperatorDeclarationSyntax operatorDeclarationSyntax: {
                        string name = $"operator {operatorDeclarationSyntax.OperatorToken.ValueText}";
                        string methodName = CreateMethodName(name, operatorDeclarationSyntax.ParameterList);
                        CaseMethod(name, methodName, operatorDeclarationSyntax, methodList, genericParameters, baseClassNode.genericArguments);
                        break;
                    }

                    static void CaseMethod(string name, string methodName, BaseMethodDeclarationSyntax method, Dictionary<string, MethodEntry> methodList, string[] genericParameters, string[] genericArguments) {
                        if (!methodList.TryGetValue(methodName, out MethodEntry entry)) {
                            entry = new MethodEntry();
                            if (method is MethodDeclarationSyntax or OperatorDeclarationSyntax)
                                AddMethodHead(method, entry.headList, name, null, genericParameters, genericArguments);
                            else
                                AddConDestructorHead(method, entry.headList, name, null, genericParameters, genericArguments);

                            methodList.Add(methodName, entry);
                        }

                        string? rawSource = GetMethodBody(method);
                        if (rawSource != null) {
                            string source = ReplaceGeneric(rawSource, genericParameters, genericArguments);
                            entry.blockList.Add(source);
                        }
                    }
                }
            }
        }


        if (context.CancellationToken.IsCancellationRequested)
            return;


        #region writing SourceCode

        StringBuilder builder = stringBuilderPool.Get();

        builder.Append("""
            // <auto-generated/>
            #pragma warning disable
            #nullable enable annotations



            """);

        // usingStatements
        if (usingStatementList.Count > 0) {
            foreach (string usingStatement in usingStatementList.Distinct())
                builder.AppendInterpolation($"using {usingStatement};\n");
            builder.Append('\n');
        }

        // namespace
        string classNamespace;
        {
            BaseNamespaceDeclarationSyntax? namespaceSyntax = inlineClass.GetParent<BaseNamespaceDeclarationSyntax>();
            if (namespaceSyntax != null) {
                int startIndex = builder.Length + 10; // 10 = "namespace ".Length
                builder.AppendInterpolation($"namespace {namespaceSyntax.AsNamespace()};\n\n");
                classNamespace = builder.ToString(startIndex, builder.Length - 3 - startIndex); // 3 = ";\n\n".Length
            }
            else
                classNamespace = string.Empty;
        }

        // attributes
        foreach (string attribute in attributeList)
            builder.AppendInterpolation($"{attribute}\n");

        // class/struct declaration
        foreach (SyntaxToken token in inlineClass.Modifiers)
            builder.AppendInterpolation($"{token.ValueText} ");

        builder.AppendInterpolation($"{inlineClass.Keyword.ValueText} ");

        if (inlineClass is RecordDeclarationSyntax recordDeclarationSyntax && recordDeclarationSyntax.ClassOrStructKeyword.ValueText is string { Length: > 0 } classStructKeyword)
            builder.AppendInterpolation($"{classStructKeyword} ");

        builder.AppendInterpolation($"{inlineClassName}{inlineClass.TypeParameterList}");

        if (primaryArgumentsList.Count > 0) {
            builder.Append('(');

            foreach (string argument in primaryArgumentsList.Distinct())
                builder.AppendInterpolation($"{argument}, ");
            builder.Length -= 2;

            builder.Append(')');
        }

        if (baseList.Count > 0) {
            builder.Append(" :");
            foreach (string baseIdentifier in baseList.Distinct())
                builder.AppendInterpolation($" {baseIdentifier},");
            builder.Length--;
        }

        builder.Append(" {\n");

        // body
        foreach (KeyValuePair<string, string> type in typeList)
            builder.AppendInterpolation($"{type.Value}\n");
        foreach (KeyValuePair<string, string> pair in fieldList)
            builder.AppendInterpolation($"{pair.Value}\n");
        foreach (KeyValuePair<string, string> pair in propertyList)
            builder.AppendInterpolation($"{pair.Value}\n");
        foreach (KeyValuePair<string, string> pair in eventList)
            builder.AppendInterpolation($"{pair.Value}\n");
        foreach (KeyValuePair<string, MethodEntry> pair in methodList) {
            foreach (string chunk in pair.Value.headList)
                builder.Append(chunk);

            if (pair.Value.blockList.Count > 0 || pair.Value.lastBlock != null) {
                // open method
                builder.Append("{\n");

                foreach (string block in pair.Value.blockList)
                    builder.AppendInterpolation($"        {{\n{block}        }}\n");
                if (pair.Value.lastBlock != null)
                    builder.AppendInterpolation($"        {{\n{pair.Value.lastBlock}        }}\n");

                // close method
                builder.Append("    }");
            }
            else
                // replace space with ';'
                builder[^1] = ';';

            builder.Append("\n\n");
        }

        // close class/struct
        builder.Append("}\n");

        string sourceCode = builder.ToString();


        string hintName = classNamespace switch {
            string { Length: > 0 } => $"{classNamespace}.{inlineClassName}.g.cs",
            _ => $"{inlineClassName}.g.cs"
        };

        context.AddSource(hintName, sourceCode);


        stringBuilderPool.Return(builder);

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
    private static string CreateMethodName(string functionName, ParameterListSyntax parameterList) {
        if (parameterList.Parameters.Count == 0)
            return functionName;

        string[] parameters = new string[parameterList.Parameters.Count];
        for (int i = 0; i < parameterList.Parameters.Count; i++)
            parameters[i] = parameterList.Parameters[i].Type!.ToString();

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

    private static void AddMethodHead(BaseMethodDeclarationSyntax method, List<string> list, string name, string? modifiers, string[] genericParameters, string[] genericArguments) {
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

        (TypeSyntax? returnType, ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier, TypeParameterListSyntax? typeParameterList) = method switch {
            MethodDeclarationSyntax m => (m.ReturnType, m.ExplicitInterfaceSpecifier, m.TypeParameterList),
            OperatorDeclarationSyntax o => (o.ReturnType, o.ExplicitInterfaceSpecifier, null),
            _ => throw new Exception($"Unreachable code: parameter {nameof(method)} must be {nameof(MethodDeclarationSyntax)} or {nameof(OperatorDeclarationSyntax)}")
        };
        list.Add(ReplaceGeneric(returnType.ToString(), genericParameters, genericArguments));
        list.Add(" ");
        if (explicitInterfaceSpecifier != null) {
            list.Add(((IdentifierNameSyntax)explicitInterfaceSpecifier.Name).Identifier.ValueText);
            list.Add(".");
        }
        list.Add(name);

        if (typeParameterList != null)
            list.Add(ReplaceGeneric(typeParameterList.ToString(), genericParameters, genericArguments));
        list.Add(ReplaceGeneric(method.ParameterList.ToString(), genericParameters, genericArguments));
        list.Add(" ");
    }

    private static void AddConDestructorHead(BaseMethodDeclarationSyntax method, List<string> list, string name, string? modifiers, string[] genericParameters, string[] genericArguments) {
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
        list.Add(ReplaceGeneric(method.ParameterList.ToString(), genericParameters, genericArguments));

        if (method is ConstructorDeclarationSyntax { Initializer: not null } constructor) {
            list.Add(" : ");
            list.Add(constructor.Initializer.ThisOrBaseKeyword.ValueText);
            list.Add(constructor.Initializer.ArgumentList.ToString());
        }

        list.Add(" ");
    }

    private static string? GetMethodBody(BaseMethodDeclarationSyntax method) {
        if (method.Body != null)
            return method.Body.Statements.ToFullString();

        if (method.ExpressionBody != null)
            if (method is MethodDeclarationSyntax normalMethod && normalMethod.ReturnType.ToString() != "void"
                || method is OperatorDeclarationSyntax operatorMethod && operatorMethod.ReturnType.ToString() != "void")
                return $"""
                                    return {method.ExpressionBody.Expression.ToFullString()};

                        """;
            else
                return $"""
                                    {method.ExpressionBody.Expression.ToFullString()};

                        """;

        return null;
    }

    private static unsafe string ReplaceGeneric(string source, string[] parameters, string[] arguments) {
        if (parameters.Length == 0)
            return source;

        List<(int index, int paraIndex)> replaceIndexList = [];

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

            if (state == 2)
                replaceIndexList.Add((source.Length - parameters[i].Length, i));
        }

        if (replaceIndexList.Count == 0)
            return source;
        replaceIndexList.Sort(((int index, int paraIndex) a, (int index, int paraIndex) b) => a.index.CompareTo(b.index));


        int extraSpace = 0;
        {
            int lastIndex = 0;
            for (int i = 0; i < replaceIndexList.Count; i++) {
                int index = replaceIndexList[i].index;
                int paraIndex = replaceIndexList[i].paraIndex;

                // remove overlapping replacements
                if (index < lastIndex) {
                    replaceIndexList.RemoveAt(i--);
                    continue;
                }

                lastIndex = index + parameters[paraIndex].Length;
                extraSpace += arguments[paraIndex].Length - parameters[paraIndex].Length;
            }
        }

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
