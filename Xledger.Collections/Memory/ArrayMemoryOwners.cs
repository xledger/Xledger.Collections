using System.Buffers;
using Xledger.Collections.Concurrent;

namespace Xledger.Collections.Memory;

class UnownedArrayMemory<T> : IMemoryOwner<T> {
    readonly SetOnceFlag isDisposed = new SetOnceFlag();
    Memory<T> memory;

    public Memory<T> Memory {
        get {
            if (this.isDisposed.IsFlagSet) {
                throw new ObjectDisposedException(nameof(UnownedArrayMemory<T>));
            }
            return this.memory;
        }
    }

    internal UnownedArrayMemory(T[] array) {
        if (array is null) {
            throw new ArgumentNullException(nameof(array));
        }
        this.memory = array;
    }

    public void Dispose() {
        if (this.isDisposed.TrySet()) {
            this.memory = default;
        }
    }
}

class OwnedArrayMemory<T>(T[] array, ArrayPool<T> owner) : IMemoryOwner<T> {
    readonly SetOnceFlag isDisposed = new SetOnceFlag();
    Memory<T> memory = array;

    public Memory<T> Memory {
        get {
            if (this.isDisposed.IsFlagSet) {
                throw new ObjectDisposedException(nameof(OwnedArrayMemory<T>));
            }
            return this.memory;
        }
    }
    
    void Dispose(bool disposing) {
        if (this.isDisposed.TrySet()) {
            owner.Return(array);
            array = null;
            this.memory = null;
        }
    }

    ~OwnedArrayMemory() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

