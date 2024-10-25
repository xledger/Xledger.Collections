namespace Xledger.Collections.Test;

public class TestImmSet{
    [Fact]
    public void TestEmpty() {
        var imm = ImmSet<string>.Empty;
        Assert.Empty(imm);
        Assert.Equal(0, imm.Count);
        Assert.False(imm.GetEnumerator().MoveNext());
        Assert.Equal(imm.GetHashCode(), new ImmSet<string>().GetHashCode());
        Assert.Equal(imm, new ImmSet<string>());
        Assert.Equal(imm, imm);
        Assert.Equal(new ImmSet<string>(), imm);
        Assert.Equal("[]", imm.ToString());

        Assert.False(imm.Equals((object)null));
        Assert.False(imm.Equals((ImmSet<string>)null));
    }

    [Fact]
    public void TestSingle() {
        var imm1 = new ImmSet<int>([1]);
        Assert.NotEmpty(imm1);
        Assert.Equal(1, imm1.Count);
        Assert.Equal(imm1, imm1);
        Assert.True(imm1.GetEnumerator().MoveNext());
        Assert.Equal("[1]", imm1.ToString());

        var imm2 = new ImmSet<int>([1]);
        Assert.Equal(imm1.GetHashCode(), imm2.GetHashCode());
        Assert.Equal(imm1, imm2);
        Assert.Equal(imm2, imm1);
    }

#if NET8_0_OR_GREATER
    [Fact]
    public void TestCollectionBuilder() {
        var ys = ImmSet.Of("hi", "how", "are", "you?");
        ImmSet<string> xs = ["hi", "how", "are", "you?"];
        Assert.Equal(ys, xs);
    }
#endif

    [Fact]
    public void TestCopy() {
        int[] arr = [1, 2, 3, 4, 5, 6];
        var hsh = new HashSet<int>(arr);
        var imm = arr.ToImmSet();

        int[] target1 = new int[100];
        hsh.CopyTo(target1);
        hsh.CopyTo(target1, 12);

        int[] target2 = new int[100];
        imm.CopyTo(target2);
        imm.CopyTo(target2, 12);

        Assert.Equal(target1, target2);

        target1 = hsh.ToArray();
        target1[0] = 99;
        Assert.NotEqual(hsh, target1);

        target2 = imm.ToArray();
        target2[0] = 99;
        Assert.NotEqual(hsh, target2);
    }

    [Fact]
    public void TestContains() {
        int[] arr = [1, 2, 3, 1, 2, 2];
        var lst = new List<int>(arr);
        var imm = arr.ToImmSet();

        Assert.Equal(0, lst.IndexOf(1));
        Assert.Equal(1, lst.IndexOf(2));
        Assert.Equal(2, lst.IndexOf(3));
        Assert.False(lst.Contains(9));
        Assert.True(lst.Contains(3));
    }

    [Fact]
    public void TestTryGetValue() {
        var imm = ImmSet.Of("foo", "bar", "baz");

        var lookup = "food".Substring(0, 3);
        var found = imm.TryGetValue(lookup, out var actual);
        Assert.True(found);
        Assert.NotSame(lookup, actual);
        Assert.Equal(lookup, actual);

        found = imm.TryGetValue("foo", out actual);
        Assert.True(found);
        Assert.Same("foo", actual);
        Assert.Equal("foo", actual);

        found = imm.TryGetValue("food", out actual);
        Assert.False(found);
    }

    [Fact]
    public void TestNoOps() {
        var imm = ImmSet.Of(7, 6, 5, 4);
        ISet<int> iset = imm;
        Assert.ThrowsAny<NotSupportedException>(() => iset.Add(1));
        Assert.ThrowsAny<NotSupportedException>(() => iset.Clear());
        Assert.ThrowsAny<NotSupportedException>(() => iset.Remove(4));
        Assert.ThrowsAny<NotSupportedException>(() => ((System.Collections.ICollection)iset).CopyTo((Array)null, 0));
    }
}


