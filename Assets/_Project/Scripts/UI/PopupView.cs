using DG.Tweening;
using UnityEngine;

namespace VertigoCase.UI
{
    // Shared show/hide animation for full-screen popups (fade the dim, pop the content in).
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PopupView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _content;
        [SerializeField, Min(0.01f)] private float _showDuration = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _startScale = 0.8f;

        private Sequence _showSequence;

        protected virtual void OnValidate()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            // Convention: the animated child of "ui_popup_x" is "ui_popup_x_content".
            if (_content == null) _content = UIFind.Child<RectTransform>(this, name + "_content");
        }

        protected virtual void OnDisable() => _showSequence?.Kill();

        protected void Show()
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 0f;
            _content.localScale = Vector3.one * _startScale;

            _showSequence?.Kill();
            _showSequence = DOTween.Sequence()
                .Join(_canvasGroup.DOFade(1f, _showDuration))
                .Join(_content.DOScale(1f, _showDuration).SetEase(Ease.OutBack));
        }

        protected void Hide() => gameObject.SetActive(false);
    }
}
