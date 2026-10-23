using System;
using NUnit.Framework;
using VertigoCase.Gameplay;

namespace VertigoCase.Tests.EditMode
{
    public class SpecialRewardPoolTests
    {
        private sealed class FixedRandomProvider : IRandomProvider
        {
            private readonly int _value;

            public FixedRandomProvider(int value)
            {
                _value = value;
            }

            public int Range(int minInclusive, int maxExclusive) => _value;
        }

        [Test]
        public void PickRandom_ReturnsTheRolledReward()
        {
            var pool = new SpecialRewardPool(new[] { "knife", "sniper", "rifle" }, 1, new FixedRandomProvider(2));

            Assert.AreEqual("rifle", pool.PickRandom());
        }

        [Test]
        public void Constructor_EmptyPool_Throws()
        {
            Assert.Throws<ArgumentException>(() => new SpecialRewardPool(new string[0], 1, new FixedRandomProvider(0)));
        }
    }
}
