using System.Threading;

namespace Xledger.Collections.Concurrent;

public class SetOnceFlag {
    int isFlagSet;

    public SetOnceFlag(bool isFlagSet = false) {
        this.isFlagSet = isFlagSet ? 1 : 0;
    }

    public bool IsFlagSet => this.isFlagSet == 1;

    public bool TrySet() {
        return 0 == Interlocked.CompareExchange(ref this.isFlagSet, 1, 0);
    }
}
