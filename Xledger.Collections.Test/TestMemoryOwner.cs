using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using Xledger.Collections.Memory;

namespace Xledger.Collections.Test;

public class TestMemoryOwner {
    [Fact]
    public void TestArray() {
        byte[] array = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        IMemoryOwner<byte> memoryOwner = array.ToOwnedMemory();
        Assert.Equal(array, memoryOwner.Memory);
    }

    [Fact]
    public void TestSlice() {
        byte[] array = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        IMemoryOwner<byte> memoryOwner = array.ToOwnedMemory();
        IMemoryOwner<byte> sliced = memoryOwner.Slice(3);
        Assert.Equal(array.AsMemory().Slice(3), sliced.Memory);
        IMemoryOwner<byte> sliced2 = sliced.Slice(1, 4);
        Assert.Equal(array.AsMemory().Slice(3).Slice(1, 4), sliced2.Memory);
    }

    [Fact]
    public void TestStream_ToMemoryOwner() {
        // This length should be larger than the default GetCopyBufferSize.
        byte[] array = new byte[4 * 1024 * 1024 + 3];
        new Random().NextBytes(array);
        var ms = new MemoryStream(array);
        using var memoryOwner = ms.ToOwnedMemory();
#if NET
        Assert.Equal(array.AsMemory(), memoryOwner.Memory);
#else
        var mem = memoryOwner.Memory;
        Assert.Equal(array.Length, mem.Length);
        for (int i = 0; i < array.Length; ++i) {
            Assert.Equal(array[i], mem.Span[i]);
        }
#endif
    }

    [Fact]
    public async Task TestStream_ToMemoryOwnerAsync() {
        // This length should be larger than the default GetCopyBufferSize.
        byte[] array = new byte[4 * 1024 * 1024 + 3];
        new Random().NextBytes(array);
        var ms = new MemoryStream(array);
        using var memoryOwner = await ms.ToOwnedMemoryAsync();
#if NET
        Assert.Equal(array.AsMemory(), memoryOwner.Memory);
#else
        var mem = memoryOwner.Memory;
        Assert.Equal(array.Length, mem.Length);
        for (int i = 0; i < array.Length; ++i) {
            Assert.Equal(array[i], mem.Span[i]);
        }
#endif
    }

    // Tests that reading from a stream with an unknown size does so correctly.
    [Fact]
    public void TestUnsizedStream_ToMemoryOwner() {
        // This length should be larger than the default GetCopyBufferSize.
        byte[] array = new byte[2 * 1024 * 1024 + 17];
        new Random().NextBytes(array);
        var ms = new UnsizedMemoryStream(array);
        using var memoryOwner = ms.ToOwnedMemory();
#if NET
        Assert.Equal(array.AsMemory(), memoryOwner.Memory);
#else
        var mem = memoryOwner.Memory;
        Assert.Equal(array.Length, mem.Length);
        for (int i = 0; i < array.Length; ++i) {
            Assert.Equal(array[i], mem.Span[i]);
        }
#endif
    }

    // Tests that reading from a stream with an unknown size does so correctly.
    [Fact]
    public async Task TestUnsizedStream_ToMemoryOwnerAsync() {
        // This length should be larger than the default GetCopyBufferSize.
        byte[] array = new byte[2 * 1024 * 1024 + 17];
        new Random().NextBytes(array);
        var ms = new UnsizedMemoryStream(array);
        using var memoryOwner = await ms.ToOwnedMemoryAsync();
#if NET
        Assert.Equal(array.AsMemory(), memoryOwner.Memory);
#else
        var mem = memoryOwner.Memory;
        Assert.Equal(array.Length, mem.Length);
        for (int i = 0; i < array.Length; ++i) {
            Assert.Equal(array[i], mem.Span[i]);
        }
#endif
    }

    class UnsizedMemoryStream(byte[] buffer) : MemoryStream(buffer) {
        public override bool CanSeek => false;

        public override int Capacity {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override long Length => throw new NotImplementedException();

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }
    }
}
