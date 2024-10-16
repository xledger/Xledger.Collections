namespace Xledger.Collections;

public static class ImmArray {
    public static ImmArray<T> Of<T>(params T[] arr) {
        return arr.ToImmArray();
    }

#if NET
    public static ImmArray<T> Of<T>(ReadOnlySpan<T> span) {
        return span.ToImmArray();
    }
#endif
}

[Serializable]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(ImmArray<>.DebugView))]
#if NET8_0_OR_GREATER
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ImmArray), nameof(ImmArray.Of))]
#endif
public sealed class ImmArray<T> : IReadOnlyList<T>, IEquatable<ImmArray<T>>, IList<T>,
    ICollection, IComparable, IComparable<ImmArray<T>>, IStructuralComparable
{
    public static readonly ImmArray<T> Empty = new();

    readonly T[] data;
    [NonSerialized]
    int hash;
    [NonSerialized]
    bool hashComputed;

    internal ImmArray() {
        this.data = [];
    }

    /// <summary>
    /// Unsafely constructs an ImmArray by taking ownership of the parameter array.
    /// </summary>
    /// <remarks>But that's why it's internal so we can guarantee this is only called sanely.</remarks>
    internal ImmArray(T[] data) {
        if (data is null) {
            throw new ArgumentNullException(nameof(data));
        }
        this.data = data;
    }

    /// <summary>
    /// Constructs an ImmArray by copying the enumerable contents.
    /// </summary>
    public ImmArray(IEnumerable<T> data) {
        if (data is null) {
            this.data = [];
        } else {
            this.data = Extensions.ArrayOf(data);
        }
    }

    /// <inheritdoc />
    public T this[int index] => this.data[index];

    /// <inheritdoc />
    public int Count => this.data.Length;

    public int Length => this.data.Length;

    /// <inheritdoc />
    public int CompareTo(ImmArray<T> other) {
        return StructuralComparisons.StructuralComparer.Compare(this.data, other?.data);
    }

    /// <inheritdoc />
    public bool Equals(ImmArray<T> other) {
        if (other is null) {
            return false;
        }
        if (this.data.Length != other.data.Length) {
            return false;
        }
        if (this.hashComputed && other.hashComputed && this.hash != other.hash) {
            return false;
        }

        for (var i = 0; i < this.data.Length; ++i) {
            if (!EqualityComparer<T>.Default.Equals(this.data[i], other.data[i])) {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object obj) => Equals(obj as ImmArray<T>);

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() {
        return ((IEnumerable<T>)this.data).GetEnumerator();
    }

    /// <inheritdoc />
    public override int GetHashCode() {
        if (!this.hashComputed) {
            var hashCode = 1343695044;
            for (var i = 0; i < this.data.Length; ++i) {
                var next = EqualityComparer<T>.Default.GetHashCode(this.data[i]);
                hashCode = hashCode * -1521134295 + next;
            }
            this.hash = hashCode;
            this.hashComputed = true;
        }
        return this.hash;
    }

    /// <inheritdoc />
    public override string ToString() {
        return $"[{string.Join(", ", this.data)}]";
    }

    /// <inheritdoc />
    public bool Contains(T item) {
        return this.data.Length > 0 && IndexOf(item) >= 0;
    }

    /// <inheritdoc />
    public int IndexOf(T item) {
        return Array.IndexOf<T>(this.data, item);
    }

    /// <summary>
    /// Creates a shallow copy of the stored data.
    /// </summary>
    public T[] ToArray() {
        return (T[])this.data.Clone();
    }

    /// <inheritdoc />
    public void CopyTo(T[] array) {
        CopyTo(0, array, 0, this.data.Length);
    }

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex) {
        CopyTo(0, array, arrayIndex, this.data.Length);
    }

    /// <summary>
    /// Copies a range of elements to a target array, starting
    /// at the specified index of the target array.
    /// </summary>
    public void CopyTo(int index, T[] array, int arrayIndex, int count) {
        if (index + count > this.data.Length) {
            throw new ArgumentException(
                "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
        }
        Array.Copy(this.data, index, array, arrayIndex, count);
    }

    public static bool operator ==(ImmArray<T> left, ImmArray<T> right) {
        return EqualityComparer<ImmArray<T>>.Default.Equals(left, right);
    }

    public static bool operator !=(ImmArray<T> left, ImmArray<T> right) {
        return !(left == right);
    }

    public static bool operator <(ImmArray<T> left, ImmArray<T> right) {
        return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
    }

    public static bool operator <=(ImmArray<T> left, ImmArray<T> right) {
        return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
    }

    public static bool operator >(ImmArray<T> left, ImmArray<T> right) {
        return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
    }

    public static bool operator >=(ImmArray<T> left, ImmArray<T> right) {
        return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
    }

    #region Explicit implementations
    T IList<T>.this[int index] {
        get => this.data[index];
        set => throw new NotSupportedException();
    }

    bool ICollection<T>.IsReadOnly => true;

    object ICollection.SyncRoot => this;

    bool ICollection.IsSynchronized => false;

    void ICollection<T>.Add(T item) {
        throw new NotSupportedException();
    }

    void ICollection<T>.Clear() {
        throw new NotSupportedException();
    }

    int IComparable.CompareTo(object obj) {
        return CompareTo(obj as ImmArray<T>);
    }

    int IStructuralComparable.CompareTo(object other, IComparer comparer) {
        return ((IStructuralComparable)this.data).CompareTo(other, comparer);
    }

    void ICollection.CopyTo(Array array, int index) {
        throw new NotSupportedException();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return this.data.GetEnumerator();
    }

    void IList<T>.Insert(int index, T item) {
        throw new NotSupportedException();
    }

    bool ICollection<T>.Remove(T item) {
        throw new NotSupportedException();
    }

    void IList<T>.RemoveAt(int index) {
        throw new NotSupportedException();
    }
    #endregion

    internal class DebugView(ImmArray<T> array) {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => array.data;
    }
}
