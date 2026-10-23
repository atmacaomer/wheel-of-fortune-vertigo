using System;

namespace VertigoCase.Gameplay
{
    // Real dice used in the game. System.Random keeps the gameplay assembly free of UnityEngine
    // and allows an optional seed to replay the exact same run when debugging.
    public sealed class SystemRandomProvider : IRandomProvider
    {
        private readonly Random _random;

        public SystemRandomProvider() : this(new Random()) { }

        public SystemRandomProvider(int seed) : this(new Random(seed)) { }

        private SystemRandomProvider(Random random)
        {
            _random = random;
        }

        public int Range(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);
    }
}
