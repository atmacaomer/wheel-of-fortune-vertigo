using System;
using System.Collections.Generic;
using UnityEngine;
using VertigoCase.Gameplay;

namespace VertigoCase.Data
{
    // Game-wide tuning: which zones are safe/super, which wheel each uses, how fast rewards grow.
    [CreateAssetMenu(fileName = "ZoneProgression", menuName = "VertigoCase/Zone Progression")]
    public sealed class ZoneProgressionSO : ScriptableObject
    {
        [SerializeField, Min(1)] private int _safeZoneInterval = 5;
        [SerializeField, Min(1)] private int _superZoneInterval = 30;
        [Tooltip("0.1 = every zone pays 10% more than zone 1.")]
        [SerializeField, Min(0f)] private float _rewardGrowthPerZone = 0.1f;

        [SerializeField] private WheelConfigSO _normalWheel;
        [SerializeField] private WheelConfigSO _safeWheel;
        [SerializeField] private WheelConfigSO _superWheel;

        [Header("Super zone special reward")]
        [Tooltip("One of these is picked at random for every super zone and claimed on arrival.")]
        [SerializeField] private RewardDefinitionSO[] _specialRewardPool;
        [SerializeField, Min(1)] private int _specialRewardAmount = 1;

        [Header("Revive (continue with currency)")]
        [SerializeField] private RewardDefinitionSO _reviveCurrency;
        [SerializeField, Min(1)] private int _reviveCost = 25;
        [Tooltip("Currency in the wallet when the game starts (handy for testing revives).")]
        [SerializeField, Min(0)] private int _startingWalletCurrency;

        public int SafeZoneInterval => _safeZoneInterval;
        public int SuperZoneInterval => _superZoneInterval;
        public float RewardGrowthPerZone => _rewardGrowthPerZone;
        public IReadOnlyList<RewardDefinitionSO> SpecialRewardPool => _specialRewardPool;
        public int SpecialRewardAmount => _specialRewardAmount;
        public RewardDefinitionSO ReviveCurrency => _reviveCurrency;
        public int ReviveCost => _reviveCost;
        public int StartingWalletCurrency => _startingWalletCurrency;

        public WheelConfigSO GetWheel(ZoneType type)
        {
            switch (type)
            {
                case ZoneType.Normal: return _normalWheel;
                case ZoneType.Safe: return _safeWheel;
                case ZoneType.Super: return _superWheel;
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}
