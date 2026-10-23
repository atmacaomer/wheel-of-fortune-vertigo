using System;
using System.Collections.Generic;
using UnityEngine;
using VertigoCase.Data;
using VertigoCase.Gameplay;
using VertigoCase.UI;

namespace VertigoCase.Core
{
    // Composition root: the only place that knows every piece. It turns the ScriptableObject data
    // into plain rule objects, creates the session and hands it to the views. No singletons needed.
    public sealed class GameBootstrapper : MonoBehaviour
    {
        // Unity caps mobile builds at 30 fps unless a target is set; the wheel and fly animations need 60.
        private const int TargetFrameRate = 60;

        [SerializeField] private ZoneProgressionSO _progression;
        [SerializeField] private WheelView _wheelView;
        [SerializeField] private ZoneBarView _zoneBarView;
        [SerializeField] private ZoneInfoView _zoneInfoView;
        [SerializeField] private RewardPanelView _rewardPanelView;
        [SerializeField] private BombPopupView _bombPopupView;
        [SerializeField] private CashOutPopupView _cashOutPopupView;
        [SerializeField] private ConfirmPopupView _exitConfirmPopup;
        [SerializeField] private ConfirmPopupView _giveUpConfirmPopup;
        [SerializeField] private CurrencyView _currencyView;
        [SerializeField] private SpecialRewardPopupView _specialRewardPopupView;

        private void OnValidate()
        {
            if (_currencyView == null) _currencyView = FindAnyObjectByType<CurrencyView>(FindObjectsInactive.Include);
            if (_exitConfirmPopup == null) _exitConfirmPopup = FindConfirmPopup("ui_popup_exit_confirm");
            if (_giveUpConfirmPopup == null) _giveUpConfirmPopup = FindConfirmPopup("ui_popup_give_up_confirm");
            if (_wheelView == null) _wheelView = FindAnyObjectByType<WheelView>(FindObjectsInactive.Include);
            if (_zoneBarView == null) _zoneBarView = FindAnyObjectByType<ZoneBarView>(FindObjectsInactive.Include);
            if (_zoneInfoView == null) _zoneInfoView = FindAnyObjectByType<ZoneInfoView>(FindObjectsInactive.Include);
            if (_rewardPanelView == null) _rewardPanelView = FindAnyObjectByType<RewardPanelView>(FindObjectsInactive.Include);
            if (_bombPopupView == null) _bombPopupView = FindAnyObjectByType<BombPopupView>(FindObjectsInactive.Include);
            if (_cashOutPopupView == null) _cashOutPopupView = FindAnyObjectByType<CashOutPopupView>(FindObjectsInactive.Include);
            if (_specialRewardPopupView == null) _specialRewardPopupView = FindAnyObjectByType<SpecialRewardPopupView>(FindObjectsInactive.Include);
        }

        private void Awake()
        {
            Application.targetFrameRate = TargetFrameRate;
        }

        // Start (not Awake) so every view has finished its own Awake/OnEnable first.
        private void Start()
        {
            var zoneRules = new ZoneRules(_progression.SafeZoneInterval, _progression.SuperZoneInterval);
            var wheels = new Dictionary<ZoneType, IReadOnlyList<WheelSlice>>();
            var rewardDefinitions = new Dictionary<string, RewardDefinitionSO>();

            foreach (ZoneType type in Enum.GetValues(typeof(ZoneType)))
            {
                WheelConfigSO wheel = _progression.GetWheel(type);
                wheels[type] = wheel.CreateSlices();

                // The reward panel looks icons up by id, so collect every reward used by any wheel.
                foreach (WheelConfigSO.SliceData slice in wheel.Slices)
                {
                    rewardDefinitions[slice.Reward.Id] = slice.Reward;
                }
            }

            var specialRewardIds = new List<string>();
            foreach (RewardDefinitionSO reward in _progression.SpecialRewardPool)
            {
                specialRewardIds.Add(reward.Id);
                rewardDefinitions[reward.Id] = reward;
            }

            string currencyId = _progression.ReviveCurrency.Id;
            var wallet = new Wallet();
            if (_progression.StartingWalletCurrency > 0) wallet.Add(currencyId, _progression.StartingWalletCurrency);

            // One dice for every game decision (spin results and special reward picks).
            var random = new SystemRandomProvider();
            var session = new GameSession(
                zoneRules,
                new WheelSpinResolver(random),
                new RewardScaling(_progression.RewardGrowthPerZone),
                new RewardCollection(),
                wallet,
                wheels,
                new SpecialRewardPool(specialRewardIds, _progression.SpecialRewardAmount, random),
                currencyId,
                _progression.ReviveCost);

            _wheelView.Init(session, _progression);
            _zoneBarView.Init(session, zoneRules);
            _zoneInfoView.Init(session, zoneRules, rewardDefinitions);
            _specialRewardPopupView.Init(session, rewardDefinitions);
            _rewardPanelView.Init(session, rewardDefinitions, _exitConfirmPopup, _wheelView.RewardOrigin);
            _bombPopupView.Init(session, _giveUpConfirmPopup);
            _cashOutPopupView.Init(session);
            _currencyView.Init(wallet, currencyId);
        }

        // Both confirmations share one view type, so they are told apart by their object names.
        private static ConfirmPopupView FindConfirmPopup(string objectName)
        {
            foreach (ConfirmPopupView popup in FindObjectsByType<ConfirmPopupView>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (popup.name == objectName) return popup;
            }
            return null;
        }
    }
}
