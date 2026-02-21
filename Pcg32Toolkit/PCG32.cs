using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Pcg32Toolkit;

/// <summary>
/// PCG XSH RR 64/32 pseudo-random number generator.
/// </summary>
public sealed class PCG32
{
    private const ulong Multiplier = 6364136223846793005UL;

    private ulong _state;
    private readonly ulong _inc;

    /// <summary>
    /// Creates a deterministic generator from the provided seed and stream selector.
    /// </summary>
    /// <param name="seed">Initial seed value for the generator state.</param>
    /// <param name="stream">Stream selector. Different values create independent sequences.</param>
    public PCG32(ulong seed, ulong stream = 54UL)
    {
        _inc = (stream << 1) | 1UL;
        _state = 0UL;

        NextUInt();
        _state += seed;
        NextUInt();
    }

    /// <summary>
    /// Creates a generator initialized from operating system entropy.
    /// </summary>
    /// <returns>A generator with a non-deterministic seed and stream.</returns>
    public static PCG32 CreateFromOsEntropy()
    {
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes);

        ulong seed = BinaryPrimitives.ReadUInt64LittleEndian(bytes[..8]);
        ulong stream = BinaryPrimitives.ReadUInt64LittleEndian(bytes[8..]);

        return new PCG32(seed, stream);
    }

    /// <summary>
    /// Returns the next 32-bit unsigned value from the generator.
    /// </summary>
    /// <returns>A pseudo-random 32-bit unsigned integer.</returns>
    public uint NextUInt()
    {
        ulong oldState = _state;
        _state = unchecked(oldState * Multiplier + _inc);

        uint xorshifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
        int rotation = (int)(oldState >> 59);

        return (xorshifted >> rotation) |
               (xorshifted << ((-rotation) & 31));
    }

    /// <summary>
    /// Returns a value in the range [0, bound).
    /// </summary>
    /// <param name="bound">Exclusive upper bound. Must be greater than zero.</param>
    /// <returns>A pseudo-random value less than <paramref name="bound"/>.</returns>
    public uint NextBounded(uint bound)
    {
        return NextBoundedCore(bound, NextUInt);
    }

    internal static uint NextBoundedCore(uint bound, Func<uint> nextUInt)
    {
        ArgumentNullException.ThrowIfNull(nextUInt);

        if (bound == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bound), "Bound must be greater than zero.");
        }

        uint threshold = unchecked(0u - bound) % bound;

        while (true)
        {
            uint value = nextUInt();
            if (value >= threshold)
            {
                return value % bound;
            }
        }
    }

    /// <summary>
    /// Returns a value in the range [minInclusive, maxExclusive).
    /// </summary>
    /// <param name="minInclusive">Inclusive lower bound.</param>
    /// <param name="maxExclusive">Exclusive upper bound.</param>
    /// <returns>A pseudo-random integer in the requested range.</returns>
    public int NextInt(int minInclusive, int maxExclusive)
    {
        if (minInclusive >= maxExclusive)
        {
            throw new ArgumentOutOfRangeException(nameof(minInclusive), "minInclusive must be less than maxExclusive.");
        }

        uint range = (uint)(maxExclusive - minInclusive);
        return minInclusive + (int)NextBounded(range);
    }

    /// <summary>
    /// Returns a value in the range [0, 1) with single-precision resolution.
    /// </summary>
    /// <returns>A pseudo-random single-precision floating-point value.</returns>
    public float NextSingle()
    {
        uint top24 = NextUInt() >> 8;
        return top24 * (1.0f / (1u << 24));
    }

    /// <summary>
    /// Returns a value in the range [0, 1) with double-precision resolution.
    /// </summary>
    /// <returns>A pseudo-random double-precision floating-point value.</returns>
    public double NextDouble()
    {
        ulong high = (ulong)(NextUInt() >> 5);
        ulong low = (ulong)(NextUInt() >> 6);
        ulong combined = (high << 26) | low;

        return combined * (1.0 / (1UL << 53));
    }
}
