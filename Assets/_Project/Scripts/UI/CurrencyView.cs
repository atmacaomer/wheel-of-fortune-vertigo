using TMPro;
using UnityEngine;
using VertigoCase.Gameplay;

namespace VertigoCase.UI
{
    // Top-left counter: how much of the revive currency (gold) the wallet holds.
    public sealed class CurrencyView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _amountText;

        private Wallet _wallet;
        private string _currencyId;

        private void OnValidate()
        {
            if (_amountText == null) _amountText = UIFind.Child<TMP_Text>(this, "ui_text_gold_amount_value");
        }

        public void Init(Wallet wallet, string currencyId)
        {
            _wallet = wallet;
            _currencyId = currencyId;
            _wallet.Changed += HandleWalletChanged;
            ShowAmount(_wallet.GetAmount(_currencyId));
        }

        private void OnDestroy()
        {
            if (_wallet != null) _wallet.Changed -= HandleWalletChanged;
        }

        private void HandleWalletChanged(string rewardId, int total)
        {
            if (rewardId == _currencyId) ShowAmount(total);
        }

        private void ShowAmount(int amount) => _amountText.text = amount.ToString();
    }
}
