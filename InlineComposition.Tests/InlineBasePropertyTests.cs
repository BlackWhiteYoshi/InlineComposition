namespace InlineComposition.Tests;

/// <summary>
/// InlineBase Properties
/// </summary>
public sealed class InlineBasePropertyTests {
    [Test]
    public async ValueTask MapBaseType() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            [InlineBase(MapBaseType = true)]
            public struct Test {
                public Test A => null!;
            }

            [InlineBase(MapBaseType = false]
            public struct Test2 {
                public Test2 B => null!;
            }

            [Inline<Test, Test2>]
            public partial struct Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public partial struct Derived {
                public Derived A => null!;

                public Test2 B => null!;

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask IgnoreInheritenceAndImplements() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            [InlineBase(IgnoreInheritenceAndImplements = true)]
            public sealed class Test : ITest;

            [InlineBase(IgnoreInheritenceAndImplements = false)]
            public sealed class Test2 : ITest2;

            [Inline<Test, Test2>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived : ITest2 {
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask InlineAttributes() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            [TestAttribute]
            [InlineBase(InlineAttributes = true)]
            public struct Test;

            [TestAttribute]
            [InlineBase(InlineAttributes = false)]
            public struct Test2;

            [Inline<Test, Test2>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            [TestAttribute]
            public sealed partial class Derived {
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }
}
