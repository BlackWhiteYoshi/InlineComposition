﻿namespace InlineComposition;

public static partial class Attributes {
    public const string InlineBaseAttribute = $$"""
    // <auto-generated>
    #pragma warning disable
    #nullable enable annotations
    
    
    using System;
    
    namespace InlineCompositionAttributes;

    /// <summary>
    /// <para>Marks this class/struct as inlineable, so it can be listed in a <see cref="InlineAttribute{T1}"/> Attribute.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("{{NAME}}", "{{VERSION}}")]
    internal sealed class InlineBaseAttribute : Attribute {
        /// <summary>
        /// If set the generator ignores the inherited class and implemented interfaces of this type.
        /// </summary>
        public bool IgnoreInheritenceAndImplements { get; init; }
    }
    
    """;
}
