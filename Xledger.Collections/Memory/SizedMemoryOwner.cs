using System.Buffers;

namespace Xledger.Collections.Memory;

sealed class SizedMemoryOwner<T> : IMemoryOwner<T> {
    IMemoryOwner<T> memoryOwner;

    internal SizedMemoryOwner(IMemoryOwner<T> memoryOwner, int start) {
        Memory = memoryOwner.Memory.Slice(start);
    }

    internal SizedMemoryOwner(IMemoryOwner<T> memoryOwner, int start, int length) {
        Memory = memoryOwner.Memory.Slice(start, length);
    }

    public Memory<T> Memory { get; }

    public void Dispose() {
        this.memoryOwner.Dispose();
    }
}
