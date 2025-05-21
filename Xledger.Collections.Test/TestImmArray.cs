namespace Xledger.Collections.Test;

public class TestImmArray {
    [Fact]
    public void TestEmpty() {
        var imm = ImmArray<string>.Empty;
        Assert.Empty(imm);
        Assert.Equal(0, imm.Count);
        Assert.False(imm.GetEnumerator().MoveNext());
        Assert.Equal(imm.GetHashCode(), new ImmArray<string>().GetHashCode());
        Assert.Equal(imm, new ImmArray<string>());
        Assert.Equal(imm, imm);
        Assert.Equal(new ImmArray<string>(), imm);
        Assert.Equal("[]", imm.ToString());

        Assert.False(imm.Equals((object)null));
        Assert.False(imm.Equals((ImmArray<string>)null));
    }

    [Fact]
    public void TestSingle() {
        var imm1 = new ImmArray<int>([1]);
        Assert.NotEmpty(imm1);
        Assert.Equal(1, imm1.Count);
        Assert.Equal(imm1, imm1);
        Assert.True(imm1.GetEnumerator().MoveNext());
        Assert.Equal("[1]", imm1.ToString());

        var imm2 = new ImmArray<int>([1]);
        Assert.Equal(imm1.GetHashCode(), imm2.GetHashCode());
        Assert.Equal(imm1, imm2);
        Assert.Equal(imm2, imm1);
    }

#if NET8_0_OR_GREATER
    [Fact]
    public void TestCollectionBuilder() {
        var ys = ImmArray.Of("hi", "how", "are", "you?");
        ImmArray<string> xs = ["hi", "how", "are", "you?"];
        Assert.Equal(ys, xs);
    }
#endif

    [Fact]
    public void TestCopy() {
        int[] arr = [1, 2, 3, 4, 5, 6];
        var lst = new List<int>(arr);
        var imm = arr.ToImmArray();

        int[] target1 = new int[100];
        lst.CopyTo(target1);
        lst.CopyTo(target1, 12);
        lst.CopyTo(2, target1, 30, 3);

        int[] target2 = new int[100];
        imm.CopyTo(target2);
        imm.CopyTo(target2, 12);
        imm.CopyTo(2, target2, 30, 3);

        Assert.Equal(target1, target2);

        target1 = lst.ToArray();
        target1[0] = 99;
        Assert.NotEqual(lst, target1);

        target2 = imm.ToArray();
        target2[0] = 99;
        Assert.NotEqual(lst, target2);
    }

    [Fact]
    public void TestContains() {
        int[] arr = [1, 2, 3, 1, 2, 2];
        var lst = new List<int>(arr);
        var imm = arr.ToImmArray();

        Assert.Equal(0, lst.IndexOf(1));
        Assert.Equal(1, lst.IndexOf(2));
        Assert.Equal(2, lst.IndexOf(3));
        Assert.False(lst.Contains(9));
        Assert.True(lst.Contains(3));

        Assert.Equal(0, imm.IndexOf(1));
        Assert.Equal(1, imm.IndexOf(2));
        Assert.Equal(2, imm.IndexOf(3));
        Assert.False(imm.Contains(9));
        Assert.True(imm.Contains(3));

        Assert.Equal(lst.IndexOf(-1), imm.IndexOf(-1));
        Assert.Equal(lst.IndexOf(0), imm.IndexOf(0));
        Assert.Equal(lst.IndexOf(1), imm.IndexOf(1));
        Assert.Equal(lst.IndexOf(2), imm.IndexOf(2));
        Assert.Equal(lst.IndexOf(3), imm.IndexOf(3));
        Assert.Equal(lst.IndexOf(9), imm.IndexOf(9));
    }

    [Fact]
    public void TestNoOps() {
        var imm = ImmArray.Of(7, 6, 5, 4);
        IList<int> ilist = imm;
        Assert.ThrowsAny<NotSupportedException>(() => ilist[0] = 1);
        Assert.ThrowsAny<NotSupportedException>(() => ilist.Add(1));
        Assert.ThrowsAny<NotSupportedException>(() => ilist.Clear());
        Assert.ThrowsAny<NotSupportedException>(() => ilist.Insert(1, 2));
        Assert.ThrowsAny<NotSupportedException>(() => ilist.Remove(4));
        Assert.ThrowsAny<NotSupportedException>(() => ilist.RemoveAt(2));
        Assert.ThrowsAny<NotSupportedException>(() => ((System.Collections.ICollection)ilist).CopyTo((Array)null, 0));
    }

    [Fact]
    public void TestCompareTo() {
        Assert.True(null <= (ImmArray<string>)null);

        var imm1 = ImmArray.Of("foo", "bar", "baz");
        Assert.True(null < imm1);
        Assert.True(null <= imm1);
        Assert.True(imm1 > null);
        Assert.True(imm1 >= null);

        var imm2 = ImmArray.Of("foo", "bar", "zip");
        Assert.True(imm1 < imm2);
        Assert.True(imm1 <= imm2);
        Assert.True(imm2 > imm1);
        Assert.True(imm2 >= imm1);
    }


    [Fact]
    public void TestCompareToRecord() {
        var imm1 = ImmArray.Of(
            new Employee(1, "Bas Rutten", 10_000),
            new Employee(2, "Conor McGregor", 200_000),
            new Employee(3, "Frank Shamrock", 6_000));
        Assert.True(null < imm1);
        Assert.True(null <= imm1);
        Assert.True(imm1 > null);
        Assert.True(imm1 >= null);

        var imm2 = ImmArray.Of(
            new Employee(1, "Bas Rutten", 10_000),
            new Employee(2, "Conor McGregor", 200_000),
            new Employee(3, "Frank Shamrock", 6_000));
        Assert.ThrowsAny<ArgumentException>(() => imm1 < imm2);
        Assert.ThrowsAny<ArgumentException>(() => imm1 <= imm2);
        Assert.ThrowsAny<ArgumentException>(() => imm2 > imm1);
        Assert.ThrowsAny<ArgumentException>(() => imm2 >= imm1);
    }

    [Fact]
    public void TestFind() {
        var arr = Enumerable.Range(-1, 100).ToArray();
        var imm = arr.ToImmArray();

        Predicate<int> pred = i => i == -1;
        Assert.Equal(Array.Find(arr, pred), imm.Find(pred));
        pred = i => i == 12;
        Assert.Equal(Array.Find(arr, pred), imm.Find(pred));
        pred = i => i == 75;
        Assert.Equal(Array.Find(arr, pred), imm.Find(pred));
        pred = i => i == 1_000_000;
        Assert.Equal(Array.Find(arr, pred), imm.Find(pred));
    }

    [Fact]
    public void TestSpan() {
        var imm = Enumerable.Range(-1, 100).ToArray().ToImmArray();
        var span = imm.Span;
        Assert.Equal(imm.Length, span.Length);
        for (int i = 0; i < imm.Length; i++) {
            Assert.Equal(imm[i], span[i]);
        }
    }

    public record Employee(int Id, string Name, decimal Salary);
}

