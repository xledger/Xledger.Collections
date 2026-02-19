namespace Xledger.Collections.Bench;

[MemoryDiagnoser(displayGenColumns: false)]
[SimpleJob(RuntimeMoniker.Net48)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class ImmSetConstruction {
    [Params(0, 300)]
    public int Capacity;

    [Params(300, 500)]
    public int Count;

    [Benchmark(Baseline = true)]
    public HashSet<string> NewHashSet() {
        var set = new HashSet<string>(this.Capacity);
        for (int i = 0; i < this.Count; ++i) {
            set.Add(Guid.NewGuid().ToString());
        }
        return set;
    }

    [Benchmark]
    public ImmSet<string> ToImmSet() {
        var set = new HashSet<string>(this.Capacity);
        for (int i = 0; i < this.Count; ++i) {
            set.Add(Guid.NewGuid().ToString());
        }
        return set.ToImmSet();
    }

    [Benchmark]
    public ImmSet<string> ImmSet_Build() {
        return ImmSet.Build<string>(set => {
            for (int i = 0; i < this.Count; ++i) {
                set.Add(Guid.NewGuid().ToString());
            }
        }, capacity: this.Capacity);
    }
}
