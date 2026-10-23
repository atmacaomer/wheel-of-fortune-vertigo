using System;
using NUnit.Framework;
using VertigoCase.Gameplay;

namespace VertigoCase.Tests.EditMode
{
    public class WheelSliceTests
    {
        [Test]
        public void RewardSliceWithZeroAmount_Throws()
        {
            Assert.Throws<ArgumentException>(() => new WheelSlice("gold", 0, 1, isBomb: false));
        }

        [Test]
        public void BombSliceWithZeroAmount_IsAllowed()
        {
            Assert.DoesNotThrow(() => new WheelSlice("bomb", 0, 1, isBomb: true));
        }
    }
}
