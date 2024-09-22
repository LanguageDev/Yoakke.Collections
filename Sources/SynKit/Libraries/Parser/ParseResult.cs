// Copyright (c) 2021-2022 Yoakke.
// Licensed under the Apache License, Version 2.0.
// Source repository: https://github.com/LanguageDev/Yoakke

namespace Yoakke.SynKit.Parser;

/// <summary>
/// The result of a parse.
/// </summary>
/// <typeparam name="T">The parsed value type.</typeparam>
public readonly struct ParseResult<T>
{
    private readonly ParseOk<T> ok;
    private readonly ParseError error;

    /// <summary>
    /// True, if the result is a success.
    /// </summary>
    public bool IsOk { get; }

    /// <summary>
    /// True, if the result is an error.
    /// </summary>
    public bool IsError => !IsOk;

    /// <summary>
    /// Retrieves the parse as a success.
    /// </summary>
    public ParseOk<T> Ok => ok;

    /// <summary>
    /// Retrieves the parse as an error.
    /// </summary>
    public ParseError Error => error;

    /// <summary>
    /// Retrieves the furthest error for this result.
    /// </summary>
    public ParseError? FurthestError => IsOk ? ok.FurthestError : error;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseResult{T}"/> struct.
    /// </summary>
    /// <param name="ok">The successful parse description.</param>
    public ParseResult(ParseOk<T> ok)
    {
        this.ok = ok;
        IsOk = true;
        error = default!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseResult{T}"/> struct.
    /// </summary>
    /// <param name="error">The error description.</param>
    public ParseResult(ParseError error)
    {
        this.error = error;
        IsOk = false;
        this.ok = default!;
    }

    /// <summary>
    /// Implicitly converts a <see cref="ParseOk{T}"/> into a <see cref="ParseResult{T}"/>.
    /// </summary>
    /// <param name="ok">The <see cref="ParseOk{T}"/> to convert.</param>
    public static implicit operator ParseResult<T>(ParseOk<T> ok) => new(ok);

    /// <summary>
    /// Implicitly converts a <see cref="ParseError"/> into a <see cref="ParseResult{T}"/>.
    /// </summary>
    /// <param name="error">The <see cref="ParseError"/> to convert.</param>
    public static implicit operator ParseResult<T>(ParseError error) => new(error);

    /// <summary>
    /// Merges alternative <see cref="ParseResult{T}"/>s that happened from the same starting position.
    /// </summary>
    /// <param name="first">The first alternative result.</param>
    /// <param name="second">The second alternative result.</param>
    /// <returns>The <see cref="ParseResult{T}"/> constructed from the alternatives.</returns>
    public static ParseResult<T> operator |(ParseResult<T> first, ParseResult<T> second)
    {
        return first.IsOk ? first.Ok : (second.IsOk ? second.Ok : second.Error);
        if (first.IsOk && second.IsOk) return first.Ok | second.Ok;
        if (first.IsOk) return first.Ok | second.Error;
        if (second.IsOk) return second.Ok | first.Error;
        // Both are errors
        return (first.Error | second.Error)!;
    }

    /// <summary>
    /// Merges an alternative <see cref="ParseResult{T}"/> and <see cref="ParseOk{T}"/>.
    /// </summary>
    /// <param name="first">The first alternative result.</param>
    /// <param name="second">The second alternative ok.</param>
    /// <returns>The <see cref="ParseResult{T}"/> constructed from the alternatives.</returns>
    public static ParseResult<T> operator |(ParseResult<T> first, ParseOk<T> second) => first.IsOk
        ? first.Ok | second
        : first.Error | second;

    /// <summary>
    /// Merges an alternative <see cref="ParseResult{T}"/> and <see cref="ParseOk{T}"/>.
    /// </summary>
    /// <param name="first">The first alternative ok.</param>
    /// <param name="second">The second alternative result.</param>
    /// <returns>The <see cref="ParseResult{T}"/> constructed from the alternatives.</returns>
    public static ParseResult<T> operator |(ParseOk<T> first, ParseResult<T> second) => second | first;

    /// <summary>
    /// Merges an alternative <see cref="ParseResult{T}"/> and <see cref="ParseError"/>.
    /// </summary>
    /// <param name="first">The first alternative result.</param>
    /// <param name="second">The second alternative error.</param>
    /// <returns>The <see cref="ParseResult{T}"/> constructed from the alternatives.</returns>
    public static ParseResult<T> operator |(ParseResult<T> first, ParseError? second) => first.IsOk
        ? first.Ok | second
        : (first.Error | second)!;

    /// <summary>
    /// Merges an alternative <see cref="ParseResult{T}"/> and <see cref="ParseError"/>.
    /// </summary>
    /// <param name="first">The first alternative error.</param>
    /// <param name="second">The second alternative result.</param>
    /// <returns>The <see cref="ParseResult{T}"/> constructed from the alternatives.</returns>
    public static ParseResult<T> operator |(ParseError? first, ParseResult<T> second) => second | first;
}
