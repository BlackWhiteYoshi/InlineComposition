﻿namespace InlineComposition;

public static partial class Attributes {
    public const string InlineConstructorAttribute = $$"""
        // <auto-generated/>
        #pragma warning disable
        #nullable enable annotations


        #if !INLINECOMPOSITION_EXCLUDE_ATTRIBUTES

        using System;

        namespace InlineCompositionAttributes;

        /// <summary>
        /// The Method under this attribute will be inlined in the constructor.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("{{NAME}}", "{{VERSION}}")]
        internal sealed class InlineConstructorAttribute : Attribute {
            /// <summary>
            /// <para>Modifiers e.g. "public", "protected", "private"</para>
            /// <para>If null, the method modifiers will be taken.</para>
            /// </summary>
            public string? Modifiers { get; init; }

            /// <summary>
            /// Indicates whether this method gets inlined before the other constructors or after.
            /// </summary>
            public bool First { get; init; }
        }

        #endif

        """;
}
