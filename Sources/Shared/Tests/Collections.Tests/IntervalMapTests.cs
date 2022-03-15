// Copyright (c) 2021-2022 Yoakke.
// Licensed under the Apache License, Version 2.0.
// Source repository: https://github.com/LanguageDev/Yoakke

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Yoakke.Collections.Intervals;

namespace Yoakke.Collections.Tests;

public sealed class IntervalMapTests
{
    private readonly struct HashSetCombiner : IValueCombiner<HashSet<int>>
    {
        public HashSet<int> Combine(HashSet<int> first, HashSet<int> second) => new(first.Concat(second));
    }

    // Legacy tests
    [InlineData(
        "",
        "[2; 3) => { 1 }",
        "[2; 3) => { 1 }")]
    [InlineData(
        "[5; 7) => { 1 } U [12; 15) => { 1 }",
        "[2; 3) => { 2 }",
        "[2; 3) => { 2 } U [5; 7) => { 1 } U [12; 15) => { 1 }")]
    [InlineData(
        "[5; 7) => { 1 } U [12; 15) => { 1 }",
        "[2; 5) => { 2 }",
        "[2; 5) => { 2 } U [5; 7) => { 1 } U [12; 15) => { 1 }")]
    [InlineData(
        "[5; 7) => { 1 } U [12; 15) => { 1 }",
        "[9; 11) => { 2 }",
        "[5; 7) => { 1 } U [9; 11) => { 2 } U [12; 15) => { 1 }")]
    [InlineData(
        "[5; 7) => { 1 } U [12; 15) => { 1 }",
        "[7; 12) => { 2 }",
        "[5; 7) => { 1 } U [7; 12) => { 2 } U [12; 15) => { 1 }")]
    [InlineData(
        "[5; 7) => { 1 } U [12; 15) => { 1 }",
        "[17; 19) => { 2 }",
        "[5; 7) => { 1 } U [12; 15) => { 1 } U [17; 19) => { 2 }")]
    [InlineData(
        "[5; 7) => { 1 } U [12; 15) => { 1 }",
        "[15; 19) => { 2 }",
        "[5; 7) => { 1 } U [12; 15) => { 1 } U [15; 19) => { 2 }")]
    [InlineData(
        "[5; 7) => { 1 }",
        "[5; 7) => { 2 }",
        "[5; 7) => { 1, 2 }")]
    [InlineData(
        "[3; 9) => { 1 }",
        "[5; 7) => { 2 }",
        "[3; 5) => { 1 } U [5; 7) => { 1, 2 } U [7; 9) => { 1 }")]
    [InlineData(
        "[5; 7) => { 1 }",
        "[3; 9) => { 2 }",
        "[3; 5) => { 2 } U [5; 7) => { 1, 2 } U [7; 9) => { 2 }")]
    [InlineData(
        "[5; 9) => { 1 }",
        "[2; 7) => { 2 }",
        "[2; 5) => { 2 } U [5; 7) => { 1, 2 } U [7; 9) => { 1 }")]
    [InlineData(
        "[5; 9) => { 1 }",
        "[7; 12) => { 2 }",
        "[5; 7) => { 1 } U [7; 9) => { 1, 2 } U [9; 12) => { 2 }")]
    [InlineData(
        "[5; 11) => { 1 }",
        "[5; 7) => { 2 }",
        "[5; 7) => { 1, 2 } U [7; 11) => { 1 }")]
    [InlineData(
        "[5; 7) => { 1 }",
        "[5; 11) => { 2 }",
        "[5; 7) => { 1, 2 } U [7; 11) => { 2 }")]
    [InlineData(
        "[5; 11) => { 1 }",
        "[8; 11) => { 2 }",
        "[5; 8) => { 1 } U [8; 11) => { 1, 2 }")]
    [InlineData(
        "[8; 11) => { 1 }",
        "[5; 11) => { 2 }",
        "[5; 8) => { 2 } U [8; 11) => { 1, 2 }")]
    [InlineData(
        "[1; 3) => { 1 } U [5; 7) => { 1 } U [9; 12) => { 1 } U [14; 15) => { 1 } U [15; 17) => { 1 } U [18; 19) => { 1 }",
        "[5; 17) => { 2 }",
        "[1; 3) => { 1 } U [5; 7) => { 1, 2 } U [7; 9) => { 2 } U [9; 12) => { 1, 2 } U [12; 14) => { 2 } U [14; 15) => { 1, 2 } U [15; 17) => { 1, 2 } U [18; 19) => { 1 }")]
    [InlineData(
        "[1; 3) => { 1 } U [5; 7) => { 1 } U [9; 12) => { 1 } U [14; 15) => { 1 } U [15; 17) => { 1 } U [18; 19) => { 1 } U [21; 24) => { 1 }",
        "[5; 19) => { 2 }",
        "[1; 3) => { 1 } U [5; 7) => { 1, 2 } U [7; 9) => { 2 } U [9; 12) => { 1, 2 } U [12; 14) => { 2 } U [14; 15) => { 1, 2 } U [15; 17) => { 1, 2 } U [17; 18) => { 2 } U [18; 19) => { 1, 2 } U [21; 24) => { 1 }")]
    [InlineData(
        "[1; 3) => { 1 } U [5; 7) => { 1 } U [7; 9) => { 1 } U [9; 12) => { 1 } U [12; 14) => { 1 } U [14; 15) => { 1 } U [15; 17) => { 1 } U [18; 19) => { 1 }",
        "[5; 17) => { 2 }",
        "[1; 3) => { 1 } U [5; 7) => { 1, 2 } U [7; 9) => { 1, 2 } U [9; 12) => { 1, 2 } U [12; 14) => { 1, 2 } U [14; 15) => { 1, 2 } U [15; 17) => { 1, 2 } U [18; 19) => { 1 }")]
    [InlineData(
        "[1; 3) => { 1 } U [5; 7) => { 1 } U [9; 12) => { 1 } U [14; 15) => { 1 }",
        "[5; 10) => { 2 }",
        "[1; 3) => { 1 } U [5; 7) => { 1, 2 } U [7; 9) => { 2 } U [9; 10) => { 1, 2 } U [10; 12) => { 1 } U [14; 15) => { 1 }")]
    [InlineData(
        "[1; 3) => { 1 } U [5; 7) => { 1 } U [9; 12) => { 1 } U [14; 15) => { 1 }",
        "[6; 12) => { 2 }",
        "[1; 3) => { 1 } U [5; 6) => { 1 } U [6; 7) => { 1, 2 } U [7; 9) => { 2 } U [9; 12) => { 1, 2 } U [14; 15) => { 1 }")]
    [InlineData(
        "[1; 3) => { 1 } U [5; 7) => { 1 } U [9; 12) => { 1 } U [14; 15) => { 1 }",
        "[6; 10) => { 2 }",
        "[1; 3) => { 1 } U [5; 6) => { 1 } U [6; 7) => { 1, 2 } U [7; 9) => { 2 } U [9; 10) => { 1, 2 } U [10; 12) => { 1 } U [14; 15) => { 1 }")]
    [Theory]
    public void AddInterval(string mapText, string assocText, string resultText)
    {
        var originalMap = ParseDenseMap(mapText);
        var resultingMap = ParseDenseMap(resultText);
        var assoc = ParseAssociation(assocText);

        originalMap.Add(assoc.Key, assoc.Value, default(HashSetCombiner));

        AssertEquals(originalMap, resultingMap);
    }

