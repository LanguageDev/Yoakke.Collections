// Copyright (c) 2021-2022 Yoakke.
// Licensed under the Apache License, Version 2.0.
// Source repository: https://github.com/LanguageDev/Yoakke

using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.SynKit.Parser.Attributes;

/// <summary>
/// An attribute to annotate a bustom parser for a rule.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal class CustomParserAttribute : Attribute
{
    /// <summary>
    /// The rule name.
    /// </summary>
    public string Rule { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomParserAttribute"/> class.
    /// </summary>
    /// <param name="rule">The rule name.</param>
    public CustomParserAttribute(string rule)
    {
        this.Rule = rule;
    }
}
