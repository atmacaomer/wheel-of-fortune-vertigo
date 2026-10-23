using DG.Tweening;
using UnityEngine;

namespace VertigoCase.UI
{
    // Endless slow spin for decorative effects, e.g. the light rays behind the wheel.
    public sealed class ContinuousRotation : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float _secondsPerTurn = 20f;

        private Tween _rotationTween;

        private void OnEnable()
        {
            _rotationTween = transform
                .DOLocalRotate(new Vector3(0f, 0f, -360f), _secondsPerTurn, RotateMode.FastBeyond360)
                .SetRelative(true)
                .SetEase(Ease.Linear)
                .SetLoops(-1);
        }

        private void OnDisable() => _rotationTween?.Kill();
    }
}
