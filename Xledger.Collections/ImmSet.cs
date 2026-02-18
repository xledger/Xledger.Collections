namespace Xledger.Collections;

public static class ImmSet {
    public static ImmSet<T> Of<T>(params T[] arr) {
        return arr.ToImmSet();
    }

#if NET
#if NET10_0_OR_GREATER
    [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
    public static ImmSet<T> Of<T>(params ReadOnlySpan<T> span) {
#else
    public static ImmSet<T> Of<T>(ReadOnlySpan<T> span) {
#endif
        return span.ToImmSet();
    }
#endif

    public readonly ref struct SetBuilder<T> {
        internal readonly HashSet<T> data;

        public SetBuilder() { this.data = []; }
        public SetBuilder(int capacity) { this.data = new(capacity); }

        public int Count => this.data.Count;

        public bool Contains(T item) => this.data.Contains(item);

        public bool TryGetValue(T equalValue, out T actualValue) =>
            this.data.TryGetValue(equalValue, out actualValue);

        public bool Add(T item) => this.data.Add(item);

        public void AddRange(IEnumerable<T> items) {
            if (items is null) {
                return;
            }

#if NET
            if (items is ICollection<T> coll) {
                this.data.EnsureCapacity(this.data.Count + coll.Count);
            } else if (items is IReadOnlyCollection<T> roColl) {
                this.data.EnsureCapacity(this.data.Count + roColl.Count);
            }
#endif

            foreach (var item in items) {
                this.data.Add(item);
            }
        }
    }

#if NET10_0_OR_GREATER
    [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
    public static ImmSet<T> New<T>(Action<SetBuilder<T>> fill) {
        var set = new SetBuilder<T>();
        fill(set);
        return new ImmSet<T>(set.data);
    }

    [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
    public static ImmSet<T> New<T>(int capacity, Action<SetBuilder<T>> fill) {
        var set = new SetBuilder<T>(capacity);
        fill(set);
        return new ImmSet<T>(set.data);
    }
#endif

    public delegate void BuildSet<T>(SetBuilder<T> set);

    public static ImmSet<T> New<T>(BuildSet<T> fill) {
        var set = new SetBuilder<T>();
        fill(set);
        return new ImmSet<T>(set.data);
    }

    public static ImmSet<T> New<T>(int capacity, BuildSet<T> fill) {
        var set = new SetBuilder<T>(capacity);
        fill(set);
        return new ImmSet<T>(set.data);
    }
}

[Serializable]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(ImmSet<>.DebugView))]
#if NET8_0_OR_GREATER
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ImmSet), nameof(ImmArray.Of))]
#endif
public sealed class ImmSet<T> : IReadOnlyCollection<T>, ISet<T>, IEquatable<ImmSet<T>>, ICollection
#if NET6_0_OR_GREATER
    , IReadOnlySet<T>
