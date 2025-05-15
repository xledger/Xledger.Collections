using System.Buffers;

namespace Xledger.Collections.Memory;

public class EmptyMemoryOwner<T> : IMemoryOwner<T> {
    public static IMemoryOwner<T> Instance = new EmptyMemoryOwner<T>();

    EmptyMemoryOwner() { }

    public Memory<T> Memory { get; } = Memory<T>.Empty;

    public void Dispose() { }
}
