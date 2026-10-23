using System;
using NUnit.Framework;
using VertigoCase.Gameplay;

namespace VertigoCase.Tests.EditMode
{
    public class RewardScalingTests
    {
        [TestCase(1, 100)]  // first zone: no growth
        [TestCase(2, 110)]
        [TestCase(11, 200)] // 10 zones of +10% → double
        public void GetAmount_GrowsTenPercentPerZone(int zone, int expected)
        {
            var scaling = new RewardScaling(growthPerZone: 0.1);

            Assert.AreEqual(expected, scaling.GetAmount(100, zone));
        }

        [Test]
        public void GetAmount_ZeroGrowth_KeepsBaseAmount()
        {
            var scaling = new RewardScaling(growthPerZone: 0);

            Assert.AreEqual(50, scaling.GetAmount(50, 25));
        }

        [Test]
        public void GetAmount_ZoneBelowOne_Throws()
        {
            var scaling = new RewardScaling(0.1);

            Assert.Throws<ArgumentOutOfRangeException>(() => scaling.GetAmount(100, 0));
        }

        [Test]
        public void Constructor_NegativeGrowth_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RewardScaling(-0.1));
        }
    }
}