#endif
{
    static readonly HashSet<T> EmptyHashSet = [];
    public static readonly ImmSet<T> Empty = new();

    readonly HashSet<T> data;
    [NonSerialized]
    int hash;
    [NonSerialized]
    bool hashComputed;

    internal ImmSet() {
        this.data = EmptyHashSet;
    }

    /// <summary>
    /// Unsafely constructs an ImmSet by taking ownership of the parameter HashSet.
    /// </summary>
    /// <remarks>But that's why it's internal so we can guarantee this is only called sanely.</remarks>
    internal ImmSet(HashSet<T> data) {
        if (data is null) {
            throw new ArgumentNullException(nameof(data));
        }
        this.data = data;
    }

    /// <summary>
    /// Constructs an ImmSet by copying the enumerable contents.
    /// </summary>
    public ImmSet(IEnumerable<T> data) {
        if (data is null) {
            this.data = EmptyHashSet;
        } else {
            this.data = new HashSet<T>(data);
        }
    }

    /// <inheritdoc />
    public int Count => this.data.Count;

    /// <inheritdoc />
    public bool Contains(T item) {
        return this.data.Contains(item);
    }

    /// <summary>
    /// Creates a shallow copy of the stored data.
    /// </summary>
    public T[] ToArray() {
        var arr = new T[Count];
        CopyTo(arr, 0);
        return arr;
    }

    /// <inheritdoc />
    public void CopyTo(T[] array) {
        this.data.CopyTo(array);
    }

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex) {
        this.data.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public bool Equals(ImmSet<T> other) {
        if (other is null) {
            return false;
        } 
        if (this.data.Count != other.data.Count) {
            return false;
        }
        if (this.hashComputed && other.hashComputed && this.hash != other.hash) {
            return false;
        }

        return this.data.SetEquals(other.data);
    }

    /// <inheritdoc />
    public override bool Equals(object obj) => Equals(obj as ImmSet<T>);

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() {
        return this.data.GetEnumerator();
    }

    /// <inheritdoc />
    public override int GetHashCode() {
        if (!this.hashComputed) {
            var hashCode = 358481680;
            var hashes = new int[this.data.Count];
            var i = 0;
            foreach (var item in this) {
                hashes[i] = EqualityComparer<T>.Default.GetHashCode(item);
                ++i;
            }
            Array.Sort(hashes);
            for (i = 0; i < this.data.Count; ++i) {
                hashCode = hashCode * -1521134295 + hashes[i];
            }
            this.hash = hashCode;
            this.hashComputed = true;
        }
        return this.hash;
    }

    /// <inheritdoc />
    public bool IsProperSubsetOf(IEnumerable<T> other) {
        if (other is ImmSet<T> immSet) {
            return this.data.IsProperSubsetOf(immSet.data);
        } else {
            return this.data.IsProperSubsetOf(other);
        }
    }

    /// <inheritdoc />
    public bool IsProperSupersetOf(IEnumerable<T> other) {
        if (other is ImmSet<T> immSet) {
            return this.data.IsProperSupersetOf(immSet.data);
        } else {
            return this.data.IsProperSupersetOf(other);
        }
    }

    /// <inheritdoc />
    public bool IsSubsetOf(IEnumerable<T> other) {
        if (other is ImmSet<T> immSet) {
            return this.data.IsSubsetOf(immSet.data);
        } else {
            return this.data.IsSubsetOf(other);
        }
    }

    /// <inheritdoc />
    public bool IsSupersetOf(IEnumerable<T> other) {
        if (other is ImmSet<T> immSet) {
            return this.data.IsSupersetOf(immSet.data);
        } else {
            return this.data.IsSupersetOf(other);
        }
    }

    /// <inheritdoc />
    public bool Overlaps(IEnumerable<T> other) {
        if (other is ImmSet<T> immSet) {
            return this.data.Overlaps(immSet.data);
        } else {
            return this.data.Overlaps(other);
        }
    }

    /// <inheritdoc />
    public bool SetEquals(IEnumerable<T> other) {
        if (other is ImmSet<T> immSet) {
            return this.data.SetEquals(immSet.data);
        } else {
            return this.data.SetEquals(other);
        }
    }

    /// <summary>
    /// Searches the set for a given value and returns the equal value it finds, if any.
    /// </summary>
    public bool TryGetValue(T equalValue, out T actualValue) {
        return this.data.TryGetValue(equalValue, out actualValue);
    }

    /// <inheritdoc />
    public override string ToString() {
        return $"[{string.Join(", ", this.data)}]";
    }

    public static bool operator ==(ImmSet<T> left, ImmSet<T> right) {
        return EqualityComparer<ImmSet<T>>.Default.Equals(left, right);
    }

    public static bool operator !=(ImmSet<T> left, ImmSet<T> right) {
        return !(left == right);
    }

    #region Explicit implementations
    bool ICollection<T>.IsReadOnly => true;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;

    void ICollection<T>.Add(T item) {
        throw new NotSupportedException();
    }

    bool ISet<T>.Add(T item) {
        throw new NotSupportedException();
    }

    void ICollection<T>.Clear() {
        throw new NotSupportedException();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return this.data.GetEnumerator();
    }

    void ISet<T>.ExceptWith(IEnumerable<T> other) {
        throw new NotSupportedException();
    }

    void ISet<T>.IntersectWith(IEnumerable<T> other) {
        throw new NotSupportedException();
    }

    bool ICollection<T>.Remove(T item) {
        throw new NotSupportedException();
    }

    void ISet<T>.SymmetricExceptWith(IEnumerable<T> other) {
        throw new NotSupportedException();
    }

    void ISet<T>.UnionWith(IEnumerable<T> other) {
        throw new NotSupportedException();
    }

    void ICollection.CopyTo(Array array, int index) {
        throw new NotSupportedException();
    }
    #endregion

    internal class DebugView(ImmSet<T> set) {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public HashSet<T> Items => set.data;
    }
}
