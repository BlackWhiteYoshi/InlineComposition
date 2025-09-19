namespace InlineComposition.Tests;

/// <summary>
/// Method, Constructor, Finalizer
/// </summary>
public sealed class MethodTests {
    [Test]
    public async ValueTask Inline_Method() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public void MyMethod() {
                    int ab = 17;
                }

                public int MethodExpression() => 23;

                public void MethodVoidExpression() => System.Console.WriteLine();

                public void IInterface.Something() => System.Console.WriteLine("");
            }

            [Inline<Test>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                public void MyMethod() {
                    {
                    int ab = 17;
                    }
                }

                public int MethodExpression() {
                    {
                        return 23;
                    }
                }

                public void MethodVoidExpression() {
                    {
                        System.Console.WriteLine();
                    }
                }

                public void IInterface.Something() {
                    {
                        System.Console.WriteLine("");
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_Static_Method() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public static void StaticMethod() {
                    int ab = 17;
                }
            }

            [Inline<Test>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                public static void StaticMethod() {
                    {
                    int ab = 17;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_Operator() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public static bool operator |(Test a, Test b) {
                    return true;
                }
            }

            [Inline<Test>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                public static bool operator |(Test a, Test b) {
                    {
                    return true;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_Extern_Method() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_internString")]
                private extern static ref string GetString(Test @this);
            }

            [Inline<Test>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_internString")]
                private extern static ref string GetString(Test @this);

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_MethodMerge() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public void MyMethod() {
                    int ab = 17;
                }

                private void ExpressionMethod() => System.Console.WriteLine("Base");

                public static void StaticMethod() => System.Console.WriteLine("Base");
            }

            [Inline<Test>]
            public sealed partial class Derived {
                [InlineMethod(MethodName = "MyMethod", Modifiers = "public")]
                private void MyMethodPartial() {
                    int methodTestPartial = 3;
                }

                [InlineMethod(MethodName = "ExpressionMethod", Modifiers = "public")]
                private void ExpressionMethodPartial() => System.Console.WriteLine("Derived");

                [InlineMethod(MethodName = "StaticMethod")]
                public static void StaticMethodPartial() => System.Console.WriteLine("Derived");
            }

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                [InlineMethod(MethodName = "MyMethod", Modifiers = "public")]
                public void MyMethod() {
                    {
                    int ab = 17;
                    }
                    {
                    int methodTestPartial = 3;
                    }
                }


                [InlineMethod(MethodName = "ExpressionMethod", Modifiers = "public")]
                public void ExpressionMethod() {
                    {
                        System.Console.WriteLine("Base");
                    }
                    {
                        System.Console.WriteLine("Derived");
                    }
                }


                [InlineMethod(MethodName = "StaticMethod")]
                public static void StaticMethod() {
                    {
                        System.Console.WriteLine("Base");
                    }
                    {
                        System.Console.WriteLine("Derived");
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_MethodOperatorMerge() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public static bool operator |() => true;
            }

            [Inline<Test>]
            public sealed partial class Derived {
                [InlineMethod(MethodName = "operator |", Modifiers = "public static")]
                private static bool BarPartial() => false;
            }

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                [InlineMethod(MethodName = "operator |", Modifiers = "public static")]
                public static bool operator |() {
                    {
                        return true;
                    }
                    {
                        return false;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_MethodMergePrepend() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public void MyMethod() {
                    int ab = 17;
                }
            }

            [Inline<Test>]
            public sealed partial class Derived {
                [InlineMethod(MethodName = "MyMethod", Modifiers = "public", First = true)]
                private void MyMethodPartial() {
                    int methodTestPartial = 3;
                }
            }

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                [InlineMethod(MethodName = "MyMethod", Modifiers = "public", First = true)]
                public void MyMethod() {
                    {
                    int methodTestPartial = 3;
                    }
                    {
                    int ab = 17;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_Method_ParameterAnyOrder() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public void MyMethod1() {
                    int ab = 1;
                }

                public void MyMethod2() {
                    int ab = 2;
                }

                public void MyMethod3() {
                    int ab = 3;
                }

                public void MyMethod4() {
                    int ab = 4;
                }

                public void MyMethod5() {
                    int ab = 5;
                }

                public void MyMethod6() {
                    int ab = 6;
                }
            }

            [Inline<Test>]
            public sealed partial class Derived {
                [InlineMethod(MethodName = "MyMethod1", Modifiers = "public", First = true)]
                private void MyMethodPartial1() {
                    int methodTestPartial = 1;
                }

                [InlineMethod(MethodName = "MyMethod2", First = true, Modifiers = "public")]
                private void MyMethodPartial2() {
                    int methodTestPartial = 2;
                }

                [InlineMethod(Modifiers = "public", MethodName = "MyMethod3", First = true)]
                private void MyMethodPartial3() {
                    int methodTestPartial = 3;
                }

                [InlineMethod(Modifiers = "public", First = true, MethodName = "MyMethod4")]
                private void MyMethodPartial4() {
                    int methodTestPartial = 4;
                }

                [InlineMethod(First = true, MethodName = "MyMethod5", Modifiers = "public")]
                private void MyMethodPartial5() {
                    int methodTestPartial = 5;
                }

                [InlineMethod(First = true, Modifiers = "public", MethodName = "MyMethod6")]
                private void MyMethodPartial6() {
                    int methodTestPartial = 6;
                }
            }

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                [InlineMethod(MethodName = "MyMethod1", Modifiers = "public", First = true)]
                public void MyMethod1() {
                    {
                    int methodTestPartial = 1;
                    }
                    {
                    int ab = 1;
                    }
                }


                [InlineMethod(MethodName = "MyMethod2", First = true, Modifiers = "public")]
                public void MyMethod2() {
                    {
                    int methodTestPartial = 2;
                    }
                    {
                    int ab = 2;
                    }
                }


                [InlineMethod(Modifiers = "public", MethodName = "MyMethod3", First = true)]
                public void MyMethod3() {
                    {
                    int methodTestPartial = 3;
                    }
                    {
                    int ab = 3;
                    }
                }


                [InlineMethod(Modifiers = "public", First = true, MethodName = "MyMethod4")]
                public void MyMethod4() {
                    {
                    int methodTestPartial = 4;
                    }
                    {
                    int ab = 4;
                    }
                }


                [InlineMethod(First = true, MethodName = "MyMethod5", Modifiers = "public")]
                public void MyMethod5() {
                    {
                    int methodTestPartial = 5;
                    }
                    {
                    int ab = 5;
                    }
                }


                [InlineMethod(First = true, Modifiers = "public", MethodName = "MyMethod6")]
                public void MyMethod6() {
                    {
                    int methodTestPartial = 6;
                    }
                    {
                    int ab = 6;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_MethodOverload_DoesNotMerge() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public void MyMethod() {
                    int ab = 17;
                }
            }

            public sealed class Test2 {
                public void MyMethod(int q) {
                    int ab = 17;
                }
            }

            [Inline<Test, Test2>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                public void MyMethod() {
                    {
                    int ab = 17;
                    }
                }

                public void MyMethod(int q) {
                    {
                    int ab = 17;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }


    [Test]
    public async ValueTask Inline_Constructor() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public Test() {
                    int ab = 17;
                }
            }

            [Inline<Test>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                public Derived() {
                    {
                    int ab = 17;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_ConstructorWithThisCall() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public Test() {
                    int ab = 17;
                }

                public Test(string message) : this() {
                    string myMessage = message;
                }
            }

            [Inline<Test>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                public Derived() {
                    {
                    int ab = 17;
                    }
                }

                public Derived(string message) : this() {
                    {
                    string myMessage = message;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_ConstructorWithBaseCall() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public Test() {
                    int ab = 17;
                }

                public Test(string message) : base() {
                    string myMessage = message;
                }
            }

            [Inline<Test>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                public Derived() {
                    {
                    int ab = 17;
                    }
                }

                public Derived(string message) : base() {
                    {
                    string myMessage = message;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_ConstructorMerge() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public Test() {
                    int ab = 17;
                }
            }

            [Inline<Test>]
            public sealed partial class Derived {
                [InlineConstructor(Modifiers = "public")]
                private void Constructor() {
                    int constructorTestPartial = 3;
                }
            }

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                [InlineConstructor(Modifiers = "public")]
                public Derived() {
                    {
                    int ab = 17;
                    }
                    {
                    int constructorTestPartial = 3;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_ConstructorMergePrepend() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            [InlineBase(IgnoreInheritenceAndImplements = true)]
            public sealed class Test {
                public Test() {
                    int ab = 17;
                }
            }

            [Inline<Test>]
            public sealed partial class Derived {
                [InlineConstructor(Modifiers = "public", First = true)]
                private void Constructor() {
                    int constructorTestPartial = 3;
                }
            }

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                [InlineConstructor(Modifiers = "public", First = true)]
                public Derived() {
                    {
                    int constructorTestPartial = 3;
                    }
                    {
                    int ab = 17;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_Constructor_ParameterOtherOrder() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            [InlineBase(IgnoreInheritenceAndImplements = true)]
            public sealed class Test {
                public Test() {
                    int ab = 17;
                }
            }

            [Inline<Test>]
            public sealed partial class Derived {
                [InlineConstructor(First = true, Modifiers = "public")]
                private void Constructor() {
                    int constructorTestPartial = 3;
                }
            }

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                [InlineConstructor(First = true, Modifiers = "public")]
                public Derived() {
                    {
                    int constructorTestPartial = 3;
                    }
                    {
                    int ab = 17;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_ConstructorOverload_DoesNotMerge() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public Test() {
                    int ab = 17;
                }
            }

            public sealed class Test2 {
                public Test(int q) {
                    int ab = 17;
                }
            }

            [Inline<Test, Test2>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                public Derived() {
                    {
                    int ab = 17;
                    }
                }

                public Derived(int q) {
                    {
                    int ab = 17;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_StaticConstructor_IsIgnored() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                static Test() { }
            }

            [Inline<Test>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_PrimaryConstructor() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test(int prime);

            [Inline<Test>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived(int prime) {
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }


    [Test]
    public async ValueTask Inline_Finalizer() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                ~Test() {
                    int ab = 17;
                }
            }

            [Inline<Test>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                ~Derived() {
                    {
                    int ab = 17;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_FinalizerMerge() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                ~Test() {
                    int ab = 17;
                }
            }

            [Inline<Test>]
            public sealed partial class Derived {
                [InlineFinalizer]
                private void Finalizer() {
                    int finalizeTestPartial = 3;
                }
            }

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                [InlineFinalizer]
                ~Derived() {
                    {
                    int ab = 17;
                    }
                    {
                    int finalizeTestPartial = 3;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_FinalizerMergePrepend() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                ~Test() {
                    int ab = 17;
                }
            }

            [Inline<Test>]
            public sealed partial class Derived {
                [InlineFinalizer(First = true)]
                private void Constructor() {
                    int finalizeTestPartial = 3;
                }
            }

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                [InlineFinalizer(First = true)]
                ~Derived() {
                    {
                    int finalizeTestPartial = 3;
                    }
                    {
                    int ab = 17;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }
}
