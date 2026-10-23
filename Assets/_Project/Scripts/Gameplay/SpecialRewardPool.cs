using System;
using System.Collections.Generic;

namespace VertigoCase.Gameplay
{
    // Special rewards a super zone can offer. One is picked at random for every upcoming super zone.
    public sealed class SpecialRewardPool
    {
        private readonly IReadOnlyList<string> _rewardIds;
        private readonly IRandomProvider _random;

        public int Amount { get; }

        public SpecialRewardPool(IReadOnlyList<string> rewardIds, int amount, IRandomProvider random)
        {
            if (rewardIds == null || rewardIds.Count == 0) throw new ArgumentException("Pool is empty.", nameof(rewardIds));
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));

            _rewardIds = rewardIds;
            Amount = amount;
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public string PickRandom() => _rewardIds[_random.Range(0, _rewardIds.Count)];
    }
}
