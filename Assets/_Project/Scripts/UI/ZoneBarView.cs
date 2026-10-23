using DG.Tweening;
using TMPro;
using UnityEngine;
using VertigoCase.Gameplay;

namespace VertigoCase.UI
{
    // Top strip of zone numbers. A fixed set of items is reused: the strip slides one item to the
    // left, snaps back and relabels, so no items are ever created or destroyed.
    public sealed class ZoneBarView : MonoBehaviour
    {
        private const int FirstZone = 1;

        [SerializeField] private RectTransform _content;
        [SerializeField] private TMP_Text[] _numberTexts;
        [Tooltip("Index of the item under the fixed current-zone frame.")]
        [SerializeField, Min(0)] private int _currentItemIndex = 6;
        [SerializeField, Min(1f)] private float _itemWidth = 80f;
        [SerializeField, Min(0.01f)] private float _slideDuration = 0.35f;
        [SerializeField] private Color _currentColor = new Color(0.1f, 0.1f, 0.1f);
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _safeColor = new Color(0.42f, 0.85f, 0.2f);
        [SerializeField] private Color _superColor = new Color(1f, 0.8f, 0.2f);

        private GameSession _session;
        private ZoneRules _zoneRules;
        private int _shownZone;
        private Tween _slideTween;

        private void OnValidate()
        {
            if (_content == null) _content = UIFind.Child<RectTransform>(this, "ui_zone_bar_content");
            if ((_numberTexts == null || _numberTexts.Length == 0) && _content != null)
                _numberTexts = _content.GetComponentsInChildren<TMP_Text>(true);
        }

        public void Init(GameSession session, ZoneRules zoneRules)
        {
            _session = session;
            _zoneRules = zoneRules;
            _session.ZoneChanged += HandleZoneChanged;
            ShowZone(_session.CurrentZone);
        }

        private void OnDestroy()
        {
            _slideTween?.Kill();
            if (_session != null) _session.ZoneChanged -= HandleZoneChanged;
        }

        private void HandleZoneChanged(int zone)
        {
            _slideTween?.Kill();

            // Only a step forward is animated; a restart to zone 1 jumps straight there.
            if (zone != _shownZone + 1)
            {
                ShowZone(zone);
                return;
            }

            _slideTween = _content
                .DOAnchorPosX(-_itemWidth, _slideDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => ShowZone(zone));
        }

        private void ShowZone(int zone)
        {
            _shownZone = zone;
            _content.anchoredPosition = new Vector2(0f, _content.anchoredPosition.y);

            for (int i = 0; i < _numberTexts.Length; i++)
            {
                int itemZone = zone - _currentItemIndex + i;
                TMP_Text text = _numberTexts[i];

                if (itemZone < 1)
                {
                    text.text = string.Empty;
                    continue;
                }

                text.text = itemZone.ToString();
                text.color = i == _currentItemIndex ? _currentColor : GetZoneColor(itemZone);
            }
        }

        private Color GetZoneColor(int zone)
        {
            // Like the original game, zone 1 is green too: nothing collected yet, so nothing to lose.
            if (zone == FirstZone) return _safeColor;

            switch (_zoneRules.GetZoneType(zone))
            {
                case ZoneType.Super: return _superColor;
                case ZoneType.Safe: return _safeColor;
                default: return _normalColor;
            }
        }
    }
}
