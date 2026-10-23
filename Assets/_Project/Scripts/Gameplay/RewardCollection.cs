using System;
using System.Collections.Generic;

namespace VertigoCase.Gameplay
{
    // Rewards collected during the current run. Lost on a bomb, kept on cash out.
    public sealed class RewardCollection
    {
        private readonly Dictionary<string, int> _amounts = new Dictionary<string, int>();

        // (rewardId, newTotal) — the reward panel adds or updates a row.
        public event Action<string, int> RewardChanged;
        public event Action Cleared;

        public IReadOnlyDictionary<string, int> Amounts => _amounts;

        public void Add(string rewardId, int amount)
        {
            if (string.IsNullOrEmpty(rewardId)) throw new ArgumentException("Reward id is required.", nameof(rewardId));
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));

            _amounts.TryGetValue(rewardId, out int current);
            int total = current + amount;
            _amounts[rewardId] = total;
            RewardChanged?.Invoke(rewardId, total);
        }

        public int GetAmount(string rewardId) => _amounts.TryGetValue(rewardId, out int amount) ? amount : 0;

        // Pays with a collected reward (e.g. gold for a revive). Returns false and changes nothing if short.
        public bool TrySpend(string rewardId, int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));

            int current = GetAmount(rewardId);
            if (current < amount) return false;

            int total = current - amount;
            if (total == 0) _amounts.Remove(rewardId);
            else _amounts[rewardId] = total;

            RewardChanged?.Invoke(rewardId, total);
            return true;
        }

        public void Clear()
        {
            _amounts.Clear();
            Cleared?.Invoke();
        }
    }
}
