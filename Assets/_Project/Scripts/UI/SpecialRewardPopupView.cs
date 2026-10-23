using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Data;
using VertigoCase.Gameplay;

namespace VertigoCase.UI
{
    // Opens on arrival at a super zone and hands out its special reward before the golden spin.
    public sealed class SpecialRewardPopupView : PopupView
    {
        [SerializeField] private Image _rewardIcon;
        [SerializeField] private TMP_Text _rewardNameText;
        [SerializeField] private Button _claimButton;

        private GameSession _session;
        private IReadOnlyDictionary<string, RewardDefinitionSO> _definitions;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (_rewardIcon == null) _rewardIcon = UIFind.Child<Image>(this, "ui_image_popup_special_reward_icon_value");
            if (_rewardNameText == null) _rewardNameText = UIFind.Child<TMP_Text>(this, "ui_text_popup_special_reward_name_value");
            if (_claimButton == null) _claimButton = UIFind.Child<Button>(this, "ui_button_claim");
        }

        public void Init(GameSession session, IReadOnlyDictionary<string, RewardDefinitionSO> definitions)
        {
            _session = session;
            _definitions = definitions;
            _session.StateChanged += HandleStateChanged;
            Hide();
        }

        private void OnEnable() => _claimButton.onClick.AddListener(HandleClaimClicked);

        protected override void OnDisable()
        {
            base.OnDisable();
            _claimButton.onClick.RemoveListener(HandleClaimClicked);
        }

        private void OnDestroy()
        {
            if (_session != null) _session.StateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState state)
        {
            if (state != GameState.SuperRewardOffer)
            {
                Hide();
                return;
            }

            RewardDefinitionSO reward = _definitions[_session.OfferedSpecialRewardId];
            _rewardIcon.sprite = reward.Icon;
            _rewardNameText.text = reward.DisplayName + "  x" + _session.SpecialRewardAmount;
            Show();
        }

        private void HandleClaimClicked() => _session.ClaimSpecialReward();
    }
}
