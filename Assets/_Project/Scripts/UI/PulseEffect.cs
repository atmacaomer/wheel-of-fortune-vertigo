using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace VertigoCase.UI
{
    // Grows and fades, then comes back: a slow "heartbeat" when looping, a one-off flash with one loop.
    // Starts in OnEnable, so a popup gets it for free (and replays it) just by being shown.
    [RequireComponent(typeof(Image))]
    public sealed class PulseEffect : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField, Min(0.01f)] private float _halfBeatDuration = 0.6f;
        [SerializeField, Min(1f)] private float _maxScale = 1.06f;
        [SerializeField, Range(0f, 1f)] private float _minAlpha = 0.2f;
        [Tooltip("-1 = forever. 1 = grow and fade out once (a flash).")]
        [SerializeField, Min(-1)] private int _loopCount = -1;

        private Sequence _pulseSequence;

        private void OnValidate()
        {
            if (_image == null) _image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            transform.localScale = Vector3.one;
            Color color = _image.color;
            color.a = 1f;
            _image.color = color;

            _pulseSequence = DOTween.Sequence()
                .Join(transform.DOScale(_maxScale, _halfBeatDuration))
                .Join(_image.DOFade(_minAlpha, _halfBeatDuration))
                .SetEase(Ease.InOutSine)
                .SetLoops(_loopCount, LoopType.Yoyo);
        }

        private void OnDisable() => _pulseSequence?.Kill();
    }
}
