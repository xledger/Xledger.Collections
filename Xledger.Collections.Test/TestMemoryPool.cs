using System.Buffers;

namespace Xledger.Collections.Test;

public class TestMemoryPool {
    [Fact]
    public void TestRentExactly() {
        using var atleast = MemoryPool<byte>.Shared.Rent(3);
        Assert.True(atleast.Memory.Length > 3);
        using var exactly = MemoryPool.RentExactly<byte>(3);
        Assert.True(exactly.Memory.Length == 3);
    }
}
