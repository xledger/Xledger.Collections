using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xledger.Collections.Memory;

namespace Xledger.Collections.Test;

public class TestMemoryOwner {
    [Fact]
    public void TestArray() {
        byte[] array = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        IMemoryOwner<byte> memoryOwner = array.ToOwnedMemory();
        Assert.Equal(array, memoryOwner.Memory);
        IMemoryOwner<byte> sliced = memoryOwner.Slice(3);
        Assert.Equal(array.AsMemory().Slice(3), sliced.Memory);
        IMemoryOwner<byte> sliced2 = sliced.Slice(1, 4);
        Assert.Equal(array.AsMemory().Slice(3).Slice(1, 4), sliced2.Memory);
    }
}
