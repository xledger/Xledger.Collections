using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Xledger.Collections.Memory;

public static class Extensions {
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

    public static IMemoryOwner<byte> ToOwnedMemory(this Stream s) {
        throw new NotImplementedException();
    }

    public static async Task<IMemoryOwner<byte>> ToOwnedMemoryAsync(this Stream s, CancellationToken tok = default) {
        throw new NotImplementedException();
    }
}