    [InlineData(
        "[2; 5) => { 1 } U [7; 9) => { 1 }",
        "[4; 8)",
        "[2; 4) => { 1 } U [8; 9) => { 1 }")]
    [InlineData(
        "[2; 5) => { 1 } U [7; 9) => { 1 }",
        "[4; 9)",
        "[2; 4) => { 1 }")]
    [InlineData(
        "[2; 5) => { 1 } U [7; 9) => { 1 }",
        "[2; 8)",
        "[8; 9) => { 1 }")]
    [InlineData(
        "(-oo; +oo) => { 1 }",
        "[0; 0]",
        "(-oo; 0) => { 1 } U (0; +oo) => { 1 }")]
    // Legacy tests inverted
    [InlineData(
        "",
        "[2; 3)",
        "")]
    [InlineData(
        "[5; 7) => { 1 } U [12; 15) => { 1 }",
        "[2; 3)",
        "[5; 7) => { 1 } U [12; 15) => { 1 }")]
    [InlineData(
        "[5; 7) => { 1 } U [12; 15) => { 1 }",
        "[2; 5)",
        "[5; 7) => { 1 } U [12; 15) => { 1 }")]
    [InlineData(
        "[5; 7) => { 1 } U [12; 15) => { 1 }",
        "[9; 11)",
        "[5; 7) => { 1 } U [12; 15) => { 1 }")]
    [InlineData(
        "[5; 7) => { 1 } U [12; 15) => { 1 }",
        "[7; 12)",
        "[5; 7) => { 1 } U [12; 15) => { 1 }")]
    [InlineData(
        "[5; 7) => { 1 } U [12; 15) => { 1 }",
        "[17; 19)",
        "[5; 7) => { 1 } U [12; 15) => { 1 }")]
    [InlineData(
        "[5; 7) => { 1 } U [12; 15) => { 1 }",
        "[15; 19)",
        "[5; 7) => { 1 } U [12; 15) => { 1 }")]
    [InlineData(
        "[5; 7) => { 1 }",
        "[5; 7)",
        "")]
    [InlineData(
        "[3; 9) => { 1 }",
        "[5; 7)",
        "[3; 5) => { 1 } U [7; 9) => { 1 }")]
    [InlineData(
        "[5; 7) => { 1 }",
        "[3; 9)",
        "")]
    [InlineData(
        "[5; 9) => { 1 }",
        "[2; 7)",
        "[7; 9) => { 1 }")]
    [InlineData(
        "[5; 9) => { 1 }",
        "[7; 12)",
        "[5; 7) => { 1 }")]
    [InlineData(
        "[5; 11) => { 1 }",
        "[5; 7)",
        "[7; 11) => { 1 }")]
    [InlineData(
        "[5; 7) => { 1 }",
        "[5; 11)",
        "")]
    [InlineData(
        "[5; 11) => { 1 }",
        "[8; 11)",
        "[5; 8) => { 1 }")]
    [InlineData(
        "[8; 11) => { 1 }",
        "[5; 11)",
        "")]
    [InlineData(
        "[1; 3) => { 1 } U [5; 7) => { 1 } U [9; 12) => { 1 } U [14; 15) => { 1 } U [15; 17) => { 1 } U [18; 19) => { 1 }",
        "[5; 17)",
        "[1; 3) => { 1 } U [18; 19) => { 1 }")]
    [InlineData(
        "[1; 3) => { 1 } U [5; 7) => { 1 } U [9; 12) => { 1 } U [14; 15) => { 1 } U [15; 17) => { 1 } U [18; 19) => { 1 } U [21; 24) => { 1 }",
        "[5; 19)",
        "[1; 3) => { 1 } U [21; 24) => { 1 }")]
    [InlineData(
        "[1; 3) => { 1 } U [5; 7) => { 1 } U [7; 9) => { 1 } U [9; 12) => { 1 } U [12; 14) => { 1 } U [14; 15) => { 1 } U [15; 17) => { 1 } U [18; 19) => { 1 }",
        "[5; 17)",
        "[1; 3) => { 1 } U [18; 19) => { 1 }")]
    [InlineData(
        "[1; 3) => { 1 } U [5; 7) => { 1 } U [9; 12) => { 1 } U [14; 15) => { 1 }",
        "[5; 10)",
        "[1; 3) => { 1 } U [10; 12) => { 1 } U [14; 15) => { 1 }")]
    [InlineData(
        "[1; 3) => { 1 } U [5; 7) => { 1 } U [9; 12) => { 1 } U [14; 15) => { 1 }",
        "[6; 12)",
        "[1; 3) => { 1 } U [5; 6) => { 1 } U [14; 15) => { 1 }")]
    [InlineData(
        "[1; 3) => { 1 } U [5; 7) => { 1 } U [9; 12) => { 1 } U [14; 15) => { 1 }",
        "[6; 10)",
        "[1; 3) => { 1 } U [5; 6) => { 1 } U [10; 12) => { 1 } U [14; 15) => { 1 }")]
    [Theory]
    public void RemoveInterval(string mapText, string intervalText, string resultText)
    {
        var originalMap = ParseDenseMap(mapText);
        var resultingMap = ParseDenseMap(resultText);
        var interval = IntervalTests.ParseInterval(intervalText);

        originalMap.Remove(interval);

        AssertEquals(originalMap, resultingMap);
    }

