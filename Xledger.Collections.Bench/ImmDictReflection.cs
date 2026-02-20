using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Xledger.Collections.Bench;

[MemoryDiagnoser(displayGenColumns: false)]
[SimpleJob(RuntimeMoniker.Net48)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class ImmDictReflection {
    [Params(0, 300)]
    public int Capacity;

    [Params(300, 500)]
    public int Count;

    public static Dictionary<string, int> NewDictionary(int capacity, int count) {
        var dict = new Dictionary<string, int>(capacity);
        for (int i = 0; i < count; ++i) {
            dict.Add(Guid.NewGuid().ToString(), i);
        }
        return dict;
    }

    public static ImmDict.BuildDict<string, int> MakeFillDict(int count) {
        return (ImmDict.DictBuilder<string, int> dict) => {
            for (int i = 0; i < count; ++i) {
                dict.Add(Guid.NewGuid().ToString(), i);
            }
        };
    }

    [Benchmark(Baseline = true)]
    public ImmDict<string, int> ImmDict_Build() {
        return ImmDict.Build<string, int>(MakeFillDict(this.Count), capacity: this.Capacity);
    }

    static readonly ConstructorInfo CI_NoCopy = typeof(ImmDict<string, int>).GetConstructor(
        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.ExactBinding,
        binder: null,
        [typeof(Dictionary<string, int>)],
        modifiers: null);
    static readonly MethodInfo MI_Build = typeof(ImmDict).GetMethods()
        .Where(mi => mi.Name == nameof(ImmDict.Build))
        .Last()
        .MakeGenericMethod(typeof(string), typeof(int));

    static readonly MethodInfo MI_NewDictionary = typeof(ImmDictReflection).GetMethod(nameof(NewDictionary));
    static readonly MethodInfo MI_MakeFillDict = typeof(ImmDictReflection).GetMethod(nameof(MakeFillDict));

    static readonly Func<int, int, ImmDict<string, int>> newNoCopy = Compile_NewNoCopy();
    static readonly Func<int, int, ImmDict<string, int>> build = Compile_Build();

    [Benchmark]
    public ImmDict<string, int> DynInvoke_NewNoCopy() {
        return (ImmDict<string, int>)CI_NoCopy.Invoke([NewDictionary(this.Capacity, this.Count)]);
    }

    [Benchmark]
    public ImmDict<string, int> NewNoCopy() {
        return new ImmDict<string, int>(NewDictionary(this.Capacity, this.Count));
    }

    static Func<int, int, ImmDict<string, int>> Compile_NewNoCopy() {
        var pi_capacity = Expression.Parameter(typeof(int), "capacity");
        var pi_count = Expression.Parameter(typeof(int), "count");
        var newImmSet =
            Expression.New(CI_NoCopy,
                Expression.Call(MI_NewDictionary, pi_capacity, pi_count));
        var fn = Expression.Lambda<Func<int, int, ImmDict<string, int>>>(
            newImmSet,
            tailCall: false,
            pi_capacity,
            pi_count).Compile();
        return fn;
    }

    static Func<int, int, ImmDict<string, int>> Compile_Build() {
        var pi_capacity = Expression.Parameter(typeof(int), "capacity");
        var pi_count = Expression.Parameter(typeof(int), "count");
        var fillSetDelegate = Expression.Call(MI_MakeFillDict, pi_count);
        var newImmSet = Expression.Call(MI_Build, fillSetDelegate, pi_capacity, Expression.Constant(false));
        var fn = Expression.Lambda<Func<int, int, ImmDict<string, int>>>(
            newImmSet,
            tailCall: false,
            pi_capacity,
            pi_count).Compile();
        return fn;
    }

    [Benchmark]
    public ImmDict<string, int> Expr_NewNoCopy() {
        return newNoCopy(this.Capacity, this.Count);
    }

    [Benchmark]
    public ImmDict<string, int> Expr_Build() {
        return build(this.Capacity, this.Count);
    }
}
