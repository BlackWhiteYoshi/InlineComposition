﻿using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace InlineComposition.Tests;

/// <summary>
/// nested namespace/usings, InlineBase missing, NoInlineAttribute
/// </summary>
public sealed class SpecialTests {
    [Test]
    public async ValueTask NestedNamespaceAndNestedUsings() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode {
                using A;
                namespace Nested {
                    using nested.A;

                    [Inline<Test>]
                    public sealed partial class Derived;

                    [InlineBase]
                    public sealed class Base;
                }
            }

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = """
            // <auto-generated/>
            #pragma warning disable
            #nullable enable annotations


            using nested.A;
            using A;
            using InlineCompositionAttributes;

            namespace MyCode.Nested;

            public sealed partial class Derived {
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_WithoutInlineBase_DoesNotWork() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            [Inline<Test>]
            public sealed partial class Derived;

            public sealed class Test {
                public int myField = 5;
            }

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
    public async ValueTask NoInline() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            [InlineBase]
            public sealed class Test {
                public int myField = 5;

                [NoInline]
                public void MyMethod() {
                    int ab = 5;
                }
            }

            [Inline<Test>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                public int myField = 5;

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }


    [Test]
    public async ValueTask EverythingCombined() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            [InlineBase]
            public sealed class TestGeneric<T> {
                public int myField = 5;

                private string asdf;

                public event Action<T> A;

                public bool A { get; private set; }

                public Test() {
                    int ab = 17;
                }

                ~Test() {
                    string qw = "y";
                }

                public void MethodTest(string key, int value) {
                    int methodTest = 1;
                }

                [NoInline]
                private void PrivateMethod() { }
            }

            [InlineBase]
            public sealed class Test2 {
                public int myField = 5;

                public void MethodTest(string key, int value) {
                    int secondMethodTest = 1;
                }
            }

            [Inline<TestGeneric<nint>>]
            public sealed partial class Test {
                [InlineMethod(MethodName = "MethodTest", Modifiers = "public")]
                private void MethodTestPartial(string key, int value) {
                    int methodTestPartial = 3;
                }

                [InlineConstructor(Modifiers = "public", First = true)]
                private void A() {
                    bool a = true;
                }

                [InlineFinalizer(First = true)]
                private void B() {
                    bool b = false;
                }
            }

            [Inline<TestGeneric<string>, Test2>]
            public partial struct Derived;

            """;

        string[] sourceTexts = Shared.GenerateSourceText(input, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics);

        await Assert.That(diagnostics.IsEmpty).IsTrue();
        await Assert.That(outputCompilation).IsNotNull();


        (string sourceText1, string sourceText2) = (sourceTexts[^2], sourceTexts[^1]);

        const string expected1 = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Test {
                public int myField = 5;


                private string asdf;


                public bool A { get; private set; }


                public event Action<nint> A;

                [InlineMethod(MethodName = "MethodTest", Modifiers = "public")]
                public void MethodTest(string key, int value) {
                    {
                    int methodTest = 1;
                    }
                    {
                    int methodTestPartial = 3;
                    }
                }


                [InlineConstructor(Modifiers = "public", First = true)]
                public Test() {
                    {
                    bool a = true;
                    }
                    {
                    int ab = 17;
                    }
                }


                [InlineFinalizer(First = true)]
                ~Test() {
                    {
                    bool b = false;
                    }
                    {
                    string qw = "y";
                    }
                }

            }

            """;
        await Assert.That(sourceText1).IsEqualTo(expected1);

        const string expected2 = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public partial struct Derived {
                public int myField = 5;


                private string asdf;


                public bool A { get; private set; }


                public event Action<string> A;

                public Derived() {
                    {
                    int ab = 17;
                    }
                }

                ~Derived() {
                    {
                    string qw = "y";
                    }
                }

                public void MethodTest(string key, int value) {
                    {
                    int methodTest = 1;
                    }
                    {
                    int secondMethodTest = 1;
                    }
                }

            }

            """;
        await Assert.That(sourceText2).IsEqualTo(expected2);
    }
}
