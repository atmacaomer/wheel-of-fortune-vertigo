using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VertigoCase.UI
{
    // One line in the collected rewards panel. Wins count up step by step with a small pulse.
    public sealed class RewardRowView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private float _punchStrength = 0.25f;
        [SerializeField, Min(0.01f)] private float _punchDuration = 0.3f;
        [Tooltip("Seconds per +1 step; large wins are capped by Max Count Duration.")]
        [SerializeField, Min(0.01f)] private float _countStepDuration = 0.08f;
        [SerializeField, Min(0.01f)] private float _maxCountDuration = 0.8f;
        [SerializeField] private float _countPulseStrength = 0.12f;
        [SerializeField, Min(0.01f)] private float _countPulseDuration = 0.1f;

        private Tween _punchTween;
        private Tween _countTween;
        private Tween _pulseTween;

        // Amount currently shown; counting starts from here.
        public int Amount { get; private set; }
        public Image Icon => _iconImage;

        private void OnValidate()
        {
            if (_iconImage == null) _iconImage = UIFind.Child<Image>(this, "ui_image_reward_icon_value");
            if (_amountText == null) _amountText = UIFind.Child<TMP_Text>(this, "ui_text_reward_amount_value");
        }

        private void OnDestroy()
        {
            _punchTween?.Kill();
            _countTween?.Kill();
            _pulseTween?.Kill();
        }

        public void SetIcon(Sprite icon) => _iconImage.sprite = icon;

        // A new row stays empty until its first reward icon has flown in.
        public void ClearAmount()
        {
            _countTween?.Kill();
            Amount = 0;
            _amountText.text = string.Empty;
        }

        public void CountTo(int target)
        {
            _countTween?.Kill();
            float duration = Mathf.Min((target - Amount) * _countStepDuration, _maxCountDuration);
            _countTween = DOTween.To(() => Amount, ShowCountStep, target, duration).SetEase(Ease.Linear);
        }

        public void Punch()
        {
            _punchTween?.Kill(true);
            _punchTween = _iconImage.rectTransform.DOPunchScale(Vector3.one * _punchStrength, _punchDuration);
        }

        private void ShowCountStep(int value)
        {
            if (value == Amount) return;

            ShowAmount(value);
            // Complete the previous pulse first so the scale never drifts away from 1.
            _pulseTween?.Kill(true);
            _pulseTween = _amountText.rectTransform.DOPunchScale(Vector3.one * _countPulseStrength, _countPulseDuration);
        }

        private void ShowAmount(int amount)
        {
            Amount = amount;
            _amountText.text = amount.ToString();
        }
    }
}
