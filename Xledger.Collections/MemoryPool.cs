using System.Buffers;

namespace Xledger.Collections;

public static class MemoryPool {
    /// <summary>
    /// Returns an IMemoryOwner`1 for sliced to be exactly capacity values. The
    /// underlying array may still be larger.
    /// </summary>
    public static IMemoryOwner<T> RentExactly<T>(int capacity) {
        var memoryOwner = MemoryPool<T>.Shared.Rent(capacity);
        return memoryOwner.Slice(capacity);
    }

    /// <summary>
    /// Returns an IMemoryOwner`1 with a Memory property of len. Takes ownership
    /// of memoryOwner and returns the underlying Memory object via that memoryOwner's
    /// Dispose.
    /// </summary>
    /// <remarks>This is in part why an instance of EmptyMemoryOwner isn't
    /// returned when len = 0. The memory in memoryOwner must be returned.</remarks>
    public static IMemoryOwner<T> Slice<T>(this IMemoryOwner<T> memoryOwner, int len) {
        return new SizedMemoryOwner<T>(memoryOwner, len);
    }
}
