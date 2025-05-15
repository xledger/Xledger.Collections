using System.Buffers;
using System.IO;

namespace Xledger.Collections;

public static class StreamExtensions {
    public static IMemoryOwner<byte> ToOwnedMemory(this Stream s) {
        throw new NotImplementedException();
    }
}
