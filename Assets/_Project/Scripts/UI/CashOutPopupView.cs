using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Gameplay;

namespace VertigoCase.UI
{
    // Opens after the player leaves on a safe/super zone; the reward panel behind it shows what was won.
    public sealed class CashOutPopupView : PopupView
    {
        [SerializeField] private Button _continueButton;

        private GameSession _session;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (_continueButton == null) _continueButton = UIFind.Child<Button>(this, "ui_button_continue");
        }

        public void Init(GameSession session)
        {
            _session = session;
            _session.StateChanged += HandleStateChanged;
            Hide();
        }

        private void OnEnable() => _continueButton.onClick.AddListener(HandleContinueClicked);

        protected override void OnDisable()
        {
            base.OnDisable();
            _continueButton.onClick.RemoveListener(HandleContinueClicked);
        }

        private void OnDestroy()
        {
            if (_session != null) _session.StateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.CashedOut) Show();
            else Hide();
        }

        private void HandleContinueClicked() => _session.StartNewRun();
    }
}
