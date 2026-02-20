namespace Xledger.Collections.Bench;

[MemoryDiagnoser(displayGenColumns: false)]
[SimpleJob(RuntimeMoniker.Net48)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class ImmArrayConstruction {
    [Params(300)]
    public int Length;

    [Benchmark(Baseline = true)]
    public string[] NewArray() {
        var arr = new string[this.Length];
        for (int i = 0; i < arr.Length; ++i) {
            arr[i] = Guid.NewGuid().ToString();
        }
        return arr;
    }

    [Benchmark]
    public ImmArray<string> ToImmArray() {
        var arr = new string[this.Length];
        for (int i = 0; i < arr.Length; ++i) {
            arr[i] = Guid.NewGuid().ToString();
        }
        return arr.ToImmArray();
    }

    [Benchmark]
    public ImmArray<string> ImmArray_Build() {
        return ImmArray.Build<string>(arr => {
            for (int i = 0; i < arr.Length; ++i) {
                arr[i] = Guid.NewGuid().ToString();
            }
        }, length: this.Length);
    }
}
