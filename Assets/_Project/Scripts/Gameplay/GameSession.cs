using System;
using System.Collections.Generic;

namespace VertigoCase.Gameplay
{
    // One run of the wheel game as a small state machine. Every public method checks that it is
    // allowed in the current state, so the rules live here and not in button enable/disable code.
    public sealed class GameSession
    {
        private const int FirstZone = 1;

        private readonly ZoneRules _zoneRules;
        private readonly WheelSpinResolver _resolver;
        private readonly RewardScaling _scaling;
        private readonly RewardCollection _rewards;
        private readonly Wallet _wallet;
        private readonly IReadOnlyDictionary<ZoneType, IReadOnlyList<WheelSlice>> _wheels;
        private readonly SpecialRewardPool _specialRewards;
        private readonly string _reviveCurrencyId;

        // Chosen in Spin(), applied in CompleteSpin() after the animation.
        private int _pendingSliceIndex = -1;

        public event Action<GameState> StateChanged;
        public event Action<int> ZoneChanged;
        // Slice index the wheel must stop on; raised before the animation starts.
        public event Action<int> SpinStarted;
        // The special reward waiting on the next super zone changed (shown next to the super zone box).
        public event Action<string> UpcomingSpecialRewardChanged;

        public GameState State { get; private set; }
        public int CurrentZone { get; private set; }
        public ZoneType CurrentZoneType => _zoneRules.GetZoneType(CurrentZone);
        public IReadOnlyList<WheelSlice> CurrentSlices => _wheels[CurrentZoneType];
        // Rewards of the current run only; the wallet keeps what was cashed out before.
        public RewardCollection Rewards => _rewards;
        public int ReviveCost { get; }
        public string UpcomingSpecialRewardId { get; private set; }
        // Offered while State is SuperRewardOffer.
        public string OfferedSpecialRewardId { get; private set; }
        public int SpecialRewardAmount => _specialRewards.Amount;

        public bool CanSpin => State == GameState.ReadyToSpin;
        public bool CanExit => State == GameState.ReadyToSpin && _zoneRules.CanLeave(CurrentZone);
        public bool CanRevive => State == GameState.BombHit && _wallet.GetAmount(_reviveCurrencyId) >= ReviveCost;

        public GameSession(
            ZoneRules zoneRules,
            WheelSpinResolver resolver,
            RewardScaling scaling,
            RewardCollection rewards,
            Wallet wallet,
            IReadOnlyDictionary<ZoneType, IReadOnlyList<WheelSlice>> wheels,
            SpecialRewardPool specialRewards,
            string reviveCurrencyId,
            int reviveCost)
        {
            _zoneRules = zoneRules ?? throw new ArgumentNullException(nameof(zoneRules));
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _scaling = scaling ?? throw new ArgumentNullException(nameof(scaling));
            _rewards = rewards ?? throw new ArgumentNullException(nameof(rewards));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _wheels = wheels ?? throw new ArgumentNullException(nameof(wheels));
            _specialRewards = specialRewards ?? throw new ArgumentNullException(nameof(specialRewards));
            if (string.IsNullOrEmpty(reviveCurrencyId)) throw new ArgumentException("Revive currency is required.", nameof(reviveCurrencyId));
            if (reviveCost <= 0) throw new ArgumentOutOfRangeException(nameof(reviveCost));
            _reviveCurrencyId = reviveCurrencyId;
            ReviveCost = reviveCost;

            ValidateWheels();
            StartNewRun();
        }

        // Amount a slice pays in the current zone (shown on the wheel and added when won).
        public int GetSliceAmount(int sliceIndex) => _scaling.GetAmount(CurrentSlices[sliceIndex].BaseAmount, CurrentZone);

        public void Spin()
        {
            if (!CanSpin) throw new InvalidOperationException($"Cannot spin while {State}.");

            var slices = CurrentSlices;
            var weights = new int[slices.Count];
            for (int i = 0; i < slices.Count; i++)
            {
                weights[i] = slices[i].Weight;
            }

            _pendingSliceIndex = _resolver.PickSliceIndex(weights);
            SetState(GameState.Spinning);
            SpinStarted?.Invoke(_pendingSliceIndex);
        }

