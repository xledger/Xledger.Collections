namespace Xledger.Collections;

public static class Extensions {
    public static ImmArray<T> ToImmArray<T>(this IEnumerable<T> xs) {
        return xs switch {
            null => ImmArray<T>.Empty,
            ImmArray<T> immArr => immArr,
            T[] arr => new ImmArray<T>(ArrayOf(arr)),
            ICollection<T> coll => new ImmArray<T>(ArrayOf(coll)),
            IReadOnlyCollection<T> roColl => new ImmArray<T>(ArrayOf(roColl.Count, roColl)),
            _ => new ImmArray<T>(ArrayOf(xs)),
        };
    }

    public static ImmArray<U> ToImmArray<T, U>(this IEnumerable<T> xs, Func<T, U> select) {
        return xs switch {
            null => ImmArray<U>.Empty,
            T[] arr => new ImmArray<U>(ArrayOf(arr.Length, arr, select)),
            ICollection<T> coll => new ImmArray<U>(ArrayOf(coll.Count, coll, select)),
            IReadOnlyCollection<T> roColl => new ImmArray<U>(ArrayOf(roColl.Count, roColl, select)),
            _ => new ImmArray<U>(ArrayOf(xs, select)),
        };
    }

#if NET
    public static ImmArray<T> ToImmArray<T>(this ReadOnlySpan<T> xs) {
        return new ImmArray<T>(xs.ToArray());
    }
#endif

    public static ImmSet<T> ToImmSet<T>(this IEnumerable<T> xs) {
        return xs switch {
            null => ImmSet<T>.Empty,
            ImmSet<T> immSet => immSet,
            _ => new ImmSet<T>(SetOf(xs)),
        };
    }

    public static ImmSet<U> ToImmSet<T, U>(this IEnumerable<T> xs, Func<T, U> select) {
        return xs switch {
            null => ImmSet<U>.Empty,
            _ => new ImmSet<U>(SetOf(xs, select)),
        };
    }

#if NET
    public static ImmSet<T> ToImmSet<T>(this ReadOnlySpan<T> xs) {
        return new ImmSet<T>(xs.ToArray());
    }
#endif

    public static ImmDict<K, V> ToImmDict<K, V>(this IEnumerable<KeyValuePair<K, V>> xs) {
        return xs switch {
            null => ImmDict<K, V>.Empty,
            ImmDict<K, V> immSet => immSet,
            _ => new ImmDict<K, V>(DictOf(xs)),
        };
    }

    public static ImmDict<K, V> ToImmDict<K, V>(this IEnumerable<(K, V)> xs) {
        return xs switch {
            null => ImmDict<K, V>.Empty,
            _ => new ImmDict<K, V>(DictOf(xs)),
        };
    }

    public static ImmDict<K, T> ToImmDict<T, K>(this IEnumerable<T> xs, Func<T, K> selectKey) {
        return xs switch {
            null => ImmDict<K, T>.Empty,
            _ => new ImmDict<K, T>(xs.ToDictionary(selectKey)),
        };
    }

    public static ImmDict<K, V> ToImmDict<T, K, V>(
        this IEnumerable<T> xs,
        Func<T, K> selectKey,
        Func<T, V> selectVal
    ) {
        return xs switch {
            null => ImmDict<K, V>.Empty,
            _ => new ImmDict<K, V>(xs.ToDictionary(selectKey, selectVal)),
        };
    }

    internal static T[] ArrayOf<T>(T[] arr) {
        return (T[])arr.Clone();
    }

    internal static T[] ArrayOf<T>(ICollection<T> coll) {
        var arr = new T[coll.Count];
        coll.CopyTo(arr, 0);
        return arr;
    }

    internal static T[] ArrayOf<T>(int n, IEnumerable<T> xs) {
        var arr = new T[n];
        var i = 0;
        foreach (var x in xs) {
            arr[i] = x;
            ++i;
        }
        return arr;
    }

    internal static T[] ArrayOf<T>(IEnumerable<T> xs) {
        var lst = new List<T>();
        foreach (var x in xs) {
            lst.Add(x);
        }
        return lst.ToArray();
    }

    internal static U[] ArrayOf<T, U>(int n, IEnumerable<T> xs, Func<T, U> f) {
        var arr = new U[n];
        var i = 0;
        foreach (var x in xs) {
            arr[i] = f(x);
            ++i;
        }
        return arr;
    }

    internal static U[] ArrayOf<T, U>(IEnumerable<T> xs, Func<T, U> f) {
        var lst = new List<U>();
        foreach (var x in xs) {
            lst.Add(f(x));
        }
        return lst.ToArray();
    }

    internal static HashSet<T> SetOf<T>(IEnumerable<T> xs) {
        return new HashSet<T>(xs);
    }

    internal static HashSet<U> SetOf<T, U>(IEnumerable<T> xs, Func<T, U> f) {
        return new HashSet<U>(xs.Select(f));
    }

    internal static Dictionary<K, V> DictOf<K, V>(IEnumerable<KeyValuePair<K, V>> xs) {
#if NET6_0_OR_GREATER
        return new Dictionary<K, V>(xs, null);
#elif NETFRAMEWORK
        return xs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
#endif
    }

    internal static Dictionary<K, V> DictOf<K, V>(IEnumerable<(K, V)> xs) {
        return xs.ToDictionary(kvp => kvp.Item1, kvp => kvp.Item2);
    }
}
