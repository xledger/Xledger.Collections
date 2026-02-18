namespace Xledger.Collections;

public static class ImmDict {
    public static ImmDict<K, V> Of<K, V>(params KeyValuePair<K, V>[] arr) {
        return arr.ToImmDict();
    }

    public static ImmDict<K, V> Of<K, V>(params (K, V)[] arr) {
        return arr.ToImmDict();
    }

    //#if NET10_0_OR_GREATER
    //    [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
    //    public static ImmDict<K, V> Of<K, V>(params ReadOnlySpan<(K, V)> span) {
    //        return span.ToImmDict();
    //    }
    //#endif

    public readonly ref struct DictBuilder<K, V> {
        internal readonly Dictionary<K, V> data;

        public DictBuilder() { this.data = []; }
        public DictBuilder(int capacity) { this.data = new(capacity); }

        public int Count => this.data.Count;

        public V this[K key] {
            get => this.data[key];
            set => this.data[key] = value;
        }

        public void Add(K key, V value) => this.data.Add(key, value);

        //public void Insert(K key, V value) => this.xs.A

        //public void AddRange(IEnumerable<T> ys) => this.xs.AddRange(ys);
    }

#if NET10_0_OR_GREATER
    [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
    public static ImmDict<K, V> New<K, V>(Action<DictBuilder<K, V>> fill) {
        var dict = new DictBuilder<K, V>();
        fill(dict);
        return new ImmDict<K, V>(dict.data);
    }

    [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
    public static ImmDict<K, V> New<K, V>(int capacity, Action<DictBuilder<K, V>> fill) {
        var dict = new DictBuilder<K, V>(capacity);
        fill(dict);
        return new ImmDict<K, V>(dict.data);
    }
#endif

    public delegate void BuildDict<K, V>(DictBuilder<K, V> dict);

    public static ImmDict<K, V> New<K, V>(BuildDict<K, V> fill) {
        var dict = new DictBuilder<K, V>();
        fill(dict);
        return new ImmDict<K, V>(dict.data);
    }

    public static ImmDict<K, V> New<K, V>(int capacity, BuildDict<K, V> fill) {
        var dict = new DictBuilder<K, V>(capacity);
        fill(dict);
        return new ImmDict<K, V>(dict.data);
    }
}

[Serializable]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(ImmDict<,>.DebugView))]
public class ImmDict<K, V> : IReadOnlyDictionary<K, V>, IDictionary<K, V>, IEquatable<ImmDict<K, V>>, ICollection
{
    static readonly Dictionary<K, V> EmptyDict = new();
    public static readonly ImmDict<K, V> Empty = new();

    readonly Dictionary<K, V> data;
    [NonSerialized]
    int hash;
    [NonSerialized]
    bool hashComputed;

    internal ImmDict() {
        this.data = EmptyDict;
    }

    /// <summary>
    /// Unsafely constructs an ImmDict by taking ownership of the parameter Dictionary.
    /// </summary>
    /// <remarks>But that's why it's internal so we can guarantee this is only called sanely.</remarks>
    internal ImmDict(Dictionary<K, V> data) {
        if (data is null) {
            throw new ArgumentNullException(nameof(data));
        }
        this.data = data;
    }

    /// <summary>
    /// Constructs an ImmDict by copying the enumerable contents.
    /// </summary>
    public ImmDict(IEnumerable<KeyValuePair<K, V>> data) {
        if (data is null) {
            this.data = EmptyDict;
        } else {
            this.data = Extensions.DictOf(data);
        }
    }

    /// <inheritdoc />
    public int Count => this.data.Count;

    /// <inheritdoc />
    public IEnumerable<K> Keys => this.data.Keys;

    /// <inheritdoc />
    public IEnumerable<V> Values => this.data.Values;

    /// <inheritdoc />
    public V this[K key] => this.data[key];

    /// <inheritdoc />
    public bool Equals(ImmDict<K, V> other) {
        if (other is null) {
            return false;
        }
        if (this.data.Count != other.data.Count) {
            return false;
        }
        if (this.hashComputed && other.hashComputed && this.hash != other.hash) {
            return false;
        }

        foreach (var kvp in this.data) {
            if (!other.data.TryGetValue(kvp.Key, out var otherValue)
                || !EqualityComparer<V>.Default.Equals(kvp.Value, otherValue)) {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object obj) => Equals(obj as ImmDict<K, V>);

    /// <inheritdoc />
    public override int GetHashCode() {
        if (!this.hashComputed) {
            var hashCode = -1487460045;
            var hashes = new int[this.data.Count];
            var i = 0;
            foreach (var kvp in this) {
                var next = 524287;
                next = next * -1521134295 + EqualityComparer<K>.Default.GetHashCode(kvp.Key);
                next = next * -1521134295 + EqualityComparer<V>.Default.GetHashCode(kvp.Value);
                hashes[i] = next;
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
    public override string ToString() {
        return $"[{string.Join(", ", this.data.Select(kvp => kvp.Key + ": " + kvp.Value))}]";
    }

    /// <inheritdoc />
    public bool ContainsKey(K key) {
        return this.data.ContainsKey(key);
    }

    public bool TryGetValue(
        K key,
#if NET6_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)]
#endif
        out V value
    ) {
        return this.data.TryGetValue(key, out value);
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<K, V>> GetEnumerator() {
        return this.data.GetEnumerator();
    }

    public static bool operator ==(ImmDict<K, V> left, ImmDict<K, V> right) {
        return EqualityComparer<ImmDict<K, V>>.Default.Equals(left, right);
    }

    public static bool operator !=(ImmDict<K, V> left, ImmDict<K, V> right) {
        return !(left == right);
    }

    #region Explicit implementations
    IEnumerator IEnumerable.GetEnumerator() {
        return this.data.GetEnumerator();
    }

    ICollection<K> IDictionary<K, V>.Keys => this.data.Keys;

    ICollection<V> IDictionary<K, V>.Values => this.data.Values;

    bool ICollection<KeyValuePair<K, V>>.IsReadOnly => true;

    object ICollection.SyncRoot => this;

    bool ICollection.IsSynchronized => false;

    V IDictionary<K, V>.this[K key] {
        get => this.data[key];
        set => throw new NotSupportedException();
    }

    void IDictionary<K, V>.Add(K key, V value) {
        throw new NotSupportedException();
    }

    bool IDictionary<K, V>.Remove(K key) {
        throw new NotSupportedException();
    }

    void ICollection<KeyValuePair<K, V>>.Add(KeyValuePair<K, V> item) {
        throw new NotSupportedException();
    }

    void ICollection<KeyValuePair<K, V>>.Clear() {
        throw new NotSupportedException();
    }

    bool ICollection<KeyValuePair<K, V>>.Contains(KeyValuePair<K, V> item) {
        return this.data.Contains(item);
    }

    void ICollection<KeyValuePair<K, V>>.CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) {
        ((ICollection<KeyValuePair<K, V>>)this.data).CopyTo(array, arrayIndex);
    }

    void ICollection.CopyTo(Array array, int index) {
        throw new NotSupportedException();
    }

    bool ICollection<KeyValuePair<K, V>>.Remove(KeyValuePair<K, V> item) {
        throw new NotSupportedException();
    }
    #endregion

    internal class DebugView(ImmDict<K, V> dict) {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public Dictionary<K, V> Items => dict.data;
    }
}
