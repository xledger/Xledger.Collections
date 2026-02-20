namespace Xledger.Collections.Bench;

[MemoryDiagnoser(displayGenColumns: false)]
[SimpleJob(RuntimeMoniker.Net48)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class ImmDictConstruction {
    [Params(0, 300)]
    public int Capacity;

    [Params(300, 500)]
    public int Count;

    [Benchmark(Baseline = true)]
    public Dictionary<string, int> NewDictionary() {
        var dict = new Dictionary<string, int>(this.Capacity);
        for (int i = 0; i < this.Count; ++i) {
            dict.Add(Guid.NewGuid().ToString(), i);
        }
        return dict;
    }

    [Benchmark]
    public ImmDict<string, int> ToImmDict() {
        var dict = new Dictionary<string, int>(this.Capacity);
        for (int i = 0; i < this.Count; ++i) {
            dict.Add(Guid.NewGuid().ToString(), i);
        }
        return dict.ToImmDict();
    }

    [Benchmark]
    public ImmDict<string, int> ImmDict_Build() {
        return ImmDict.Build<string, int>(dict => {
            for (int i = 0; i < this.Count; ++i) {
                dict.Add(Guid.NewGuid().ToString(), i);
            }
        }, capacity: this.Capacity);
    }
}
