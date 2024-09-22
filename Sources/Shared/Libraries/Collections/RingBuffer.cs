// Copyright (c) 2021-2022 Yoakke.
// Licensed under the Apache License, Version 2.0.
// Source repository: https://github.com/LanguageDev/Yoakke

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Yoakke.Collections;

/// <summary>
/// A ring-buffer backed <see cref="IDeque{T}"/> implementation.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed class RingBuffer<T> : IDeque<T>
{
    private const int DefaultCapacity = 8;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public int Count { get; private set; }

    /// <summary>
    /// Gets or sets the capacity of the backing array.
    /// </summary>
    public int Capacity
    {
        get => this.storage.Length;
        set
        {
            // If we try to reduce it below the current count, throw
            if (value < this.Count) throw new ArgumentOutOfRangeException(nameof(value));

            // If capacity is already enough, early-return
            if (value <= this.Capacity) return;

            // If we reallocate, we use this occasion to reorder the elements to have the head in the front
            var newStorage = new T[value];
            var (first, second) = this.AsMemory();
            first.CopyTo(newStorage);
            second.CopyTo(newStorage.AsMemory(first.Length));
            this.Head = 0;
            this.storage = newStorage;
        }
    }

    /// <summary>
    /// The current head-index, which is the position of the first element.
    /// </summary>
    public int Head { get; private set; }

    /// <summary>
    /// The current tail-index, which is the position of the index after the last element.
    /// </summary>
    public int Tail => (this.Head + this.Count) % this.Capacity;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowOutOfRange()
    {
        throw new ArgumentOutOfRangeException("frog");
    }

    /// <inheritdoc/>
    public T this[int index]
    {
        get
        {
            if ((uint)index >= this.Count) ThrowOutOfRange();
            var nd = Head + index;
            if (nd >= this.Capacity) nd -= this.Capacity;
            return this.storage[nd];
        }

        set
        {
            if (index < 0 || index >= this.Count) throw new ArgumentOutOfRangeException(nameof(index));
            this.storage[(this.Head + index) % this.Capacity] = value;
        }
    }

    /// <inheritdoc/>
    T IReadOnlyList<T>.this[int index] => this[index];

    private T[] storage = Array.Empty<T>();

    /// <summary>
    /// Initializes a new instance of the <see cref="RingBuffer{T}"/> class that is empty and has the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The number of elements that the new ring buffer can initially store.</param>
    public RingBuffer(int capacity)
    {
        this.Capacity = capacity;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RingBuffer{T}"/> class that is empty and has the default capacity.
    /// </summary>
    public RingBuffer()
        : this(DefaultCapacity)
    {
    }

    /// <inheritdoc/>
    public void Clear()
    {
        var (first, second) = this.AsMemory();
        first.Span.Clear();
        second.Span.Clear();
        this.Head = 0;
        this.Count = 0;
    }

    /// <inheritdoc/>
    public void Add(T item) => this.AddBack(item);

    /// <inheritdoc/>
    public void AddFront(T item)
    {
        this.EnsureCapacity(this.Count + 1);
        this.Head = this.Head == 0 ? this.Capacity - 1 : this.Head - 1;
        this.storage[this.Head] = item;
        ++this.Count;
    }

    /// <inheritdoc/>
    public void AddBack(T item)
    {
        this.EnsureCapacity(this.Count + 1);
        this.storage[this.Tail] = item;
        ++this.Count;
    }

    /// <inheritdoc/>
    public void Insert(int index, T item)
    {
        if (index < 0 || index > this.Count) throw new ArgumentOutOfRangeException(nameof(index));

        // If we are adding to the back, use the handler for that
        if (index == this.Count)
        {
            this.AddBack(item);
            return;
        }
        // Same with adding to the front
        if (index == 0)
        {
            this.AddFront(item);
            return;
        }

        // We are inserting to somewhere in the middle
        this.EnsureCapacity(this.Count + 1);
        var absIndex = (this.Head + index) % this.Capacity;

        if (this.Head < this.Tail || absIndex < this.Tail)
        {
            // Buffer is in one piece, or it's being split into two, but insertion is in the latter piece
            for (var i = this.Tail; i > absIndex; --i) this.storage[i] = this.storage[i - 1];
        }
        else
        {
            // Buffer is split into two and it's in the first section
            for (var i = this.Tail; i > 0; --i) this.storage[i] = this.storage[i - 1];
            this.storage[0] = this.storage[^1];
            for (var i = this.Capacity - 1; i > absIndex; --i) this.storage[i] = this.storage[i - 1];
        }
        ++this.Count;
        this.storage[absIndex] = item;
    }

    /// <inheritdoc/>
    public bool Remove(T item)
    {
        var index = this.IndexOf(item);

        if (index == -1) return false;

        this.RemoveAt(index);
        return true;
    }

    /// <inheritdoc/>
    public T RemoveFront()
    {
        if (this.Count == 0) throw new InvalidOperationException("The ring buffer was empty");

        var result = this.storage[this.Head];
        this.storage[this.Head] = default!;
        this.Head = (this.Head + 1) % this.Capacity;
        --this.Count;
        return result;
    }

    /// <inheritdoc/>
    public T RemoveBack()
    {
        if (this.Count == 0) throw new InvalidOperationException("The ring buffer was empty");

        var index = this.Tail - 1;
        var result = this.storage[index];
        this.storage[index] = default!;
        --this.Count;
        return result!;
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
        if (this.Count == 0) throw new InvalidOperationException("The ring buffer was empty");
        if (index < 0 || index >= this.Count) throw new ArgumentOutOfRangeException(nameof(index));

        // If we are removing from the back, use the handler for that
        if (index == this.Count - 1)
        {
            this.RemoveBack();
            return;
        }
        // Same with removing from front
        if (index == 0)
        {
            this.RemoveFront();
            return;
        }

        // We are removing from some middle
        var absIndex = (this.Head + index) % this.Capacity;
        if (this.Head < this.Tail || absIndex < this.Tail)
        {
            // The underlying buffer is still in one piece
            // Or the buffer is split but we are removing from the second portion
            for (var i = absIndex; i < this.Tail - 1; ++i) this.storage[i] = this.storage[i + 1];
            this.storage[this.Tail - 1] = default!;
        }
        else
        {
            // The buffer is split and we are removing from the first part, potential wrapping
            for (var i = absIndex; i < this.Capacity - 1; ++i) this.storage[i] = this.storage[i + 1];
            if (this.Tail > 0)
            {
                this.storage[^1] = this.storage[0];
                for (var i = 0; i < this.Tail - 1; ++i) this.storage[i] = this.storage[i + 1];
                this.storage[this.Tail - 1] = default!;
            }
        }
        --this.Count;
    }

    /// <inheritdoc/>
    public bool Contains(T item) => this.IndexOf(item) != -1;

    /// <inheritdoc/>
    public int IndexOf(T item)
    {
        var (first, second) = this.AsMemory();
        var index = 0;
        for (var i = 0; i < first.Length; ++i, ++index)
        {
            if (EqualityComparer<T>.Default.Equals(item, first.Span[i])) return index;
        }
        for (var i = 0; i < second.Length; ++i, ++index)
        {
            if (EqualityComparer<T>.Default.Equals(item, second.Span[i])) return index;
        }
        return -1;
    }

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
        var (first, second) = this.AsMemory();
        first.CopyTo(array.AsMemory(arrayIndex));
        second.CopyTo(array.AsMemory(arrayIndex + first.Length));
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        var (first, second) = this.AsMemory();
        for (var i = 0; i < first.Length; ++i) yield return first.Span[i];
        for (var i = 0; i < second.Length; ++i) yield return second.Span[i];
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <summary>
    /// Ensures that the capacity of this ring buffer is at least the specified <paramref name="capacity"/>.
    /// If the current capacity of the ring buffer is less than specified <paramref name="capacity"/>,
    /// the capacity is increased by continuously twice current capacity until it is at least the
    /// specified <paramref name="capacity"/>.
    /// </summary>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    /// <returns>The new capacity of this ring buffer.</returns>
    public int EnsureCapacity(int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        if (this.Capacity < capacity)
        {
            var newCapacity = this.Capacity == 0 ? DefaultCapacity : this.Capacity * 2;
            newCapacity = Math.Max(newCapacity, capacity);
            this.Capacity = newCapacity;
        }
        return this.Capacity;
    }

    private (Memory<T> First, Memory<T> Second) AsMemory()
    {
        // For empty buffer, just return an empty pair
        if (this.Count == 0) return (Memory<T>.Empty, Memory<T>.Empty);

        // For non-empty buffers we can assum that if tail <= head, then the buffer is split in 2
        return this.Tail > this.Head
            ? (this.storage.AsMemory(this.Head, this.Count), Memory<T>.Empty)
            : (this.storage.AsMemory(this.Head, this.Capacity - this.Head), this.storage.AsMemory(0, this.Tail));
    }
}
