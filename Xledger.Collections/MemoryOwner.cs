using System.Buffers;

namespace Xledger.Collections;

public static class MemoryOwner<T> {
    public static IMemoryOwner<T> Empty = new EmptyMemoryOwner();

    sealed class EmptyMemoryOwner : IMemoryOwner<T> {
        public Memory<T> Memory { get; } = Memory<T>.Empty;

        public void Dispose() { }
    }
}

sealed class SizedMemoryOwner<T>(IMemoryOwner<T> memoryOwner, int capacity) : IMemoryOwner<T> {
    public Memory<T> Memory { get; } = memoryOwner.Memory.Slice(0, capacity);

    public void Dispose() {
        memoryOwner.Dispose();
    }
}