    [InlineData("", "(0; 0)")]
    [InlineData("[0; 5) => { 1 }", "[0; 5)")]
    [InlineData("[0; 5) => { 1 }", "[0; 1)")]
    [InlineData("[0; 5) => { 1 }", "[4; 5)")]
    [InlineData("[0; 5) => { 1 }", "[2; 3)")]
    [InlineData("(-oo; +oo) => { 1 }", "[2; 3)")]
    [InlineData("[0; 1) => { 1 } U [1; 2) => { 1 }", "[0; 2)")]
    [Theory]
    public void ContainsInterval(string mapText, string intervalText)
    {
        var map = ParseDenseMap(mapText);
        var interval = IntervalTests.ParseInterval(intervalText);

        Assert.True(map.ContainsKeys(interval));
    }

    [InlineData("[0; 5) => { 1 }", "[6; 7)")]
    [InlineData("[0; 1) => { 1 } U [1; 2) => { 1 }", "[0; 3)")]
    [InlineData("[0; 1) => { 1 } U [1; 2) => { 1 }", "[-1; 2)")]
    [InlineData("[0; 1) => { 1 } U [1; 2) => { 1 }", "[-1; 3)")]
    [InlineData("[0; 1) => { 1 } U [2; 3) => { 1 }", "[1; 2)")]
    [InlineData("[0; 1) => { 1 } U [2; 3) => { 1 }", "[0; 3)")]
    [Theory]
    public void DoesNotContainInterval(string mapText, string intervalText)
    {
        var map = ParseDenseMap(mapText);
        var interval = IntervalTests.ParseInterval(intervalText);

        Assert.False(map.ContainsKeys(interval));
    }

