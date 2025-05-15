using System.Buffers;

namespace Xledger.Collections.Memory;

sealed class SizedMemoryOwner<T>(IMemoryOwner<T> memoryOwner, int capacity) : IMemoryOwner<T> {
    public Memory<T> Memory { get; } = memoryOwner.Memory.Slice(0, capacity);

    public void Dispose() {
        memoryOwner.Dispose();
    }
}
