// Copyright (c) 2021 Yoakke.
// Licensed under the Apache License, Version 2.0.
// Source repository: https://github.com/LanguageDev/Yoakke

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Yoakke.Collections.Intervals;

namespace Yoakke.Collections.Dense
{
    /// <summary>
    /// A default <see cref="IDenseSet{T}"/> implementation backed by a list of intervals.
    /// </summary>
    /// <typeparam name="T">The set element and interval endpoint type.</typeparam>
    public sealed class DenseSet<T> : IDenseSet<T>
    {
        /// <inheritdoc/>
        public bool IsEmpty => this.intervals.Count > 0;

        // NOTE: We can implement these later
        // For that we need some domain information with some IDomain<T> interface

        /// <inheritdoc/>
        public int? Count => null;

        // NOTE: See above note

        /// <inheritdoc/>
        public IEnumerable<T>? Values => null;

        /// <summary>
        /// The comparer used.
        /// </summary>
        public IntervalComparer<T> Comparer { get; }

        private readonly List<Interval<T>> intervals = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseSet{T}"/> class.
        /// </summary>
        public DenseSet()
            : this(IntervalComparer<T>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseSet{T}"/> class.
        /// </summary>
        /// <param name="comparer">The comparer to use.</param>
        public DenseSet(IntervalComparer<T> comparer)
        {
            this.Comparer = comparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseSet{T}"/> class.
        /// </summary>
        /// <param name="comparer">The comparer to use.</param>
        public DenseSet(BoundComparer<T> comparer)
            : this(new IntervalComparer<T>(comparer))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseSet{T}"/> class.
        /// </summary>
        /// <param name="equalityComparer">The equality comparer to use.</param>
        /// <param name="comparer">The comparer to use.</param>
        public DenseSet(IEqualityComparer<T> equalityComparer, IComparer<T> comparer)
            : this(new BoundComparer<T>(equalityComparer, comparer))
        {
        }

        #region Elemental Operations

        /// <inheritdoc/>
        public void Clear() => this.intervals.Clear();

        /// <inheritdoc/>
        public bool Add(T item) => this.Add(Interval<T>.Singleton(item));

        /// <inheritdoc/>
        public bool Add(Interval<T> interval)
        {
            if (this.Comparer.IsEmpty(interval)) return false;

            // For an empty set, it's trivial, we just add it
            if (this.intervals.Count == 0)
            {
                this.intervals.Add(interval);
                return true;
            }

            // Not empty, find all the intervals that are touched
            var (from, to) = this.TouchingRange(interval);

            if (from == to)
            {
                // Just insert, touches nothing
                this.intervals.Insert(from, interval);
                return true;
            }
            else
            {
                // We need to remove all touched intervals
                // First we need to modify the inserted interval, because the touched intervals might extend beyond
                // the inserted one
                var firstLower = this.intervals[from].Lower;
                var lastUpper = this.intervals[to - 1].Upper;
                var minLower = this.Comparer.BoundComparer.Min(firstLower, interval.Lower);
                var maxUpper = this.Comparer.BoundComparer.Max(lastUpper, interval.Upper);
                // There are 3 ways we can cover a new value
                //  - There are more than 1 touched intervals, because that means we fill out values in between
                //  - The first touched intervals lower value is below the inserted one
                //  - The last touched intervals upper value is above the inserted one
                var coversNewValue = to - from > 1
                                 || !this.Comparer.BoundComparer.Equals(minLower, interval.Lower)
                                 || !this.Comparer.BoundComparer.Equals(maxUpper, interval.Upper);
                if (!coversNewValue) return false;
                // It covers something new, we need to do the insertion
                interval = new(minLower, maxUpper);
                // Remove all touched ranges, except the first one
                this.intervals.RemoveRange(from + 1, to - from - 1);
                // We modify the first, not removed touched range to save on memory juggling a bit
                this.intervals[from] = interval;
                return true;
            }
        }

        /// <inheritdoc/>
        public bool Remove(T item) => this.Remove(Interval<T>.Singleton(item));

        /// <inheritdoc/>
        public bool Remove(Interval<T> interval)
        {
            // An empty set or an empty removal is trivial
            if (this.intervals.Count == 0 || this.Comparer.IsEmpty(interval)) return false;

            // Not empty, find all the intervals that are intersecting
            var (from, to) = this.IntersectingRange(interval);

            // If the removed interval intersects nothing, we are done
            if (from == to) return false;

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Complement()
        {
            if (this.intervals.Count == 0)
            {
                // Inverse of the empty set is the full interval
                this.intervals.Add(Interval<T>.Full);
                return;
            }

            if (this.intervals.Count == 1
             && this.intervals[0].Lower is LowerBound<T>.Unbounded
             && this.intervals[0].Upper is UpperBound<T>.Unbounded)
            {
                // Inverse of full interval is the empty one
                this.intervals.Clear();
                return;
            }

            // The interval set is neither empty, nor full, there are 3 cases:
            //  - Both ends are unbounded: for N intervals this creates N - 1 intervals when inverted
            //  - One end is unbounded: for N intervals this creates N intervals when inverted
            //  - Both ends are bounded: for N intervals this creates N + 1 intervals when inverted
            var lowerUnbounded = this.intervals[0].Lower is LowerBound<T>.Unbounded;
            var upperUnbounded = this.intervals[^1].Upper is UpperBound<T>.Unbounded;

            if (lowerUnbounded && upperUnbounded)
            {
                var nIntervals = this.intervals.Count - 1;
                for (var i = 0; i < nIntervals; ++i)
                {
                    var lower = this.intervals[i].Upper.Touching!;
                    var upper = this.intervals[i + 1].Lower.Touching!;
                    this.intervals[i] = new(lower, upper);
                }
                this.intervals.RemoveAt(this.intervals.Count - 1);
            }
            else if (lowerUnbounded)
            {
                var nIntervals = this.intervals.Count;
                if (nIntervals > 1)
                {
                    var prevTouch = this.intervals[nIntervals - 1].Lower.Touching!;

                    // Modify the last one
                    var lastLower = this.intervals[nIntervals - 1].Upper.Touching!;
                    this.intervals[nIntervals - 1] = new(lastLower, UpperBound<T>.Unbounded.Instance);

                    for (var i = nIntervals - 2; i > 0; --i)
                    {
                        var loTouch = prevTouch;
                        var hiTouch = this.intervals[i].Upper.Touching!;
                        prevTouch = this.intervals[i].Lower.Touching!;
                        this.intervals[i] = new(hiTouch, loTouch);
                    }

                    // First one
                    var hTouch = this.intervals[0].Upper.Touching!;
                    this.intervals[0] = new(hTouch, prevTouch);
                }
                else
                {
                    // Modify the only one
                    var lower = this.intervals[0].Upper.Touching!;
                    this.intervals[0] = new(lower, UpperBound<T>.Unbounded.Instance);
                }
            }
            else if (upperUnbounded)
            {
                var nIntervals = this.intervals.Count;
                if (nIntervals > 1)
                {
                    var prevTouch = this.intervals[0].Upper.Touching!;

                    // Modify the first one
                    var firstUpper = this.intervals[0].Lower.Touching!;
                    this.intervals[0] = new(LowerBound<T>.Unbounded.Instance, firstUpper);

                    for (var i = 1; i < nIntervals - 1; ++i)
                    {
                        var loTouch = this.intervals[i].Lower.Touching!;
                        var hiTouch = prevTouch;
                        prevTouch = this.intervals[i].Upper.Touching!;
                        this.intervals[i] = new(hiTouch, loTouch);
                    }

                    // Last one
                    var lTouch = this.intervals[nIntervals - 1].Lower.Touching!;
                    this.intervals[nIntervals - 1] = new(prevTouch, lTouch);
                }
                else
                {
                    // Modify the only one
                    var upper = this.intervals[0].Lower.Touching!;
                    this.intervals[0] = new(LowerBound<T>.Unbounded.Instance, upper);
                }
            }
            else
            {
                // Bounded, meaning N + 1 entries
                var nIntervals = this.intervals.Count;

                // Add a last entry
                this.intervals.Add(new(this.intervals[^1].Upper.Touching!, UpperBound<T>.Unbounded.Instance));

                var prevTouch = this.intervals[0].Upper.Touching!;

                // Modify first one
                var firstUpper = this.intervals[0].Lower.Touching!;
                this.intervals[0] = new(LowerBound<T>.Unbounded.Instance, firstUpper);

                for (var i = 1; i < nIntervals; ++i)
                {
                    var loTouch = this.intervals[i].Lower.Touching!;
                    var upper = prevTouch;
                    prevTouch = this.intervals[i].Upper.Touching!;
                    this.intervals[i] = new(upper, loTouch);
                }
            }
        }

        /// <inheritdoc/>
        public bool Contains(T item) => this.Contains(Interval<T>.Singleton(item));

        /// <inheritdoc/>
        public bool Contains(Interval<T> interval) =>
            this.TryGetIntersecting(interval, out var existing) && this.Comparer.Contains(existing, interval);

        /// <inheritdoc/>
        public bool Overlaps(Interval<T> interval) => this.TryGetIntersecting(interval, out _);

        #endregion

        #region Set Relation

        /// <inheritdoc/>
        public bool IsProperSubsetOf(IEnumerable<T> other) => this.IsProperSubsetOf(other.Select(i => Interval<T>.Singleton(i)));

        /// <inheritdoc/>
        public bool IsProperSubsetOf(IEnumerable<Interval<T>> other) => this.IsSubsetOf(other, out var proper) && proper;

        /// <inheritdoc/>
        public bool IsProperSupersetOf(IEnumerable<T> other) => this.IsProperSupersetOf(other.Select(i => Interval<T>.Singleton(i)));

        /// <inheritdoc/>
        public bool IsProperSupersetOf(IEnumerable<Interval<T>> other) => throw new NotImplementedException();

        /// <inheritdoc/>
        public bool IsSubsetOf(IEnumerable<T> other) => this.IsSubsetOf(other.Select(i => Interval<T>.Singleton(i)));

        /// <inheritdoc/>
        // NOTE: Quite inefficient, but correct for now
        public bool IsSubsetOf(IEnumerable<Interval<T>> other) => this.MakeDenseSet(other).IsSupersetOf(this);

        /// <inheritdoc/>
        public bool IsSubsetOf(IEnumerable<T> other, out bool proper) => throw new NotImplementedException();

        /// <inheritdoc/>
        public bool IsSubsetOf(IEnumerable<Interval<T>> other, out bool proper) => throw new NotImplementedException();

        /// <inheritdoc/>
        public bool IsSupersetOf(IEnumerable<T> other) => this.IsSupersetOf(other.Select(i => Interval<T>.Singleton(i)));

        /// <inheritdoc/>
        public bool IsSupersetOf(IEnumerable<Interval<T>> other)
        {
            foreach (var iv in other)
            {
                if (!this.Contains(iv)) return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public bool IsSupersetOf(IEnumerable<T> other, out bool proper) => throw new NotImplementedException();

        /// <inheritdoc/>
        public bool IsSupersetOf(IEnumerable<Interval<T>> other, out bool proper) => throw new NotImplementedException();

        /// <inheritdoc/>
        public bool Overlaps(IEnumerable<T> other) => this.Overlaps(other.Select(i => Interval<T>.Singleton(i)));

        /// <inheritdoc/>
        public bool Overlaps(IEnumerable<Interval<T>> other)
        {
            foreach (var iv in other)
            {
                if (this.Overlaps(iv)) return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public bool SetEquals(IEnumerable<T> other) => this.SetEquals(other.Select(i => Interval<T>.Singleton(i)));

        /// <inheritdoc/>
        public bool SetEquals(IEnumerable<Interval<T>> other)
        {
            foreach (var iv in other)
            {
                if (!this.TryGetIntersecting(iv, out var existing) || !this.Comparer.Equals(iv, existing)) return false;
            }
            return true;
        }

        #endregion

        #region Set Operations

        /// <inheritdoc/>
        public void ExceptWith(IEnumerable<T> other) => this.ExceptWith(other.Select(i => Interval<T>.Singleton(i)));

        /// <inheritdoc/>
        public void ExceptWith(IEnumerable<Interval<T>> other)
        {
            foreach (var iv in other) this.Remove(iv);
        }

        /// <inheritdoc/>
        public void IntersectWith(IEnumerable<T> other) => this.IntersectWith(other.Select(i => Interval<T>.Singleton(i)));

        /// <inheritdoc/>
        public void IntersectWith(IEnumerable<Interval<T>> other) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void SymmetricExceptWith(IEnumerable<T> other) => this.SymmetricExceptWith(other.Select(i => Interval<T>.Singleton(i)));

        /// <inheritdoc/>
        public void SymmetricExceptWith(IEnumerable<Interval<T>> other) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void UnionWith(IEnumerable<T> other) => this.UnionWith(other.Select(i => Interval<T>.Singleton(i)));

        /// <inheritdoc/>
        public void UnionWith(IEnumerable<Interval<T>> other)
        {
            foreach (var iv in other) this.Add(iv);
        }

        #endregion

        /// <inheritdoc/>
        public IEnumerator<Interval<T>> GetEnumerator() => this.intervals.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => (this.intervals as IEnumerable).GetEnumerator();

        private bool TryGetTouching(Interval<T> interval, [MaybeNullWhen(false)] out Interval<T> existing)
        {
            var (from, to) = this.TouchingRange(interval);
            if (from == to || to - from > 1)
            {
                existing = null;
                return false;
            }
            else
            {
                existing = this.intervals[from];
                return true;
            }
        }

        private bool TryGetIntersecting(Interval<T> interval, [MaybeNullWhen(false)] out Interval<T> existing)
        {
            var (from, to) = this.IntersectingRange(interval);
            if (from == to || to - from > 1)
            {
                existing = null;
                return false;
            }
            else
            {
                existing = this.intervals[from];
                return true;
            }
        }

        private (int From, int To) TouchingRange(Interval<T> interval)
        {
            var (from, to) = this.IntersectingRange(interval);
            if (from != 0 && this.Comparer.BoundComparer.IsTouching(interval.Lower, this.intervals[from - 1].Upper)) from -= 1;
            if (to != this.intervals.Count && this.Comparer.BoundComparer.IsTouching(interval.Upper, this.intervals[to].Lower)) to += 1;
            return (from, to);
        }

        private (int From, int To) IntersectingRange(Interval<T> interval)
        {
            var from = this.BinarySearch(0, interval.Lower, iv => iv.Upper);
            var to = this.BinarySearch(from, interval.Upper, iv => iv.Lower);
            return (from, to);
        }

        private int BinarySearch(int start, Bound<T> searchedKey, Func<Interval<T>, Bound<T>> keySelector)
        {
            var size = this.intervals.Count - start;
            if (size == 0) return 0;

            while (size > 1)
            {
                var half = size / 2;
                var mid = start + half;
                var key = keySelector(this.intervals[mid]);
                var cmp = this.Comparer.BoundComparer.Compare(searchedKey, key);
                start = cmp > 0 ? mid : start;
                size -= half;
            }

            var resultKey = keySelector(this.intervals[start]);
            var resultCmp = this.Comparer.BoundComparer.Compare(searchedKey, resultKey);
            return start + (resultCmp > 0 ? 1 : 0);
        }

        private IReadOnlyDenseSet<T> MakeDenseSet(IEnumerable<Interval<T>> intervals)
        {
            // if (intervals is IReadOnlyDenseSet<T> set) return set;
            var result = new DenseSet<T>(this.Comparer);
            foreach (var iv in intervals) result.Add(iv);
            return result;
        }
    }
}