namespace Xledger.Collections.Test;

public class TestSerialization {
    static readonly System.Text.UTF8Encoding UTF8 = new(false);

    public delegate byte[] Serialize<T>(T obj);
    public delegate T Deserialize<T>(byte[] s);

    static readonly IEnumerable<int[]> Ints = [
        [1, 31, 23, 3, 45, 66, 1038, 9, 1, 31, 66],
        null,
        [-2],
    ];

    // .Distinct() here because .ToDictionary() Add's keys which disallows duplicates.
    static readonly IEnumerable<(string, int)[]> StringsInts =
        Ints.Select(ints => ints?.Distinct().Select(i => (i.ToString(), i)).ToArray());

    static IEnumerable<(Serialize<T> s, Deserialize<T> d)> Serializers<T>() => [
        (NewtonsoftSerialize, NewtonsoftDeserialize<T>),
        // TODO: Not sure how to get System.Text.Json to deserialize these collection
        // TODO: types without adding a dependency onto it. For now, just verify that
        // TODO: the JSON emitted by System.Text.Json is readable by Newtonsoft.Json.
        (SystemTextSerialize, NewtonsoftDeserialize<T>),
        //(SystemTextSerialize, SystemTextDeserialize<T>),
        (BinaryFormatterSerialize, BinaryFormatterDeserialize<T>),
    ];

    public static readonly IEnumerable<object[]> JsonArrayParameters =
        from ints in Ints
        from serde in Serializers<ImmArray<int>>()
        select new object[] { serde.s, serde.d, ints, };

    public static readonly IEnumerable<object[]> JsonSetParameters =
        from ints in Ints
        from serde in Serializers<ImmSet<int>>()
        select new object[] { serde.s, serde.d, ints, };

    public static readonly IEnumerable<object[]> JsonDictParameters =
        from items in StringsInts
        from serde in Serializers<ImmDict<string, int>>()
        select new object[] { serde.s, serde.d, items, };

    static readonly System.Reflection.FieldInfo ImmArray_int_hash = typeof(ImmArray<int>)
        .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .FirstOrDefault(fi => fi.Name == "hash");

    static readonly System.Reflection.FieldInfo ImmSet_int_hash = typeof(ImmSet<int>)
        .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .FirstOrDefault(fi => fi.Name == "hash");

    static readonly System.Reflection.FieldInfo ImmDict_int_hash = typeof(ImmDict<string, int>)
        .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .FirstOrDefault(fi => fi.Name == "hash");

    [Theory]
    [MemberData(nameof(JsonArrayParameters))]
    public void TestImmArray(Serialize<ImmArray<int>> serialize, Deserialize<ImmArray<int>> deserialize, int[] ints) {
        Test(serialize, deserialize, ImmArray_int_hash, Extensions.ToImmArray, ints);
    }

    [Theory]
    [MemberData(nameof(JsonSetParameters))]
    public void TestImmSet(Serialize<ImmSet<int>> serialize, Deserialize<ImmSet<int>> deserialize, int[] ints) {
        Test(serialize, deserialize, ImmSet_int_hash, Extensions.ToImmSet, ints);
    }

    [Theory]
    [MemberData(nameof(JsonDictParameters))]
    public void TestImmDict(Serialize<ImmDict<string, int>> serialize, Deserialize<ImmDict<string, int>> deserialize, (string, int)[] items) {
        Test(serialize, deserialize, ImmDict_int_hash, Extensions.ToImmDict, items);
    }

    static void Test<T, V>(
        Serialize<T> serialize,
        Deserialize<T> deserialize,
        System.Reflection.FieldInfo hash,
        Func<IEnumerable<V>, T> make,
        V[] vals
    ) {
        Assert.NotNull(hash);

        var coll = make(vals);
        if (vals is not null) {
            Assert.Equal(0, hash.GetValue(coll));
        }
        Assert.NotEqual(0, coll.GetHashCode());
        //var expectedJson = Newtonsoft.Json.JsonConvert.SerializeObject(ints);
        var bytes = serialize(coll);
        //Assert.Equal(expectedJson, UTF8.GetString(bytes));
        var coll2 = deserialize(bytes);
        // This use of reflection to ensure that arr2.hash is 0 MUST be done before
        // any comparisons. This ensures that the hash code is not serialized.
        Assert.Equal(0, hash.GetValue(coll2));
        // And then compare the values.
        Assert.True(coll.Equals(coll2));
        Assert.Equal(coll, coll2);
    }

    static byte[] NewtonsoftSerialize<T>(T obj) {
        return UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(obj));
    }

    static T NewtonsoftDeserialize<T>(byte[] json) {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(UTF8.GetString(json));
    }

    static byte[] SystemTextSerialize<T>(T obj) {
        return UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(obj));
    }

    static T SystemTextDeserialize<T>(byte[] json) {
        return System.Text.Json.JsonSerializer.Deserialize<T>(UTF8.GetString(json));
    }

    static byte[] BinaryFormatterSerialize<T>(T obj) {
        var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        using var ms = new System.IO.MemoryStream();
        bf.Serialize(ms, obj);
        var bytes = ms.ToArray();
        return bytes;
    }

    static T BinaryFormatterDeserialize<T>(byte[] bytes) {
        var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        using var ms = new System.IO.MemoryStream(bytes);
        var obj = bf.Deserialize(ms);
        return (T)obj;
    }
}
