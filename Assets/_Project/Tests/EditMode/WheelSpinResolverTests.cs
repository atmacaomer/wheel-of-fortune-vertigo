using System;
using NUnit.Framework;
using VertigoCase.Gameplay;

namespace VertigoCase.Tests.EditMode
{
    public class WheelSpinResolverTests
    {
        // "Loaded dice": always returns the value the test chose.
        private sealed class FixedRandomProvider : IRandomProvider
        {
            private readonly int _value;
            public FixedRandomProvider(int value) => _value = value;
            public int Range(int minInclusive, int maxExclusive) => _value;
        }

        private static readonly int[] Weights = { 40, 30, 10, 20 }; // ruler: [0-40) [40-70) [70-80) [80-100)

        [TestCase(0, 0)]
        [TestCase(39, 0)]
        [TestCase(40, 1)]
        [TestCase(75, 2)]
        [TestCase(99, 3)]
        public void PickSliceIndex_ReturnsSliceUnderTheRoll(int roll, int expectedIndex)
        {
            var resolver = new WheelSpinResolver(new FixedRandomProvider(roll));

            Assert.AreEqual(expectedIndex, resolver.PickSliceIndex(Weights));
        }

        [Test]
        public void PickSliceIndex_SkipsZeroWeightSlices()
        {
            var resolver = new WheelSpinResolver(new FixedRandomProvider(5));

            Assert.AreEqual(2, resolver.PickSliceIndex(new[] { 5, 0, 5 }));
        }

        [Test]
        public void PickSliceIndex_AllZeroWeights_Throws()
        {
            var resolver = new WheelSpinResolver(new FixedRandomProvider(0));

            Assert.Throws<ArgumentException>(() => resolver.PickSliceIndex(new[] { 0, 0 }));
        }

        [Test]
        public void PickSliceIndex_NoSlices_Throws()
        {
            var resolver = new WheelSpinResolver(new FixedRandomProvider(0));

            Assert.Throws<ArgumentException>(() => resolver.PickSliceIndex(Array.Empty<int>()));
        }
    }
}
