using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Gameplay;

namespace VertigoCase.UI
{
    // Opens when the wheel lands on the bomb: give up (lose the run) or revive with wallet gold.
    public sealed class BombPopupView : PopupView
    {
        [SerializeField] private Button _giveUpButton;
        [SerializeField] private Button _reviveButton;
        [SerializeField] private TMP_Text _reviveCostText;
        [SerializeField] private CanvasGroup _reviveGroup;
        [SerializeField, Range(0f, 1f)] private float _unaffordableAlpha = 0.4f;

        private GameSession _session;
        private ConfirmPopupView _giveUpConfirmPopup;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (_giveUpButton == null) _giveUpButton = UIFind.Child<Button>(this, "ui_button_give_up");
            if (_reviveButton == null) _reviveButton = UIFind.Child<Button>(this, "ui_button_revive");
            if (_reviveCostText == null) _reviveCostText = UIFind.Child<TMP_Text>(this, "ui_text_revive_cost_value");
            if (_reviveGroup == null && _reviveButton != null) _reviveGroup = _reviveButton.GetComponent<CanvasGroup>();
        }

        public void Init(GameSession session, ConfirmPopupView giveUpConfirmPopup)
        {
            _session = session;
            _giveUpConfirmPopup = giveUpConfirmPopup;
            _session.StateChanged += HandleStateChanged;
            _reviveCostText.text = "REVIVE  " + _session.ReviveCost;
            Hide();
        }

        private void OnEnable()
        {
            _giveUpButton.onClick.AddListener(HandleGiveUpClicked);
            _reviveButton.onClick.AddListener(HandleReviveClicked);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _giveUpButton.onClick.RemoveListener(HandleGiveUpClicked);
            _reviveButton.onClick.RemoveListener(HandleReviveClicked);
        }

        private void OnDestroy()
        {
            if (_session != null) _session.StateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.BombHit)
            {
                // Dim the whole button (fill, frame and text) when the wallet cannot pay.
                bool canRevive = _session.CanRevive;
                _reviveButton.interactable = canRevive;
                _reviveGroup.alpha = canRevive ? 1f : _unaffordableAlpha;
                Show();
            }
            else
            {
                Hide();
            }
        }

        // Losing everything is asked once more before it happens.
        private void HandleGiveUpClicked() => _giveUpConfirmPopup.Open(_session.StartNewRun);

        private void HandleReviveClicked() => _session.Revive();
    }
}
