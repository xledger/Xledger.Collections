using System.Buffers;
using Xledger.Collections.Concurrent;

namespace Xledger.Collections.Memory;

sealed class SizedMemoryOwner<T> : IMemoryOwner<T> {
    readonly SetOnceFlag isDisposed = new SetOnceFlag();
    IMemoryOwner<T> memoryOwner;

    internal SizedMemoryOwner(IMemoryOwner<T> memoryOwner, int start) {
        Memory = memoryOwner.Memory.Slice(start);
    }

    internal SizedMemoryOwner(IMemoryOwner<T> memoryOwner, int start, int length) {
        Memory = memoryOwner.Memory.Slice(start, length);
    }

    public Memory<T> Memory { get; }

    void Dispose(bool disposing) {
        if (this.isDisposed.TrySet()) {
            this.memoryOwner?.Dispose();
            this.memoryOwner = null;
        }
    }

    ~SizedMemoryOwner() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
