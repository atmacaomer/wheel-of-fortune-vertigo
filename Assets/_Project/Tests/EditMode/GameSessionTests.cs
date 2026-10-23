using System;
using System.Collections.Generic;
using NUnit.Framework;
using VertigoCase.Gameplay;

namespace VertigoCase.Tests.EditMode
{
    public class GameSessionTests
    {
        // Roll 0 lands on the first slice, roll 1 on the second (all weights are 1).
        private const int RollReward = 0;
        private const int RollBomb = 1;
        private const int ReviveCost = 15;

        private sealed class ScriptedRandomProvider : IRandomProvider
        {
            public int NextRoll { get; set; }

            public int Range(int minInclusive, int maxExclusive) => NextRoll;
        }

        private ScriptedRandomProvider _random;
        private RewardCollection _rewards;
        private Wallet _wallet;
        private GameSession _session;

        [SetUp]
        public void SetUp()
        {
            _random = new ScriptedRandomProvider();
            _rewards = new RewardCollection();
            _wallet = new Wallet();
            _session = CreateSession(CreateWheels(bombOnSafeWheel: false));
        }

        private GameSession CreateSession(Dictionary<ZoneType, IReadOnlyList<WheelSlice>> wheels)
        {
            return new GameSession(
                new ZoneRules(5, 30),
                new WheelSpinResolver(_random),
                new RewardScaling(0.1),
                _rewards,
                _wallet,
                wheels,
                new SpecialRewardPool(new[] { "knife", "sniper" }, 1, _random),
                "gold",
                ReviveCost);
        }

        private void SpinRewardsUntilZone(int zone)
        {
            while (_session.CurrentZone < zone) SpinAndComplete(RollReward);
        }

        private static Dictionary<ZoneType, IReadOnlyList<WheelSlice>> CreateWheels(bool bombOnSafeWheel)
        {
            return new Dictionary<ZoneType, IReadOnlyList<WheelSlice>>
            {
                [ZoneType.Normal] = new[] { new WheelSlice("gold", 10, 1, false), new WheelSlice("bomb", 0, 1, true) },
                [ZoneType.Safe] = new[] { new WheelSlice("gold", 10, 1, false), new WheelSlice("cash", 50, 1, bombOnSafeWheel) },
                [ZoneType.Super] = new[] { new WheelSlice("gold", 10, 1, false), new WheelSlice("cash", 50, 1, false) },
            };
        }

        private void SpinAndComplete(int roll)
        {
            _random.NextRoll = roll;
            _session.Spin();
            _session.CompleteSpin();
        }

        [Test]
        public void NewSession_StartsReadyOnZoneOne()
        {
            Assert.AreEqual(GameState.ReadyToSpin, _session.State);
            Assert.AreEqual(1, _session.CurrentZone);
        }

        [Test]
        public void Spin_RaisesSpinStartedWithPickedIndex()
        {
            int startedIndex = -1;
            _session.SpinStarted += index => startedIndex = index;
            _random.NextRoll = RollBomb;

            _session.Spin();

            Assert.AreEqual(1, startedIndex);
            Assert.AreEqual(GameState.Spinning, _session.State);
        }

        [Test]
        public void Spin_WhileSpinning_Throws()
        {
            _session.Spin();

            Assert.Throws<InvalidOperationException>(() => _session.Spin());
        }

        [Test]
        public void CompleteSpin_Reward_AddsRewardAndAdvancesZone()
        {
            SpinAndComplete(RollReward);

            Assert.AreEqual(10, _rewards.GetAmount("gold"));
            Assert.AreEqual(2, _session.CurrentZone);
            Assert.AreEqual(GameState.ReadyToSpin, _session.State);
        }

        [Test]
        public void CompleteSpin_RewardGrowsWithZone()
        {
            SpinAndComplete(RollReward); // zone 1: 10
            SpinAndComplete(RollReward); // zone 2: 10 * 1.1 = 11

            Assert.AreEqual(21, _rewards.GetAmount("gold"));
        }

        [Test]
        public void CompleteSpin_Bomb_StaysOnZoneInBombHit()
        {
            SpinAndComplete(RollReward);

            SpinAndComplete(RollBomb);

            Assert.AreEqual(GameState.BombHit, _session.State);
            Assert.AreEqual(2, _session.CurrentZone);
        }

        [Test]
        public void StartNewRun_AfterBomb_ClearsRewardsAndResetsZone()
        {
            SpinAndComplete(RollReward);
            SpinAndComplete(RollBomb);

            _session.StartNewRun();

            Assert.AreEqual(0, _rewards.Amounts.Count);
            Assert.AreEqual(1, _session.CurrentZone);
            Assert.AreEqual(GameState.ReadyToSpin, _session.State);
        }

