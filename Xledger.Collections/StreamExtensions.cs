using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Xledger.Collections;

public static class StreamExtensions {
    public static IMemoryOwner<byte> ToOwnedMemory(this Stream s) {
        throw new NotImplementedException();
    }

    public static async Task<IMemoryOwner<byte>> ToOwnedMemoryAsync(this Stream s, CancellationToken tok = default) {
        throw new NotImplementedException();
    }
}
