using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Xledger.Collections.Bench;

[MemoryDiagnoser(displayGenColumns: false)]
[SimpleJob(RuntimeMoniker.Net48)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class ImmArrayReflection {
    [Params(300)]
    public int Length;

    public static string[] NewArray(int length) {
        var arr = new string[length];
        for (int i = 0; i < arr.Length; ++i) {
            arr[i] = Guid.NewGuid().ToString();
        }
        return arr;
    }

    [Benchmark(Baseline = true)]
    public ImmArray<string> ImmArray_Build() {
        return ImmArray.Build<string>(FillSpan, length: this.Length);
    }

    static readonly ConstructorInfo CI_NoCopy = typeof(ImmArray<string>).GetConstructor([typeof(string[])]);
    static readonly MethodInfo MI_Build = typeof(ImmArray).GetMethods()
        .Where(mi =>
            mi.Name == nameof(ImmArray.Build)
#if NET10_0_OR_GREATER
            && mi.CustomAttributes.Where(
                a => a.AttributeType == typeof(System.Runtime.CompilerServices.OverloadResolutionPriorityAttribute)
            ).Any()
#endif
        )
        .Single()
        .MakeGenericMethod(typeof(string));

    static readonly MethodInfo MI_NewArray = typeof(ImmArrayReflection).GetMethod(nameof(NewArray));
    static readonly MethodInfo MI_FillSpan = typeof(ImmArrayReflection).GetMethod(nameof(FillSpan));

    static readonly Func<int, ImmArray<string>> newNoCopy = Compile_NewNoCopy();
    static readonly Func<int, ImmArray<string>> build = Compile_Build();

    public static void FillSpan(Span<string> arr) {
        for (int i = 0; i < arr.Length; ++i) {
            arr[i] = Guid.NewGuid().ToString();
        }
    }

    [Benchmark]
    public ImmArray<string> DynInvoke_NewNoCopy() {
        return (ImmArray<string>)CI_NoCopy.Invoke([NewArray(this.Length)]);
    }

    [Benchmark]
    public ImmArray<string> NewNoCopy() {
        return new ImmArray<string>(NewArray(this.Length));
    }

    static Func<int, ImmArray<string>> Compile_NewNoCopy() {
        var pi_length = Expression.Parameter(typeof(int), "length");
        var newImmArray =
            Expression.New(CI_NoCopy,
                Expression.Call(MI_NewArray, pi_length));
        var fn = Expression.Lambda<Func<int, ImmArray<string>>>(newImmArray, tailCall: false, pi_length).Compile();
        return fn;
    }

    static Func<int, ImmArray<string>> Compile_Build() {
        var pi_length = Expression.Parameter(typeof(int), "length");
#if NET10_0_OR_GREATER
        var fillSpanDelegate = MI_FillSpan.CreateDelegate<Action<Span<string>>>();
#else
        var fillSpanDelegate = MI_FillSpan.CreateDelegate(typeof(ImmArray.FillSpan<string>));
#endif
        var newImmArray = Expression.Call(MI_Build, Expression.Constant(fillSpanDelegate), pi_length);
        var fn = Expression.Lambda<Func<int, ImmArray<string>>>(newImmArray, tailCall: false, pi_length).Compile();
        return fn;
    }

    [Benchmark]
    public ImmArray<string> Expr_NewNoCopy() {
        return newNoCopy(this.Length);
    }

    [Benchmark]
    public ImmArray<string> Expr_Build() {
        return build(this.Length);
    }
}
