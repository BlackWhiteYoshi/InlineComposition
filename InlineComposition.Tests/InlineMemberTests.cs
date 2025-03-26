namespace InlineComposition.Tests;

/// <summary>
/// type, field, property, event
/// </summary>
public sealed class InlineMemberTests {
    /// <summary>
    /// If nested class/struct/record/record-struct gets inlined
    /// </summary>
    /// <returns></returns>
    [Test]
    public async ValueTask Inline_Type() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            [InlineBase]
            public sealed class Test {
                public struct NestedStruct;

                /// <summary>
                /// asdf
                /// </summary>
                [SomeAttribute]
                private sealed record class Nested {
                    public int Number => 1;
                }
            }

            [Inline<Test>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                public struct NestedStruct;


                /// <summary>
                /// asdf
                /// </summary>
                [SomeAttribute]
                private sealed record class Nested {
                    public int Number => 1;
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_Field() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            [InlineBase]
            public sealed class Test {
                public int myField = 5;

                private string asdf;
            }

            [Inline<Test>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                public int myField = 5;


                private string asdf;

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_Property() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            [InlineBase]
            public sealed class Test {
                public bool A { get; private set; }
            }

            [Inline<Test>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                public bool A { get; private set; }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_Event() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            [InlineBase]
            public sealed class Test {
                public event Action<byte> A;
            }

            [Inline<Test>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                public event Action<byte> A;

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }
}
