namespace InlineComposition.Tests;

/// <summary>
/// multiple bases, seperate inlines
/// </summary>
public sealed class MultipleTests {
    [Test]
    public async ValueTask MultipleBases() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public int myField = 5;
            }

            public sealed class Test2 {
                public int myField2 = 25;
            }

            [Inline<Test, Test2>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                public int myField = 5;

                public int myField2 = 25;

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask MultipleBases_PrimaryConstructor_GetMerged() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test(int prime);

            public sealed class Test2(int prime, string a);

            [Inline<Test, Test2>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived(int prime, string a) {
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask MultipleBases_PrimaryConstructor_DifferentNamesGetNotMerged() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test(int prime);

            public sealed class Test2(int prime2, string a);

            [Inline<Test, Test2>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived(int prime, int prime2, string a) {
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask MultipleBases_ClassesAndInterfacesGetMerged() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public class N;
            public interface IN;
            public class A;
            public interface IA;
            public interface IB;
            public interface IC;

            public sealed class Test : A, IA, IB {
                public int myField = 5;
            }

            public sealed class Test2 : A, IA, IC {
                public int myField2 = 25;
            }

            [Inline<Test, Test2>]
            public sealed partial class Derived : N, IN;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived : A, IA, IB, IC {
                public int myField = 5;

                public int myField2 = 25;

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask MultipleBases_ConflictsNonMethods_GetMerged() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public int myField = 5;
            }

            public sealed class Test2 {
                public int myField = 5;
            }

            [Inline<Test, Test2>]
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
    public async ValueTask MultipleBases_ConflictsMethods_GetMerged() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public void MyMethod() {
                    int ab = 17;
                }
            }

            public sealed class Test2 {
                public void MyMethod() {
                    int cd = 283;
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
                    {
                    int cd = 283;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask MultipleBases_ConflictsExternMethods_GetMerged() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_internString")]
                private extern static ref string GetString(Test @this);
            }

            public sealed class Test2 {
                [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_internString")]
                private extern static ref string GetString(Test @this);
            }

            [Inline<Test, Test2>]
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
    public async ValueTask MultipleBases_ConflictsConstructors_GetMerged() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public Test() {
                    int ab = 17;
                }
            }

            public sealed class Test2 {
                public Test2() {
                    int cd = 283;
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
                    {
                    int cd = 283;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask MultipleBases_ConflictsFinalizer_GetMerged() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                ~Test() {
                    int ab = 17;
                }
            }

            public sealed class Test2 {
                ~Test2() {
                    int cd = 283;
                }
            }

            [Inline<Test, Test2>]
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
                    {
                    int cd = 283;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask MultipleInlines() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            public sealed class Test {
                public int a = 1;
            }

            public sealed class Test2 {
                public int b = 2;
            }

            [Inline<Test>]
            public sealed partial class Derived1;

            [Inline<Test2>]
            public sealed partial class Derived2;

            """;
        string[] sourceTexts = Shared.GenerateSourceText(input, out _, out _);
        (string sourceText1, string sourceText2) = (sourceTexts[^2], sourceTexts[^1]);

        const string expected1 = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived1 {
                public int a = 1;

            }

            """;
        await Assert.That(sourceText1).IsEqualTo(expected1);

        const string expected2 = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived2 {
                public int b = 2;

            }

            """;
        await Assert.That(sourceText2).IsEqualTo(expected2);
    }
}
