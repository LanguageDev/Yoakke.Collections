// Copyright (c) 2021-2022 Yoakke.
// Licensed under the Apache License, Version 2.0.
// Source repository: https://github.com/LanguageDev/Yoakke

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Yoakke.SynKit.Lexer.Generator.Model;

/// <summary>
/// Describes a declared lexer.
/// </summary>
internal class LexerModel
{
    /// <summary>
    /// The symbol containing the source character stream.
    /// </summary>
    public ISymbol? SourceSymbol { get; set; }

    /// <summary>
    /// The symbol used to define an error/unknown token type.
    /// </summary>
    public IFieldSymbol? ErrorSymbol { get; set; }

    /// <summary>
    /// The symbol used to define an end token type.
    /// </summary>
    public IFieldSymbol? EndSymbol { get; set; }

    /// <summary>
    /// The list of <see cref="TokenModel"/>s.
    /// </summary>
    public IList<TokenModel> Tokens { get; } = new List<TokenModel>();
}