using System.Buffers;

namespace Xledger.Collections.Memory;

class UnownedMemoryOwner<T>(T[] array) : IMemoryOwner<T> {
    public Memory<T> Memory { get; } = array;
    public void Dispose() { }
}