        // Called by the view when the spin animation has finished.
        public void CompleteSpin()
        {
            if (State != GameState.Spinning) throw new InvalidOperationException($"No spin to complete while {State}.");

            int index = _pendingSliceIndex;
            _pendingSliceIndex = -1;

            // Rewards are not cleared here but in StartNewRun, so a revive could still keep them.
            if (CurrentSlices[index].IsBomb)
            {
                SetState(GameState.BombHit);
                return;
            }

            _rewards.Add(CurrentSlices[index].RewardId, GetSliceAmount(index));
            CurrentZone++;
            ZoneChanged?.Invoke(CurrentZone);

            if (CurrentZoneType == ZoneType.Super)
            {
                // The promised special reward is offered now; the next super zone gets a new pick.
                OfferedSpecialRewardId = UpcomingSpecialRewardId;
                PickUpcomingSpecialReward();
                SetState(GameState.SuperRewardOffer);
                return;
            }

            SetState(GameState.ReadyToSpin);
        }

        // The special reward joins this run's rewards (so a later bomb can still take it), then the golden spin.
        public void ClaimSpecialReward()
        {
            if (State != GameState.SuperRewardOffer) throw new InvalidOperationException($"No special reward to claim while {State}.");

            _rewards.Add(OfferedSpecialRewardId, _specialRewards.Amount);
            OfferedSpecialRewardId = null;
            SetState(GameState.ReadyToSpin);
        }

        // Cash out: only between spins and only on safe/super zones.
        public void Exit()
        {
            if (!CanExit) throw new InvalidOperationException($"Cannot exit on zone {CurrentZone} while {State}.");

            // The run's rewards become the player's for good; the run list is cleared on the next run.
            _wallet.Deposit(_rewards.Amounts);
            SetState(GameState.CashedOut);
        }

        // Bonus continue system: pay from the wallet to keep the zone and this run's rewards.
        public void Revive()
        {
            if (!CanRevive) throw new InvalidOperationException($"Cannot revive while {State} with this much {_reviveCurrencyId}.");

            _wallet.TrySpend(_reviveCurrencyId, ReviveCost);
            SetState(GameState.ReadyToSpin);
        }

        // Fresh run from zone 1. Used at startup, after giving up on a bomb and after cashing out.
        public void StartNewRun()
        {
            if (State == GameState.Spinning) throw new InvalidOperationException("Cannot restart during a spin.");

            _rewards.Clear();
            CurrentZone = FirstZone;
            OfferedSpecialRewardId = null;
            PickUpcomingSpecialReward();
            ZoneChanged?.Invoke(CurrentZone);
            SetState(GameState.ReadyToSpin);
        }

        private void PickUpcomingSpecialReward()
        {
            UpcomingSpecialRewardId = _specialRewards.PickRandom();
            UpcomingSpecialRewardChanged?.Invoke(UpcomingSpecialRewardId);
        }

        private void SetState(GameState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }

        // Fail at startup instead of mid-game if the data is wrong (e.g. a bomb on the safe wheel).
        private void ValidateWheels()
        {
            foreach (ZoneType type in Enum.GetValues(typeof(ZoneType)))
            {
                if (!_wheels.TryGetValue(type, out var slices) || slices == null || slices.Count == 0)
                    throw new ArgumentException($"No wheel configured for {type} zones.");

                int totalWeight = 0;
                for (int i = 0; i < slices.Count; i++)
                {
                    totalWeight += slices[i].Weight;
                    if (type != ZoneType.Normal && slices[i].IsBomb)
                        throw new ArgumentException($"{type} wheel must not contain a bomb (slice {i}).");
                }
                if (totalWeight == 0) throw new ArgumentException($"{type} wheel has only zero weights.");
            }
        }
    }
}
