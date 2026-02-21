using Pcg32Toolkit;

namespace Pcg32Toolkit.Tests;

public sealed class PCG32Tests
{
    [Fact]
    public void NextUInt_KnownSeedAndStream_MatchesReferenceSequence()
    {
        var rng = new PCG32(seed: 42UL, stream: 54UL);

        uint[] expected =
        [
            0xa15c02b7u,
            0x7b47f409u,
            0xba1d3330u,
            0x83d2f293u,
            0xbfa4784bu,
            0xcbed606eu
        ];

        foreach (uint expectedValue in expected)
        {
            Assert.Equal(expectedValue, rng.NextUInt());
        }
    }

    [Fact]
    public void NextUInt_SameSeedAndStream_ProducesSameSequence()
    {
        var first = new PCG32(seed: 123456789UL, stream: 987654321UL);
        var second = new PCG32(seed: 123456789UL, stream: 987654321UL);

        for (int i = 0; i < 1024; i++)
        {
            Assert.Equal(first.NextUInt(), second.NextUInt());
        }
    }

    [Fact]
    public void NextUInt_DifferentStreams_ProduceDifferentSequences()
    {
        var first = new PCG32(seed: 42UL, stream: 54UL);
        var second = new PCG32(seed: 42UL, stream: 55UL);

        bool hasDifference = false;

        for (int i = 0; i < 64; i++)
        {
            if (first.NextUInt() != second.NextUInt())
            {
                hasDifference = true;
                break;
            }
        }

        Assert.True(hasDifference);
    }

    [Fact]
    public void NextBounded_WhenBoundIsZero_ThrowsWithArgumentNameAndMessage()
    {
        var rng = new PCG32(seed: 1UL, stream: 1UL);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextBounded(0));
        Assert.Equal("bound", exception.ParamName);
        Assert.Contains("Bound must be greater than zero.", exception.Message);
    }

    [Fact]
    public void NextBoundedCore_WhenProviderIsNull_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => PCG32.NextBoundedCore(6u, null!));

        Assert.Equal("nextUInt", exception.ParamName);
    }

    [Fact]
    public void NextBoundedCore_RejectsBelowThresholdAndAcceptsNextValue()
    {
        Func<uint> provider = CreateSequenceProvider(0u, 4u);

        uint result = PCG32.NextBoundedCore(6u, provider);

        Assert.Equal(4u, result);
    }

    [Fact]
    public void NextBoundedCore_ValueEqualToThreshold_IsAccepted()
    {
        Func<uint> provider = CreateSequenceProvider(4u);

        uint result = PCG32.NextBoundedCore(6u, provider);

        Assert.Equal(4u, result);
    }

    [Theory]
    [InlineData(1u)]
    [InlineData(2u)]
    [InlineData(6u)]
    [InlineData(97u)]
    [InlineData(1_000_000u)]
    public void NextBounded_StaysWithinRange(uint bound)
    {
        var rng = new PCG32(seed: 2026UL, stream: 99UL);

        for (int i = 0; i < 10_000; i++)
        {
            uint value = rng.NextBounded(bound);
            Assert.InRange(value, 0u, bound - 1u);
        }
    }

    [Fact]
    public void NextBounded_WhenBoundIsOne_AlwaysReturnsZero()
    {
        var rng = new PCG32(seed: 2UL, stream: 3UL);

        for (int i = 0; i < 10_000; i++)
        {
            Assert.Equal(0u, rng.NextBounded(1u));
        }
    }

    [Fact]
    public void NextBounded_KnownSeedAndBound_MatchesReferenceSequence()
    {
        var rng = new PCG32(seed: 42UL, stream: 54UL);

        uint[] expected = [3u, 3u, 2u, 1u, 1u, 4u];

        foreach (uint expectedValue in expected)
        {
            Assert.Equal(expectedValue, rng.NextBounded(6u));
        }
    }

    [Fact]
    public void NextInt_WhenRangeIsInvalid_ThrowsWithCorrectArgumentName()
    {
        var rng = new PCG32(seed: 12UL, stream: 34UL);

        var equalRangeException = Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(5, 5));
        Assert.Equal("minInclusive", equalRangeException.ParamName);
        Assert.Contains("minInclusive must be less than maxExclusive.", equalRangeException.Message);

        var descendingRangeException = Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(6, 5));
        Assert.Equal("minInclusive", descendingRangeException.ParamName);
        Assert.Contains("minInclusive must be less than maxExclusive.", descendingRangeException.Message);
    }

    [Fact]
    public void NextInt_StaysWithinRange_WithNegativeMin()
    {
        var rng = new PCG32(seed: 17UL, stream: 25UL);

        for (int i = 0; i < 10_000; i++)
        {
            int value = rng.NextInt(-10, 10);
            Assert.InRange(value, -10, 9);
        }
    }

    [Fact]
    public void NextSingle_AlwaysReturnsUnitIntervalValues()
    {
        var rng = new PCG32(seed: 100UL, stream: 200UL);

        for (int i = 0; i < 10_000; i++)
        {
            float value = rng.NextSingle();
            Assert.True(value >= 0.0f && value < 1.0f);
        }
    }

    [Fact]
    public void NextSingle_KnownSeedAndStream_MatchesReferenceValue()
    {
        var rng = new PCG32(seed: 42UL, stream: 54UL);

        Assert.Equal(0.63031018f, rng.NextSingle(), precision: 7);
    }

    [Fact]
    public void NextDouble_AlwaysReturnsUnitIntervalValues()
    {
        var rng = new PCG32(seed: 100UL, stream: 200UL);

        for (int i = 0; i < 10_000; i++)
        {
            double value = rng.NextDouble();
            Assert.True(value >= 0.0 && value < 1.0);
        }
    }

    [Fact]
    public void NextDouble_KnownSeedAndStream_MatchesReferenceValue()
    {
        var rng = new PCG32(seed: 42UL, stream: 54UL);

        Assert.Equal(0.6303102186438938, rng.NextDouble(), precision: 15);
    }

    [Fact]
    public void CreateFromOsEntropy_CreatesUsableGenerator()
    {
        var rng = PCG32.CreateFromOsEntropy();

        uint value = rng.NextUInt();

        Assert.InRange(value, uint.MinValue, uint.MaxValue);
        Assert.InRange(rng.NextSingle(), 0.0f, float.BitDecrement(1.0f));
        Assert.InRange(rng.NextDouble(), 0.0, double.BitDecrement(1.0));
    }

    [Fact]
    public void CreateFromOsEntropy_TwoInstances_DoNotMatchEntirePrefix()
    {
        var first = PCG32.CreateFromOsEntropy();
        var second = PCG32.CreateFromOsEntropy();

        bool allEqual = true;

        for (int i = 0; i < 16; i++)
        {
            if (first.NextUInt() != second.NextUInt())
            {
                allEqual = false;
                break;
            }
        }

        Assert.False(allEqual);
    }

    private static Func<uint> CreateSequenceProvider(params uint[] values)
    {
        var queue = new Queue<uint>(values);
        return () => queue.Dequeue();
    }
}
