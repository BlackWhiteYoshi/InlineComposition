namespace InlineComposition.Tests;

/// <summary>
/// class, struct, record class, record struct
/// </summary>
public sealed class TypeTests {
    [Test]
    public async ValueTask ClassWithClass() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public class Test {
                public int myField = 5;
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
    public async ValueTask ClassWithStruct() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public struct Test {
                public int myField = 5;
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
    public async ValueTask ClassWithRecordClass() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public class record Test {
                public int myField = 5;
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
    public async ValueTask ClassWithRecordStruct() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public struct record Test {
                public int myField = 5;
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
    public async ValueTask StructWithClass() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public int myField = 5;
            }

            [Inline<Test>]
            public partial struct Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public partial struct Derived {
                public int myField = 5;

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask StructWithStruct() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public struct Test {
                public int myField = 5;
            }

            [Inline<Test>]
            public partial struct Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public partial struct Derived {
                public int myField = 5;

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask StructWithRecordClass() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed record class Test {
                public int myField = 5;
            }

            [Inline<Test>]
            public partial struct Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public partial struct Derived {
                public int myField = 5;

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask StructWithRecordStruct() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed record struct Test {
                public int myField = 5;
            }

            [Inline<Test>]
            public partial struct Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public partial struct Derived {
                public int myField = 5;

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }


    [Test]
    public async ValueTask RecordClassWithClass() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public class Test {
                public int myField = 5;
            }

            [Inline<Test>]
            public sealed partial record Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial record Derived {
                public int myField = 5;

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask RecordClassWithStruct() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public struct Test {
                public int myField = 5;
            }

            [Inline<Test>]
            public sealed partial record class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial record class Derived {
                public int myField = 5;

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask RecordClassWithRecordClass() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public record class Test {
                public int myField = 5;
            }

            [Inline<Test>]
            public sealed partial record class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial record class Derived {
                public int myField = 5;

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask RecordClassWithRecordStruct() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public record struct Test {
                public int myField = 5;
            }

            [Inline<Test>]
            public sealed partial record class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial record class Derived {
                public int myField = 5;

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }


    [Test]
    public async ValueTask RecordStructWithClass() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public class Test {
                public int myField = 5;
            }

            [Inline<Test>]
            public partial record struct Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public partial record struct Derived {
                public int myField = 5;

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask RecordStructWithStruct() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public struct Test {
                public int myField = 5;
            }

            [Inline<Test>]
            public partial record struct Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public partial record struct Derived {
                public int myField = 5;

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask RecordStructWithRecordClass() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public record Test {
                public int myField = 5;
            }

            [Inline<Test>]
            public partial record struct Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public partial record struct Derived {
                public int myField = 5;

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask RecordStructWithRecordStruct() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public record struct Test {
                public int myField = 5;
            }

            [Inline<Test>]
            public partial record struct Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public partial record struct Derived {
                public int myField = 5;

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }
}
