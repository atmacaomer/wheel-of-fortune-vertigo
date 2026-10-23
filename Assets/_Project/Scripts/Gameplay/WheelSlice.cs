using System;

namespace VertigoCase.Gameplay
{
    // What the rules need to know about one slice. Built from WheelConfigSO at startup;
    // sprites and other visuals stay on the view side.
    public sealed class WheelSlice
    {
        public string RewardId { get; }
        public int BaseAmount { get; }
        public int Weight { get; }
        public bool IsBomb { get; }

        public WheelSlice(string rewardId, int baseAmount, int weight, bool isBomb)
        {
            if (string.IsNullOrEmpty(rewardId)) throw new ArgumentException("Reward id is required.", nameof(rewardId));
            if (baseAmount < 0) throw new ArgumentOutOfRangeException(nameof(baseAmount));
            if (!isBomb && baseAmount == 0) throw new ArgumentException($"Reward slice '{rewardId}' needs an amount above 0.", nameof(baseAmount));
            if (weight < 0) throw new ArgumentOutOfRangeException(nameof(weight));

            RewardId = rewardId;
            BaseAmount = baseAmount;
            Weight = weight;
            IsBomb = isBomb;
        }
    }
}
