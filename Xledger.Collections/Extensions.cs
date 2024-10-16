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

    internal static HashSet<T> SetOf<T>(IEnumerable<T> xs) {
        return new HashSet<T>(xs);
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
