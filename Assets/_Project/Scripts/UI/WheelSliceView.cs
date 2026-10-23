using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VertigoCase.UI
{
    // One of the 8 slices on the wheel: reward icon + amount, both kept inside the wheel hole.
    public sealed class WheelSliceView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _amountText;
        [Tooltip("A slice without an amount (the bomb) has room to spare: its icon is centered in the hole and bigger.")]
        [SerializeField, Min(0.1f)] private float _iconOnlyScale = 1.6f;
        [SerializeField] private float _iconOnlyYOffset = -10f;

        private Vector2 _iconPosition;

        private void OnValidate()
        {
            if (_iconImage == null) _iconImage = UIFind.Child<Image>(this, "ui_image_slice_icon_value");
            if (_amountText == null) _amountText = UIFind.Child<TMP_Text>(this, "ui_text_slice_amount_value");
        }

        private void Awake() => _iconPosition = _iconImage.rectTransform.anchoredPosition;

        // Amount 0 (the bomb) shows no text.
        public void SetContent(Sprite icon, int amount)
        {
            bool hasAmount = amount > 0;
            _iconImage.sprite = icon;
            _amountText.text = hasAmount ? "x" + amount : string.Empty;

            RectTransform iconTransform = _iconImage.rectTransform;
            iconTransform.anchoredPosition = hasAmount ? _iconPosition : _iconPosition + new Vector2(0f, _iconOnlyYOffset);
            iconTransform.localScale = Vector3.one * (hasAmount ? 1f : _iconOnlyScale);
        }
    }
}
