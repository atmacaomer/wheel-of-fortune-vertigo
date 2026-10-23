using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Data;
using VertigoCase.Gameplay;

namespace VertigoCase.UI
{
    // "SAFE ZONE 10" / "SUPER ZONE 30" boxes: the next zone of each type still ahead,
    // plus the special reward waiting on that super zone.
    public sealed class ZoneInfoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _safeZoneText;
        [SerializeField] private TMP_Text _superZoneText;
        [SerializeField] private Image _specialRewardIcon;

        private GameSession _session;
        private ZoneRules _zoneRules;
        private IReadOnlyDictionary<string, RewardDefinitionSO> _definitions;

        private void OnValidate()
        {
            if (_safeZoneText == null) _safeZoneText = UIFind.Child<TMP_Text>(this, "ui_text_zone_info_safe_value");
            if (_superZoneText == null) _superZoneText = UIFind.Child<TMP_Text>(this, "ui_text_zone_info_super_value");
            if (_specialRewardIcon == null) _specialRewardIcon = UIFind.Child<Image>(this, "ui_image_zone_info_super_reward_value");
        }

        public void Init(GameSession session, ZoneRules zoneRules, IReadOnlyDictionary<string, RewardDefinitionSO> definitions)
        {
            _session = session;
            _zoneRules = zoneRules;
            _definitions = definitions;
            _session.ZoneChanged += HandleZoneChanged;
            _session.UpcomingSpecialRewardChanged += HandleUpcomingSpecialRewardChanged;
            HandleZoneChanged(_session.CurrentZone);
            HandleUpcomingSpecialRewardChanged(_session.UpcomingSpecialRewardId);
        }

        private void OnDestroy()
        {
            if (_session == null) return;

            _session.ZoneChanged -= HandleZoneChanged;
            _session.UpcomingSpecialRewardChanged -= HandleUpcomingSpecialRewardChanged;
        }

        // Counted from the next zone, so standing on zone 30 already shows 60 (whose reward was just picked).
        private void HandleZoneChanged(int zone)
        {
            _safeZoneText.text = _zoneRules.GetUpcomingZone(zone + 1, ZoneType.Safe).ToString();
            _superZoneText.text = _zoneRules.GetUpcomingZone(zone + 1, ZoneType.Super).ToString();
        }

        private void HandleUpcomingSpecialRewardChanged(string rewardId) =>
            _specialRewardIcon.sprite = _definitions[rewardId].Icon;
    }
}
