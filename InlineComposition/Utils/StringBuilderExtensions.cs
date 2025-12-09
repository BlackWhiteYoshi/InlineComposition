using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.CompilerServices;
using System.Text;

namespace InlineComposition;

public static class StringBuilderExtensions {
    /// <summary>
    /// Appends in a recursive way the full namespace.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="namespaceSyntax"></param>
    /// <returns></returns>
    public static StringBuilder AppendNamespace(this StringBuilder builder, BaseNamespaceDeclarationSyntax namespaceSyntax) {
        BaseNamespaceDeclarationSyntax? parentNamespace = namespaceSyntax.GetParent<BaseNamespaceDeclarationSyntax>();

        if (parentNamespace != null)
            builder.AppendNamespace(parentNamespace)
                .Append('.');
        builder.Append(namespaceSyntax.Name);

        return builder;
    }
    extension(BaseNamespaceDeclarationSyntax namespaceSyntax) {
        /// <summary>
        /// Creates a type to map method <see cref="StringBuilderInterpolationHandler.AppendFormatted(StringBuilderInterpolationHandler.INamespace)"/> to <see cref="AppendNamespace"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public StringBuilderInterpolationHandler.INamespace AsNamespace => Unsafe.As<StringBuilderInterpolationHandler.INamespace>(namespaceSyntax);
    }


    /// <summary>
    /// The same as <see cref="StringBuilder.Append(string)"/>, but only for interpolated strings: $"..."<br />
    /// It constructs the string directly in the builder, so no unnecessary string memory allocations.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="handler"></param>
    /// <returns></returns>
    public static StringBuilder AppendInterpolation(this StringBuilder builder, [InterpolatedStringHandlerArgument("builder")] StringBuilderInterpolationHandler handler) => builder;

    [InterpolatedStringHandler]
    public readonly ref struct StringBuilderInterpolationHandler {
        private readonly StringBuilder builder;

        public StringBuilderInterpolationHandler(int literalLength, int formattedCount, StringBuilder builder) => this.builder = builder;

        public void AppendLiteral(string str) => builder.Append(str);

        public void AppendFormatted<T>(T item) => builder.Append(item);


        public interface INamespace;
        public void AppendFormatted(INamespace @namespace) => builder.AppendNamespace(Unsafe.As<BaseNamespaceDeclarationSyntax>(@namespace));
    }
}
