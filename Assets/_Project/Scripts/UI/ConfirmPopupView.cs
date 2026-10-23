using System;
using UnityEngine;
using UnityEngine.UI;

namespace VertigoCase.UI
{
    // Generic "are you sure?" popup. The caller decides what confirming does, so the same view
    // serves both the exit and the give-up confirmation.
    public sealed class ConfirmPopupView : PopupView
    {
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        private Action _onConfirm;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (_confirmButton == null) _confirmButton = UIFind.Child<Button>(this, "ui_button_confirm");
            if (_cancelButton == null) _cancelButton = UIFind.Child<Button>(this, "ui_button_cancel");
        }

        public void Open(Action onConfirm)
        {
            _onConfirm = onConfirm;
            Show();
        }

        private void OnEnable()
        {
            _confirmButton.onClick.AddListener(HandleConfirmClicked);
            _cancelButton.onClick.AddListener(Hide);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _confirmButton.onClick.RemoveListener(HandleConfirmClicked);
            _cancelButton.onClick.RemoveListener(Hide);
        }

        private void HandleConfirmClicked()
        {
            Hide();
            _onConfirm?.Invoke();
        }
    }
}
