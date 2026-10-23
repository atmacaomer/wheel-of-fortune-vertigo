using System;
using System.Collections.Generic;

namespace VertigoCase.Gameplay
{
    // The player's permanent stash. Cashed-out rewards are deposited here; revives are paid from here.
    // Reuses RewardCollection for the bookkeeping but never exposes Clear: a wallet is never wiped.
    public sealed class Wallet
    {
        private readonly RewardCollection _items = new RewardCollection();

        // (rewardId, newTotal)
        public event Action<string, int> Changed
        {
            add => _items.RewardChanged += value;
            remove => _items.RewardChanged -= value;
        }

        public int GetAmount(string rewardId) => _items.GetAmount(rewardId);

        public void Add(string rewardId, int amount) => _items.Add(rewardId, amount);

        public void Deposit(IReadOnlyDictionary<string, int> rewards)
        {
            if (rewards == null) throw new ArgumentNullException(nameof(rewards));

            foreach (KeyValuePair<string, int> reward in rewards)
            {
                _items.Add(reward.Key, reward.Value);
            }
        }

        public bool TrySpend(string rewardId, int amount) => _items.TrySpend(rewardId, amount);
    }
}