    private static void AssertEquals(
        IntervalMap<int, HashSet<int>> a,
        IEnumerable<KeyValuePair<Interval<int>, HashSet<int>>> b)
    {
        // Key equality
        Assert.True(a.Select(i => i.Key).SequenceEqual(b.Select(i => i.Key)));
        // Value equality
        Assert.True(a.Select(i => i.Value).Zip(b.Select(i => i.Value)).All(pair => pair.First.SetEquals(pair.Second)));
    }

    private static IntervalMap<int, HashSet<int>> ParseDenseMap(string text)
    {
        text = text.Trim();
        // Empty string means empty set
        if (text.Length == 0) return new();

        // Split by Union and parse intervals
        var intervalParts = text.Split('U');
        var intervalSetPairs = intervalParts.Select(ParseAssociation);

        // Construct the dense set
        var result = new IntervalMap<int, HashSet<int>>();
        foreach (var (iv, set) in intervalSetPairs) result.Add(iv, set, default(HashSetCombiner));

        // Check equality
        AssertEquals(result, intervalSetPairs);

        return result;
    }

    private static KeyValuePair<Interval<int>, HashSet<int>> ParseAssociation(string text)
    {
        var parts = text.Split("=>");
        var interval = IntervalTests.ParseInterval(parts[0]);
        var set = ParseSet(parts[1]);
        return new(interval, set);
    }

    private static HashSet<int> ParseSet(string text)
    {
        text = text.Trim();
        Assert.Equal('{', text[0]);
        Assert.Equal('}', text[^1]);
        return text[1..^1].Split(',').Select(t => int.Parse(t.Trim())).ToHashSet();
    }
}
