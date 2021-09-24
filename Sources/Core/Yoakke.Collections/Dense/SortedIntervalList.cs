// Copyright (c) 2021 Yoakke.
// Licensed under the Apache License, Version 2.0.
// Source repository: https://github.com/LanguageDev/Yoakke

using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Collections.Intervals;

namespace Yoakke.Collections.Dense
{
    /// <summary>
    /// Represents a sorted list of intervals associated to some other value to help the interval set and map
    /// implementations.
    /// </summary>
    /// <typeparam name="TKey">The interval endpoint type.</typeparam>
    /// <typeparam name="TValue">The stored type.</typeparam>
    internal class SortedIntervalList<TKey, TValue>
    {
        /// <summary>
        /// The underlying list.
        /// </summary>
        public List<TValue> Values { get; } = new();

        /// <summary>
        /// The comparer used.
        /// </summary>
        public IntervalComparer<TKey> Comparer { get; }

        private BoundComparer<TKey> BoundComparer => this.Comparer.BoundComparer;

        private readonly Func<TValue, Interval<TKey>> intervalSelector;
        private readonly Func<TValue, Interval<TKey>, TValue> intervalMover;

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedIntervalList{TStored, TValue}"/> class.
        /// </summary>
        /// <param name="comparer">The comparer to use.</param>
        /// <param name="intervalSelector">The function to select intervals from the elmenets.</param>
        /// <param name="intervalMover">The function that moves a value to a new interval.</param>
        public SortedIntervalList(
            IntervalComparer<TKey> comparer,
            Func<TValue, Interval<TKey>> intervalSelector,
            Func<TValue, Interval<TKey>, TValue> intervalMover)
        {
            this.Comparer = comparer;
            this.intervalSelector = intervalSelector;
            this.intervalMover = intervalMover;
        }

        /// <summary>
        /// Removes the values that <paramref name="interval"/> covers.
        /// </summary>
        /// <param name="interval">The interval of values to remove.</param>
        /// <returns>True, if any values were removed.</returns>
        public bool Remove(Interval<TKey> interval)
        {
            // An empty set or an empty removal is trivial
            if (this.Values.Count == 0 || this.Comparer.IsEmpty(interval)) return false;

            // Not empty, find all the intervals that are intersecting
            var (from, to) = this.IntersectingRange(interval);

            // If the removed interval intersects nothing, we are done
            if (from == to) return false;

            if (to - from == 1)
            {
                // Intersects a single interval
                var existing = this.Values[from];
                var existingLower = this.intervalSelector(existing).Lower;
                var existingUpper = this.intervalSelector(existing).Upper;
                var lowerCompare = this.BoundComparer.Compare(existingLower, interval.Lower);
                var upperCompare = this.BoundComparer.Compare(existingUpper, interval.Upper);
                if (lowerCompare >= 0 && upperCompare <= 0)
                {
                    // Simplest case, we just remove the entry, as the interval completely covers this one
                    this.Values.RemoveAt(from);
                }
                else if (lowerCompare >= 0)
                {
                    // The upper bound does not match, we need to modify
                    var newInterval = new Interval<TKey>(interval.Upper.Touching!, existingUpper);
                    this.Values[from] = this.intervalMover(existing, newInterval);
                }
                else if (upperCompare <= 0)
                {
                    // The lower bound does not match, we need to modify
                    var newInterval = new Interval<TKey>(existingLower, interval.Lower.Touching!);
                    this.Values[from] = this.intervalMover(existing, newInterval);
                }
                else
                {
                    // The interval is being split into 2
                    var newInterval1 = new Interval<TKey>(existingLower, interval.Lower.Touching!);
                    var newInterval2 = new Interval<TKey>(interval.Upper.Touching!, existingUpper);
                    this.Values[from] = this.intervalMover(existing, newInterval1);
                    this.Values.Insert(from + 1, this.intervalMover(existing, newInterval2));
                }
            }
            else
            {
                // Intersects multiple intervals
                // Let's look at the edge relations
                var lowerExisting = this.Values[from];
                var upperExisting = this.Values[to - 1];
                var lowerExistingLower = this.intervalSelector(lowerExisting).Lower;
                var upperExistingUpper = this.intervalSelector(upperExisting).Upper;
                var lowerCompare = this.BoundComparer.Compare(lowerExistingLower, interval.Lower);
                var upperCompare = this.BoundComparer.Compare(upperExistingUpper, interval.Upper);
                // Split edges if needed, track indices for deletion
                var deleteFrom = from;
                var deleteTo = to;
                if (lowerCompare < 0)
                {
                    // Need to split lower
                    var newLower = new Interval<TKey>(lowerExistingLower, interval.Lower.Touching!);
                    this.Values[from] = this.intervalMover(lowerExisting, newLower);
                    ++deleteFrom;
                }
                if (upperCompare > 0)
                {
                    // Need to split upper
                    var newUpper = new Interval<TKey>(interval.Upper.Touching!, upperExistingUpper);
                    this.Values[to - 1] = this.intervalMover(upperExisting, newUpper);
                    --deleteTo;
                }
                // Remove all fully removed intervals
                this.Values.RemoveRange(deleteFrom, deleteTo - deleteFrom);
            }
            return true;
        }

        /// <summary>
        /// Retrieves the range that is intersecting or at least touching a given interval.
        /// </summary>
        /// <param name="interval">The interval to check touch with.</param>
        /// <returns>The index range to index <see cref="Values"/> with.</returns>
        public (int From, int To) TouchingRange(Interval<TKey> interval)
        {
            var (from, to) = this.IntersectingRange(interval);
            var fromUpper = this.intervalSelector(this.Values[from - 1]).Upper;
            var toLower = this.intervalSelector(this.Values[to]).Lower;
            if (from != 0 && this.BoundComparer.IsTouching(interval.Lower, fromUpper)) from -= 1;
            if (to != this.Values.Count && this.BoundComparer.IsTouching(interval.Upper, toLower)) to += 1;
            return (from, to);
        }

        /// <summary>
        /// Retrieves the range that is intersecting a given interval.
        /// </summary>
        /// <param name="interval">The interval to check intersection with.</param>
        /// <returns>The index range to index <see cref="Values"/> with.</returns>
        public (int From, int To) IntersectingRange(Interval<TKey> interval)
        {
            var from = this.BinarySearch(0, interval.Lower, iv => iv.Upper);
            var to = this.BinarySearch(from, interval.Upper, iv => iv.Lower);
            return (from, to);
        }

        private int BinarySearch(int start, Bound<TKey> searchedKey, Func<Interval<TKey>, Bound<TKey>> keySelector)
        {
            var size = this.Values.Count - start;
            if (size == 0) return start;

            while (size > 1)
            {
                var half = size / 2;
                var mid = start + half;
                var key = keySelector(this.intervalSelector(this.Values[mid]));
                var cmp = this.BoundComparer.Compare(searchedKey, key);
                start = cmp > 0 ? mid : start;
                size -= half;
            }

            var resultKey = keySelector(this.intervalSelector(this.Values[start]));
            var resultCmp = this.BoundComparer.Compare(searchedKey, resultKey);
            return start + (resultCmp > 0 ? 1 : 0);
        }
    }
}