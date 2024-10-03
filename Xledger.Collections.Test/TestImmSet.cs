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
    public void TestMutability() {
        var bck = new HashSet<int>([1, 2, 3, 4, 5, 6]);
        var hsh = new List<int>(bck);
        Assert.Equal(bck, hsh);

        ImmSet<int> foo = ImmSet.Of(1, 2, 3);

        // Are ImmSets sane?
        var imm1 = bck.ToImmSet();
        var imm2 = new ImmSet<int>(bck);
        var imm3 = hsh.ToImmSet();
        Assert.Equal(imm1, imm2);
        Assert.Equal(imm1, imm3);
        Assert.Equal(imm2, imm3);
        Assert.Equal(bck, imm1);
        Assert.Equal(bck, imm2);
        Assert.Equal(bck, imm3);
        Assert.Equal(hsh, imm1);
        Assert.Equal(hsh, imm2);
        Assert.Equal(hsh, imm3);
        Assert.True(imm1.Equals(imm2));
        Assert.True(imm1.Equals(imm3));
        Assert.True(imm2.Equals(imm1));
        Assert.True(imm2.Equals(imm3));
        Assert.True(imm3.Equals(imm1));
        Assert.True(imm3.Equals(imm2));

        Assert.True(imm1.Equals((object)imm2));
        Assert.True(imm1.Equals((object)imm3));
        Assert.True(imm2.Equals((object)imm1));
        Assert.True(imm2.Equals((object)imm3));
        Assert.True(imm3.Equals((object)imm1));
        Assert.True(imm3.Equals((object)imm2));

        Assert.True(imm1 == imm2);
        Assert.True(imm2 == imm3);

        // Yes.
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


