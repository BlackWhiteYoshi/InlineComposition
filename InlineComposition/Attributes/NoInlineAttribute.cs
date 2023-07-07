﻿namespace InlineComposition;

public static partial class Attributes {
    public const string NoInlineAttribute = $$"""
    // <auto-generated>
    #pragma warning disable
    #nullable enable annotations
    
    
    using System;
    
    namespace InlineCompositionAttributes;

    /// <summary>
    /// <para>Only usefule in a class/struct with a <see cref="InlineBaseAttribute"/>.</para>
    /// <para>Skips/Ignores this member.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method)]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("{{NAME}}", "{{VERSION}}")]
    internal sealed class NoInlineAttribute : Attribute { }
    
    """;
}
