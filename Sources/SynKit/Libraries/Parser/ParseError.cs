// Copyright (c) 2021-2022 Yoakke.
// Licensed under the Apache License, Version 2.0.
// Source repository: https://github.com/LanguageDev/Yoakke

using System;
using System.Collections.Generic;
using System.Collections.Generic.Polyfill;
using System.Linq;

namespace Yoakke.SynKit.Parser;

/// <summary>
/// Describes a parse error.
/// </summary>
public class ParseError
{
    private static readonly Dictionary<string, ParseErrorElement> plug = new();

    /// <summary>
    /// The error cases in different parse contexts.
    /// </summary>
    public IReadOnlyDictionary<string, ParseErrorElement> Elements { get; } = plug;

    /// <summary>
    /// The item that was found, if any.
    /// </summary>
    public object? Got { get; }

    /// <summary>
    /// The position where the error occurred.
    /// </summary>
    public IComparable Position { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseError"/> class.
    /// </summary>
    /// <param name="expected">The expected input.</param>
    /// <param name="got">The item that was found.</param>
    /// <param name="position">The position where the error occurred.</param>
    /// <param name="context">The context in which the error occurred.</param>
    public ParseError(object expected, object? got, IComparable position, string context)
        : this(plug, got, position)
    {
    }

    private ParseError(IReadOnlyDictionary<string, ParseErrorElement> elements, object? got, IComparable position)
    {
        this.Elements = elements;
        this.Got = got;
        this.Position = position;
    }

    /// <summary>
    /// Unifies two alternative <see cref="ParseError"/>s.
    /// </summary>
    /// <param name="first">The first error to unify.</param>
    /// <param name="second">The second error to unify.</param>
    /// <returns>The error that represents both of them properly.</returns>
    public static ParseError? operator |(ParseError? first, ParseError? second)
    {
        return first;
        // Check nullities
        if (first is null && second is null) return null;
        if (first is null) return second!;
        if (second is null) return first;
        // Position overrides above all
        var cmp = first.Position.CompareTo(second.Position);
        if (cmp < 0) return second;
        if (cmp > 0) return first;
        // Both of them got stuck at the same place, merge entries
        var elements = first.Elements.Values.ToDictionary(e => e.Context, e => e.Expected.ToHashSet());
        foreach (var element in second.Elements.Values)
        {
            if (elements.TryGetValue(element.Context, out var part))
            {
                foreach (var e in element.Expected) part.Add(e);
            }
            else
            {
                part = element.Expected.ToHashSet();
                elements.Add(element.Context, part);
            }
        }
        // TODO: Think this through
        // NOTE: Could it ever happen that first.Got and second.Got are different but neither are null?
        // Would we want to unify these and move them to ParseErrorElement or something?
        // Since position is here now, Got is kind of a utility we have here, we could just aswell have a
        // 'reason' for each element
        return new(
            elements.ToDictionary(kv => kv.Key, kv => new ParseErrorElement(kv.Value, kv.Key)),
            first.Got ?? second.Got,
            first.Position);
    }
}
