﻿namespace InlineComposition;

public static partial class Attributes {
    public const string InlineMethodAttribute = $$"""
        // <auto-generated/>
        #pragma warning disable
        #nullable enable annotations
    
    
        using System;
    
        namespace InlineCompositionAttributes;

        /// <summary>
        /// The Method under this attribute will be inlined in the method given by <see cref="MethodName"/>.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("{{AssemblyInfo.NAME}}", "{{AssemblyInfo.VERSION}}")]
        internal sealed class InlineMethodAttribute : Attribute {
            /// <summary>
            /// The method name as string literal.
            /// </summary>
            public required string MethodName { get; init; }

            /// <summary>
            /// <para>Modifiers e.g. "public static extern", "protected abstract"</para>
            /// <para>If null, the method modifiers will be taken.</para>
            /// </summary>
            public string? Modifiers { get; init; }

            /// <summary>
            /// Indicates whether this method gets inlined before the other methods or after.
            /// </summary>
            public bool First { get; init; }
        }
    
        """;
}
