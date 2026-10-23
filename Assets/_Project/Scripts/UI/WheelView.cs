using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Data;
using VertigoCase.Gameplay;

namespace VertigoCase.UI
{
    // Shows the wheel of the current zone and plays the spin the session already decided.
    public sealed class WheelView : MonoBehaviour
    {
        private const float FullCircle = 360f;
        private const float SliceAngle = FullCircle / WheelConfigSO.SliceCount;

        [SerializeField] private RectTransform _rotator;
        [SerializeField] private Image _baseImage;
        [SerializeField] private Image _indicatorImage;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private Button _spinButton;
        [SerializeField] private WheelSliceView[] _slices;
        [Tooltip("Where the winning slice's icon sits after a spin; won rewards fly from here.")]
        [SerializeField] private RectTransform _rewardOrigin;
        [SerializeField] private Image _winFlashImage;
        [SerializeField] private Image _raysImage;
        [SerializeField] private ContinuousRotation _raysRotation;
        [SerializeField, Min(0.01f)] private float _winFlashDuration = 0.6f;
        [SerializeField, Min(0f)] private float _winFlashStartScale = 0.3f;
        [SerializeField, Min(0f)] private float _winFlashEndScale = 1.4f;
        [SerializeField, Min(0.1f)] private float _spinDuration = 4f;
        [SerializeField, Min(1)] private int _fullTurns = 5;

        private GameSession _session;
        private ZoneProgressionSO _progression;
        private Tween _spinTween;
        private Sequence _winFlashSequence;

        private void OnValidate()
        {
            if (_rotator == null) _rotator = UIFind.Child<RectTransform>(this, "ui_wheel_rotator");
            if (_baseImage == null) _baseImage = UIFind.Child<Image>(this, "ui_image_wheel_base_value");
            if (_indicatorImage == null) _indicatorImage = UIFind.Child<Image>(this, "ui_image_wheel_indicator_value");
            if (_titleText == null) _titleText = UIFind.Child<TMP_Text>(this, "ui_text_wheel_title_value");
            if (_spinButton == null) _spinButton = UIFind.Child<Button>(this, "ui_button_spin");
            if (_slices == null || _slices.Length == 0) _slices = GetComponentsInChildren<WheelSliceView>(true);
            if (_rewardOrigin == null) _rewardOrigin = UIFind.Child<RectTransform>(this, "ui_wheel_reward_origin");
            if (_winFlashImage == null) _winFlashImage = UIFind.Child<Image>(this, "ui_image_wheel_win_flash");
            if (_raysImage == null) _raysImage = UIFind.Child<Image>(this, "ui_image_wheel_rays");
            if (_raysRotation == null && _raysImage != null) _raysRotation = _raysImage.GetComponent<ContinuousRotation>();
        }

        public Transform RewardOrigin => _rewardOrigin;

        public void Init(GameSession session, ZoneProgressionSO progression)
        {
            _session = session;
            _progression = progression;

            _session.ZoneChanged += HandleZoneChanged;
            _session.StateChanged += HandleStateChanged;
            _session.SpinStarted += HandleSpinStarted;

            ShowCurrentWheel();
            HandleStateChanged(_session.State);
        }

        private void OnEnable() => _spinButton.onClick.AddListener(HandleSpinClicked);

        private void OnDisable() => _spinButton.onClick.RemoveListener(HandleSpinClicked);

        private void OnDestroy()
        {
            _spinTween?.Kill();
            _winFlashSequence?.Kill();
            if (_session == null) return;

            _session.ZoneChanged -= HandleZoneChanged;
            _session.StateChanged -= HandleStateChanged;
            _session.SpinStarted -= HandleSpinStarted;
        }

        private void HandleSpinClicked() => _session.Spin();

        // The new zone's wheel is shown once the win flash is over, so the won slot doesn't change under it.
        private void HandleZoneChanged(int zone)
        {
            if (IsWinFlashPlaying) _winFlashSequence.OnComplete(ShowCurrentWheel);
            else ShowCurrentWheel();
        }

        // Input lock: the button is only clickable when the session accepts a spin.
        private void HandleStateChanged(GameState state) => _spinButton.interactable = _session.CanSpin;

        private void HandleSpinStarted(int sliceIndex)
        {
            // Slice i sits at -i * 45 degrees, so rotating the wheel to +i * 45 brings it under the
            // indicator. Subtracting full turns makes the wheel spin clockwise several times first.
            float targetAngle = sliceIndex * SliceAngle - _fullTurns * FullCircle;

            // Spun again during the flash: finish it now so the wheel shows the current zone's slices.
            if (IsWinFlashPlaying) _winFlashSequence.Complete(true);

            _spinTween?.Kill();
            _spinTween = _rotator
                .DOLocalRotate(new Vector3(0f, 0f, targetAngle), _spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuart)
                .OnComplete(() => HandleSpinFinished(sliceIndex));
        }

        private void HandleSpinFinished(int sliceIndex)
        {
            if (!_session.CurrentSlices[sliceIndex].IsBomb) PlayWinFlash();
            _session.CompleteSpin();
        }

        private bool IsWinFlashPlaying => _winFlashSequence != null && _winFlashSequence.IsActive();

        // A soft light bursts out of the winning slot, then fades.
        private void PlayWinFlash()
        {
            RectTransform flash = _winFlashImage.rectTransform;
            flash.localScale = Vector3.one * _winFlashStartScale;
            _winFlashImage.color = Color.white;

            _winFlashSequence?.Kill();
            _winFlashSequence = DOTween.Sequence()
                .Join(flash.DOScale(_winFlashEndScale, _winFlashDuration).SetEase(Ease.OutQuad))
                .Join(_winFlashImage.DOFade(0f, _winFlashDuration).SetEase(Ease.InQuad));
        }

        private void ShowCurrentWheel()
        {
            WheelConfigSO config = _progression.GetWheel(_session.CurrentZoneType);
            _baseImage.sprite = config.BaseSprite;
            _indicatorImage.sprite = config.IndicatorSprite;
            _titleText.text = config.Title;
            _titleText.color = config.TitleColor;
            // Disabling the rotation component stops its tween (OnDisable); static rays sit straight.
            _raysImage.gameObject.SetActive(config.ShowRays);
            _raysImage.color = config.RaysColor;
            _raysRotation.enabled = config.RotateRays;
            if (!config.RotateRays) _raysImage.rectTransform.localRotation = Quaternion.identity;

            for (int i = 0; i < _slices.Length; i++)
            {
                _slices[i].SetContent(config.Slices[i].Reward.Icon, _session.GetSliceAmount(i));
            }
        }
    }
}
