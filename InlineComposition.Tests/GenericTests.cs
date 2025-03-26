namespace InlineComposition.Tests;

/// <summary>
/// If the inlined class/struct/record itself or base has type parameters
/// </summary>
public sealed class GenericTests {
    [Test]
    public async ValueTask Generic() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            [InlineBase]
            public sealed class Test<T> {
                public T myField = default;

                public void MyMethod<U>() {
                    T variable1 = default;
                    U variable2 = default;
                }

                public T GenericParameter(System.Collections.Generic.List<T> list) { }

                public T Id(T t) => default!;
            }

            [Inline<Test<string>>]
            public sealed partial class Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public sealed partial class Derived {
                public string myField = default;

                public void MyMethod<U>() {
                    {
                    string variable1 = default;
                    U variable2 = default;
                    }
                }

                public string GenericParameter(System.Collections.Generic.List<string> list) {
                    {
                    }
                }

                public string Id(string t) {
                    {
                        return default!;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask MapBaseTypeGeneric() {
        const string input = """
            using InlineCompositionAttributes;

            namespace MyCode;

            [InlineBase(MapBaseType = true)]
            public struct Test<T> : IEquatable<Test<T>> {
                public Test<T> A(Test<T> t, T t2) => null!;
            }

            [InlineBase(MapBaseType = false]
            public struct Test2<T> : IComparable<T> {
                public Test<T> B(Test<T> t, T t2) => null!;
            }

            [Inline<Test<string>, Test2<string>>]
            public partial struct Derived;

            """;
        string sourceText = Shared.GenerateSourceText(input, out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public partial struct Derived : IEquatable<Derived>, IComparable<string> {
                public Derived A(Derived t, string t2) {
                    {
                        return null!;
                    }
                }

                public Test<string> B(Test<string> t, string t2) {
                    {
                        return null!;
                    }
                }

            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }
}
