using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Xledger.Collections.Bench;

[MemoryDiagnoser(displayGenColumns: false)]
[SimpleJob(RuntimeMoniker.Net48)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class ImmSetReflection {
    [Params(0, 300)]
    public int Capacity;

    [Params(300, 500)]
    public int Count;

    public static HashSet<string> NewHashSet(int capacity, int count) {
        var set = new HashSet<string>(capacity);
        for (int i = 0; i < count; ++i) {
            set.Add(Guid.NewGuid().ToString());
        }
        return set;
    }

    public static ImmSet.BuildSet<string> MakeFillSet(int count) {
        return (ImmSet.SetBuilder<string> set) => {
            for (int i = 0; i < count; ++i) {
                set.Add(Guid.NewGuid().ToString());
            }
        };
    }

    [Benchmark(Baseline = true)]
    public ImmSet<string> ImmSet_Build() {
        return ImmSet.Build<string>(MakeFillSet(this.Count), capacity: this.Capacity);
    }

    static readonly ConstructorInfo CI_NoCopy = typeof(ImmSet<string>).GetConstructor(
        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.ExactBinding,
        binder: null,
        [typeof(HashSet<string>)],
        modifiers: null);
    static readonly MethodInfo MI_Build = typeof(ImmSet).GetMethods()
        .Where(mi => mi.Name == nameof(ImmSet.Build))
        .Last()
        .MakeGenericMethod(typeof(string));

    static readonly MethodInfo MI_NewHashSet = typeof(ImmSetReflection).GetMethod(nameof(NewHashSet));
    static readonly MethodInfo MI_MakeFillSet = typeof(ImmSetReflection).GetMethod(nameof(MakeFillSet));

    static readonly Func<int, int, ImmSet<string>> newNoCopy = Compile_NewNoCopy();
    static readonly Func<int, int, ImmSet<string>> build = Compile_Build();

    [Benchmark]
    public ImmSet<string> DynInvoke_NewNoCopy() {
        return (ImmSet<string>)CI_NoCopy.Invoke([NewHashSet(this.Capacity, this.Count)]);
    }

    [Benchmark]
    public ImmSet<string> NewNoCopy() {
        return new ImmSet<string>(NewHashSet(this.Capacity, this.Count));
    }

    static Func<int, int, ImmSet<string>> Compile_NewNoCopy() {
        var pi_capacity = Expression.Parameter(typeof(int), "capacity");
        var pi_count = Expression.Parameter(typeof(int), "count");
        var newImmSet =
            Expression.New(CI_NoCopy,
                Expression.Call(MI_NewHashSet, pi_capacity, pi_count));
        var fn = Expression.Lambda<Func<int, int, ImmSet<string>>>(
            newImmSet,
            tailCall: false,
            pi_capacity,
            pi_count).Compile();
        return fn;
    }

    static Func<int, int, ImmSet<string>> Compile_Build() {
        var pi_capacity = Expression.Parameter(typeof(int), "capacity");
        var pi_count = Expression.Parameter(typeof(int), "count");
        var fillSetDelegate = Expression.Call(MI_MakeFillSet, pi_count);
        var newImmSet = Expression.Call(MI_Build, fillSetDelegate, pi_capacity, Expression.Constant(false));
        var fn = Expression.Lambda<Func<int, int, ImmSet<string>>>(
            newImmSet,
            tailCall: false,
            pi_capacity,
            pi_count).Compile();
        return fn;
    }

    [Benchmark]
    public ImmSet<string> Expr_NewNoCopy() {
        return newNoCopy(this.Capacity, this.Count);
    }

    [Benchmark]
    public ImmSet<string> Expr_Build() {
        return build(this.Capacity, this.Count);
    }
}