        [Test]
        public void Revive_PaysFromWallet_KeepsZoneAndRunRewards()
        {
            _wallet.Add("gold", 20);
            SpinAndComplete(RollReward); // 10 gold in this run
            SpinAndComplete(RollBomb);

            _session.Revive();

            Assert.AreEqual(GameState.ReadyToSpin, _session.State);
            Assert.AreEqual(2, _session.CurrentZone);
            Assert.AreEqual(20 - ReviveCost, _wallet.GetAmount("gold"));
            Assert.AreEqual(10, _rewards.GetAmount("gold"));
        }

        [Test]
        public void Revive_RunGoldDoesNotCount_OnlyWalletGold()
        {
            SpinAndComplete(RollReward); // 10 gold
            SpinAndComplete(RollReward); // +11 = 21 run gold, but the wallet is empty
            SpinAndComplete(RollBomb);

            Assert.IsFalse(_session.CanRevive);
            Assert.Throws<InvalidOperationException>(() => _session.Revive());
        }

        [Test]
        public void Exit_OnNormalZone_IsNotAllowed()
        {
            Assert.IsFalse(_session.CanExit);
            Assert.Throws<InvalidOperationException>(() => _session.Exit());
        }

        [Test]
        public void Exit_OnSafeZone_DepositsRunRewardsIntoWallet()
        {
            for (int i = 0; i < 4; i++) SpinAndComplete(RollReward); // now on zone 5
            int runGold = _rewards.GetAmount("gold");

            Assert.IsTrue(_session.CanExit);
            _session.Exit();

            Assert.AreEqual(GameState.CashedOut, _session.State);
            Assert.AreEqual(runGold, _wallet.GetAmount("gold"));
        }

        [Test]
        public void StartNewRun_KeepsWallet()
        {
            _wallet.Add("gold", 50);
            SpinAndComplete(RollReward);
            SpinAndComplete(RollBomb);

            _session.StartNewRun();

            Assert.AreEqual(50, _wallet.GetAmount("gold"));
        }

        [Test]
        public void SafeZone_UsesSafeWheel()
        {
            for (int i = 0; i < 4; i++) SpinAndComplete(RollReward);

            Assert.AreEqual(ZoneType.Safe, _session.CurrentZoneType);
            Assert.AreEqual("cash", _session.CurrentSlices[1].RewardId);
        }

        [Test]
        public void Constructor_BombOnSafeWheel_Throws()
        {
            Assert.Throws<ArgumentException>(() => CreateSession(CreateWheels(bombOnSafeWheel: true)));
        }

        [Test]
        public void Constructor_WheelWithOnlyZeroWeights_Throws()
        {
            var wheels = CreateWheels(bombOnSafeWheel: false);
            wheels[ZoneType.Safe] = new[] { new WheelSlice("gold", 10, 0, false), new WheelSlice("cash", 50, 0, false) };

            Assert.Throws<ArgumentException>(() => CreateSession(wheels));
        }

        [Test]
        public void NewRun_AlreadyKnowsTheUpcomingSpecialReward()
        {
            Assert.AreEqual("knife", _session.UpcomingSpecialRewardId); // roll 0 -> first in the pool
        }

        [Test]
        public void ReachingSuperZone_OffersTheSpecialRewardInsteadOfSpinning()
        {
            SpinRewardsUntilZone(30);

            Assert.AreEqual(GameState.SuperRewardOffer, _session.State);
            Assert.AreEqual("knife", _session.OfferedSpecialRewardId);
            Assert.IsFalse(_session.CanSpin);
            Assert.IsFalse(_session.CanExit);
        }

        [Test]
        public void ReachingSuperZone_PicksTheRewardForTheNextSuperZone()
        {
            int picks = 0;
            _session.UpcomingSpecialRewardChanged += id => picks++;

            SpinRewardsUntilZone(30);

            Assert.AreEqual(1, picks);
        }

        [Test]
        public void ClaimSpecialReward_AddsItToTheRunAndAllowsTheGoldenSpin()
        {
            SpinRewardsUntilZone(30);

            _session.ClaimSpecialReward();

            Assert.AreEqual(1, _rewards.GetAmount("knife"));
            Assert.AreEqual(GameState.ReadyToSpin, _session.State);
            Assert.IsTrue(_session.CanExit);
        }

        [Test]
        public void ClaimSpecialReward_WithoutOffer_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _session.ClaimSpecialReward());
        }
    }
}
