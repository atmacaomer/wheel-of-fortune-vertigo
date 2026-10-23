using System;

namespace VertigoCase.Gameplay
{
    // "Rewards get better every zone": amount = base * (1 + (zone - 1) * growthPerZone).
    public sealed class RewardScaling
    {
        private readonly double _growthPerZone;

        public RewardScaling(double growthPerZone)
        {
            if (growthPerZone < 0) throw new ArgumentOutOfRangeException(nameof(growthPerZone));
            _growthPerZone = growthPerZone;
        }

        public int GetAmount(int baseAmount, int zone)
        {
            if (zone <= 0) throw new ArgumentOutOfRangeException(nameof(zone));

            double multiplier = 1 + (zone - 1) * _growthPerZone;
            return (int)Math.Round(baseAmount * multiplier, MidpointRounding.AwayFromZero);
        }
    }
}
