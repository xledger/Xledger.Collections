namespace Xledger.Collections.Test;

public class TestImmDict {
    [Fact]
    public void TestEmpty() {
        var imm = ImmDict<string, object>.Empty;
        Assert.Empty(imm);
        Assert.Equal(0, imm.Count);
        Assert.False(imm.GetEnumerator().MoveNext());
        Assert.Equal(imm.GetHashCode(), new ImmDict<string, object>().GetHashCode());
        Assert.Equal(imm, new ImmDict<string, object>());
        Assert.Equal(imm, imm);
        Assert.Equal(new ImmDict<string, object>(), imm);
        Assert.Equal("[]", imm.ToString());

        Assert.False(imm.Equals((object)null));
        Assert.False(imm.Equals((ImmDict<string, object>)null));
    }

    [Fact]
    public void TestSingle() {
        var imm1 = new ImmDict<int, object>(new Dictionary<int, object> {
            [1] = "foo",
        });
        Assert.NotEmpty(imm1);
        Assert.Equal(1, imm1.Count);
        Assert.Equal(imm1, imm1);
        Assert.True(imm1.GetEnumerator().MoveNext());
        Assert.Equal("[1: foo]", imm1.ToString());

        var imm2 = new ImmDict<int, object>(new Dictionary<int, object> {
            [1] = "foo",
        });
        Assert.Equal(imm1.GetHashCode(), imm2.GetHashCode());
        Assert.Equal(imm1, imm2);
        Assert.Equal(imm2, imm1);
    }

    [Fact]
    public void TestContains() {
        var items = Enumerable.Range(-100, 1_000).ToDictionary(i => i, i => i * i);
        var dct = items;
        var imm = items.ToImmDict();

        for (int i = -1000; i < 2000; ++i) {
            Assert.Equal(dct.ContainsKey(i), imm.ContainsKey(i));
            if (dct.TryGetValue(i, out var dctValue)) {
                imm.TryGetValue(i, out var immValue);
                Assert.Equal(dctValue, immValue);
            }
        }
    }

    [Fact]
    public void TestHashCode() {
        var items = Enumerable.Range(-100, 1_000).ToDictionary(i => i.ToString(), i => (i * i).ToString() + "A");
        var imm = items.ToImmDict();
        var ritems = Enumerable.Range(-100, 1_000).Reverse().ToDictionary(i => i.ToString(), i => (i * i).ToString() + "A");
        var two = ritems.ToImmDict();

        Assert.Equal(imm.GetHashCode(), two.GetHashCode());
        Assert.True(imm == two);
    }

    [Fact]
    public void TestNoOps() {
        var imm = ImmDict.Of((7, 6), (5, 4));
        IDictionary<int, int> idict = imm;
        Assert.ThrowsAny<NotSupportedException>(() => idict.Add(1, 2));
        Assert.ThrowsAny<NotSupportedException>(() => idict.Clear());
        Assert.ThrowsAny<NotSupportedException>(() => idict.Remove(4));
        Assert.ThrowsAny<NotSupportedException>(() => ((System.Collections.ICollection)idict).CopyTo((Array)null, 0));
    }

    [Fact]
    public void TestSelectKey() {
        var dct = Enumerable.Range(-100, 1_000).ToDictionary(i => i);
        var imm = Enumerable.Range(-100, 1_000).ToImmDict(i => i);

        for (int i = -1000; i < 2000; ++i) {
            Assert.Equal(dct.ContainsKey(i), imm.ContainsKey(i));
            if (dct.TryGetValue(i, out var dctValue)) {
                imm.TryGetValue(i, out var immValue);
                Assert.Equal(dctValue, immValue);
            }
        }
    }

    [Fact]
    public void TestSelectKeySelectValue() {
        var dct = Enumerable.Range(-100, 1_000).ToDictionary(i => i, i => (i * i).ToString());
        var imm = Enumerable.Range(-100, 1_000).ToImmDict(i => i, i => (i * i).ToString());

        for (int i = -1000; i < 2000; ++i) {
            Assert.Equal(dct.ContainsKey(i), imm.ContainsKey(i));
            if (dct.TryGetValue(i, out var dctValue)) {
                imm.TryGetValue(i, out var immValue);
                Assert.Equal(dctValue, immValue);
            }
        }
    }
}


