using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Xledger.Collections.Memory;

public static class Extensions {
    /// <summary>
    /// Returns an IMemoryOwner`1 with its Memory property properly sliced. Takes
    /// ownership of memoryOwner and returns the underlying Memory object via
    /// that memoryOwner's Dispose.
    /// </summary>
    public static IMemoryOwner<T> Slice<T>(this IMemoryOwner<T> memoryOwner, int start) {
        return new SizedMemoryOwner<T>(memoryOwner, start);
    }

    /// <summary>
    /// Returns an IMemoryOwner`1 with its Memory property properly sliced. Takes
    /// ownership of memoryOwner and returns the underlying Memory object via
    /// that memoryOwner's Dispose.
    /// </summary>
    public static IMemoryOwner<T> Slice<T>(this IMemoryOwner<T> memoryOwner, int start, int length) {
        return new SizedMemoryOwner<T>(memoryOwner, start, length);
    }

#if NET
    public static IMemoryOwner<byte> ToOwnedMemory(this Stream source, bool leaveOpen = false) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        (bool canHoldEntireStream, int initialBufLen) = GetBufferSize(source);

        var currentOwner = MemoryPool<byte>.Shared.Rent(initialBufLen);
        var currentBuffer = currentOwner.Memory;
        int totalBytesRead = 0;

        try {
            while (true) {
                var dest = currentBuffer.Slice(totalBytesRead);

                int bytesRead = source.Read(dest.Span);

                if (bytesRead == 0) {
                    break;
                }

                totalBytesRead += bytesRead;

                if (canHoldEntireStream && totalBytesRead == initialBufLen) {
                    // We've read the entire stream.
                    break;
                }

                if (totalBytesRead != currentBuffer.Length) {
                    continue;
                }

                if (currentBuffer.Length == Array.MaxLength) {
                    Span<byte> probe = stackalloc byte[1];
                    if (source.Read(probe) > 0) {
                        throw new IOException($"Stream exceeds the maximum bufferable array size of {Array.MaxLength} bytes.");
                    }
                    break; // we are at the end of the stream
                }

                var newCapacity = (long)currentBuffer.Length * 2;
                if (newCapacity > Array.MaxLength) {
                    newCapacity = Array.MaxLength;
                }

                var newOwner = MemoryPool<byte>.Shared.Rent((int)newCapacity);
                var newBuffer = newOwner.Memory;

                currentBuffer.CopyTo(newBuffer);
                currentOwner.Dispose();
                currentOwner = newOwner;
                currentBuffer = newBuffer;
            }
        } catch (Exception) {
            currentOwner.Dispose();
            throw;
        } finally {
            if (!leaveOpen) {
                source.Dispose();
            }
        }

        return currentOwner.Slice(0, totalBytesRead);
    }

    static readonly byte[] ASYNC_PROBE = new byte[1];

    public static async Task<IMemoryOwner<byte>> ToOwnedMemoryAsync(this Stream source, bool leaveOpen = false, CancellationToken tok = default) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        (bool canHoldEntireStream, int initialBufLen) = GetBufferSize(source);

        var currentOwner = MemoryPool<byte>.Shared.Rent(initialBufLen);
        var currentBuffer = currentOwner.Memory;
        int totalBytesRead = 0;

        try {
            while (true) {
                tok.ThrowIfCancellationRequested();

                var dest = currentBuffer.Slice(totalBytesRead);

                int bytesRead = await source.ReadAsync(dest, tok).ConfigureAwait(false);

                if (bytesRead == 0) {
                    break;
                }

                totalBytesRead += bytesRead;

                if (canHoldEntireStream && totalBytesRead == initialBufLen) {
                    // We've read the entire stream.
                    break;
                }

                if (totalBytesRead != currentBuffer.Length) {
                    continue;
                }

                if (currentBuffer.Length == Array.MaxLength) {
                    if (await source.ReadAsync(ASYNC_PROBE, tok).ConfigureAwait(false) > 0) {
                        throw new IOException($"Stream exceeds the maximum bufferable array size of {Array.MaxLength} bytes.");
                    }
                    break; // we are at the end of the stream
                }

                var newCapacity = (long)currentBuffer.Length * 2;
                if (newCapacity > Array.MaxLength) {
                    newCapacity = Array.MaxLength;
                }

                var newOwner = MemoryPool<byte>.Shared.Rent((int)newCapacity);
                var newBuffer = newOwner.Memory;

                currentBuffer.CopyTo(newBuffer);
                currentOwner.Dispose();
                currentOwner = newOwner;
                currentBuffer = newBuffer;
            }
        } catch (Exception) {
            currentOwner.Dispose();
            throw;
        } finally {
            if (!leaveOpen) {
                source.Dispose();
            }
        }

        return currentOwner.Slice(0, totalBytesRead);
    }

    // Initially copied from System.IO.Stream, adapted to be static and to match
    // the use above which is to copy an entire stream into a single array.
    static (bool isSufficient, int length) GetBufferSize(Stream stream) {
        // This value was originally picked to be the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
        // The CopyTo{Async} buffer is short-lived and is likely to be collected at Gen0, and it offers a significant improvement in Copy
        // performance.  Since then, the base implementations of CopyTo{Async} have been updated to use ArrayPool, which will end up rounding
        // this size up to the next power of two (131,072), which will by default be on the large object heap.  However, most of the time
        // the buffer should be pooled, the LOH threshold is now configurable and thus may be different than 85K, and there are measurable
        // benefits to using the larger buffer size.  So, for now, this value remains.
        const int DefaultCopyBufferSize = 81920;

        bool isSufficient = false;
        int bufferSize = DefaultCopyBufferSize;

        if (stream.CanSeek) {
            long length = stream.Length;
            long position = stream.Position;
            if (length <= position) // Handles negative overflows
            {
                // There are no bytes left in the stream to copy.
                // However, because CopyTo{Async} is virtual, we need to
                // ensure that any override is still invoked to provide its
                // own validation, so we use the smallest legal buffer size here.
                bufferSize = 1;
            } else {
                long remaining = length - position;
                if (remaining > Array.MaxLength) {
                    throw new IOException($"Stream exceeds the maximum bufferable array size of {Array.MaxLength} bytes.");
                } else if (remaining > 0) {
                    // If there is some remaining amount in the stream, we copy into a buffer of that size.
                    isSufficient = true;
                    bufferSize = (int)remaining;
                }
            }
        }

        return (isSufficient, bufferSize);
    }
#endif

    /// <summary>
    /// Adapt an array to IMemoryOwner. If you pass in an ArrayPool owner, the Array will be returned to the pool on dispose.
    /// If you pass null for the owner, nothing will happen when it is disposed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public static IMemoryOwner<T> ToOwnedMemory<T>(this T[] @this, ArrayPool<T> owner = null) {
        if (owner is null) {
            return new UnownedArrayMemory<T>(@this);
        } else {
            return new OwnedArrayMemory<T>(@this, owner);
        }
    }
}
