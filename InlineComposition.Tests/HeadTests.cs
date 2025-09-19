namespace InlineComposition.Tests;

/// <summary>
/// head -> Comments and BaseClasses
/// </summary>
public sealed class HeadTests {
    [Test]
    public async ValueTask Inline_AttributeAndComment() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                /// <summary>
                /// Test comment with Attribute
                /// </summary>
                [Something]
                public bool A { get; private set; }
            }

            [Inline<Test>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                /// <summary>
                /// Test comment with Attribute
                /// </summary>
                [Something]
                public bool A { get; private set; }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_BaseClassAndInterfaces() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test : MyBase, IA, IB;

            [Inline<Test>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived : MyBase, IA, IB {
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Inline_BaseClassWithPrimaryConstructor() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test : MyBase(5);

            [Inline<Test>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived : MyBase(5) {
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }
}
