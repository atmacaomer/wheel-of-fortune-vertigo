using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Data;
using VertigoCase.Gameplay;

namespace VertigoCase.UI
{
    // Left panel: one row per collected reward type, plus the exit (cash out) button.
    // A won reward flies from the wheel into its row before the amount is updated.
    public sealed class RewardPanelView : MonoBehaviour
    {
        [SerializeField] private Button _exitButton;
        [SerializeField] private RectTransform _listRoot;
        [SerializeField] private RewardRowView _rowPrefab;
        [Tooltip("Always shown first, in this order. Other rows follow, most recently won on top.")]
        [SerializeField] private RewardDefinitionSO[] _pinnedRewards;
        [Header("Reward fly animation")]
        [Tooltip("Upper limit of flying copies; smaller wins send one copy per unit.")]
        [SerializeField, Min(1)] private int _maxFlyIcons = 5;
        [SerializeField, Min(0.1f)] private float _flyIconScale = 0.95f;
        [SerializeField, Min(0f)] private float _burstRadius = 110f;
        [SerializeField, Min(0.01f)] private float _burstDuration = 0.25f;
        [SerializeField, Min(0.01f)] private float _flyDuration = 0.5f;
        [SerializeField, Min(0f)] private float _flyStagger = 0.07f;

        private readonly Dictionary<string, RewardRowView> _rows = new Dictionary<string, RewardRowView>();
        // Last total per reward, to know how much a single win added.
        private readonly Dictionary<string, int> _totals = new Dictionary<string, int>();
        private readonly List<GameObject> _flyingIcons = new List<GameObject>();
        // Non-pinned reward ids in the order they first appeared, newest first; rows past the bottom are masked out.
        private readonly List<string> _newestFirst = new List<string>();
        private GameSession _session;
        private IReadOnlyDictionary<string, RewardDefinitionSO> _definitions;
        private ConfirmPopupView _exitConfirmPopup;
        private Transform _flyOrigin;
        private Transform _flyLayer;
        private Sequence _flySequence;

        private void OnValidate()
        {
            if (_exitButton == null) _exitButton = UIFind.Child<Button>(this, "ui_button_exit");
            if (_listRoot == null) _listRoot = UIFind.Child<RectTransform>(this, "ui_rewards_list");
        }

        public void Init(
            GameSession session,
            IReadOnlyDictionary<string, RewardDefinitionSO> definitions,
            ConfirmPopupView exitConfirmPopup,
            Transform flyOrigin)
        {
            _session = session;
            _definitions = definitions;
            _exitConfirmPopup = exitConfirmPopup;
            _flyOrigin = flyOrigin;
            // Fly above every panel by using the root canvas as parent.
            _flyLayer = GetComponentInParent<Canvas>().rootCanvas.transform;

            _session.StateChanged += HandleStateChanged;
            _session.Rewards.RewardChanged += HandleRewardChanged;
            _session.Rewards.Cleared += HandleCleared;

            HandleStateChanged(_session.State);
        }

        private void OnEnable() => _exitButton.onClick.AddListener(HandleExitClicked);

        private void OnDisable() => _exitButton.onClick.RemoveListener(HandleExitClicked);

        private void OnDestroy()
        {
            StopFlight();
            if (_session == null) return;

            _session.StateChanged -= HandleStateChanged;
            _session.Rewards.RewardChanged -= HandleRewardChanged;
            _session.Rewards.Cleared -= HandleCleared;
        }

        // Leaving is confirmed first ("the best rewards are saved for last").
        private void HandleExitClicked() => _exitConfirmPopup.Open(_session.Exit);

        // Exit is only offered between spins on safe/super zones.
        private void HandleStateChanged(GameState state) => _exitButton.interactable = _session.CanExit;

        private void HandleRewardChanged(string rewardId, int total)
        {
            // Run rewards only ever grow (revives are paid from the wallet), so every change is a win.
            _totals.TryGetValue(rewardId, out int previousTotal);
            _totals[rewardId] = total;
            int wonAmount = total - previousTotal;

            if (!_rows.TryGetValue(rewardId, out RewardRowView row))
            {
                row = Instantiate(_rowPrefab, _listRoot);
                row.SetIcon(_definitions[rewardId].Icon);
                row.ClearAmount();
                _rows.Add(rewardId, row);
                if (!IsPinned(rewardId)) _newestFirst.Insert(0, rewardId);

                ApplyRowOrder();
                // The fly target must be where the layout puts the new row, not where it spawned.
                LayoutRebuilder.ForceRebuildLayoutImmediate(_listRoot);
            }

            // One copy per unit won, capped: x1 -> 1 copy, x3 -> 3, x100 -> max.
            int iconCount = Mathf.Clamp(wonAmount, 1, _maxFlyIcons);
            FlyToRow(row, total, iconCount);
        }

        // Row order: pinned rewards first (in their configured order), then the others newest first.
        // Existing rows keep their place; a new reward pushes the older ones down (and out of the mask).
        private void ApplyRowOrder()
        {
            int siblingIndex = 0;
            foreach (RewardDefinitionSO pinned in _pinnedRewards)
            {
                if (_rows.TryGetValue(pinned.Id, out RewardRowView pinnedRow))
                    pinnedRow.transform.SetSiblingIndex(siblingIndex++);
            }
            foreach (string id in _newestFirst)
            {
                _rows[id].transform.SetSiblingIndex(siblingIndex++);
            }
        }

        private bool IsPinned(string rewardId)
        {
            foreach (RewardDefinitionSO pinned in _pinnedRewards)
            {
                if (pinned.Id == rewardId) return true;
            }
            return false;
        }

        // Several small copies burst out of the winning slot, then fly one after another into the row.
        // The count starts when the first copy lands; every landing punches the row icon.
        private void FlyToRow(RewardRowView row, int total, int iconCount)
        {
            _flySequence?.Complete(true);

            Vector3 target = row.Icon.transform.position;
            // Burst radius is in canvas units; convert to world units for DOMove.
            float burstRadius = _burstRadius * _flyLayer.lossyScale.x;
            float landTime = _burstDuration + _flyDuration;
            _flySequence = DOTween.Sequence();

            for (int i = 0; i < iconCount; i++)
            {
                Image icon = Instantiate(row.Icon, _flyLayer);
                _flyingIcons.Add(icon.gameObject);
                RectTransform iconTransform = icon.rectTransform;
                iconTransform.position = _flyOrigin.position;
                iconTransform.localScale = Vector3.zero;

                // Cosmetic scatter only, so UnityEngine.Random is fine here (game results use IRandomProvider).
                Vector3 burstPosition = _flyOrigin.position + (Vector3)(Random.insideUnitCircle * burstRadius);
                float flyStart = _burstDuration + i * _flyStagger;
                bool isFirst = i == 0;

                _flySequence
                    .Insert(0f, iconTransform.DOScale(_flyIconScale, _burstDuration).SetEase(Ease.OutBack))
                    .Insert(0f, iconTransform.DOMove(burstPosition, _burstDuration).SetEase(Ease.OutQuad))
                    .Insert(flyStart, iconTransform.DOMove(target, _flyDuration).SetEase(Ease.InQuad))
                    .InsertCallback(flyStart + _flyDuration, () =>
                    {
                        icon.gameObject.SetActive(false);
                        if (isFirst) row.CountTo(total);
                        row.Punch();
                    });
            }

            _flySequence.InsertCallback(landTime + (iconCount - 1) * _flyStagger, StopFlight);
        }

        private void StopFlight()
        {
            _flySequence?.Kill();
            _flySequence = null;
            foreach (GameObject icon in _flyingIcons)
            {
                if (icon != null) Destroy(icon);
            }
            _flyingIcons.Clear();
        }

        private void HandleCleared()
        {
            StopFlight();
            foreach (RewardRowView row in _rows.Values)
            {
                Destroy(row.gameObject);
            }
            _rows.Clear();
            _totals.Clear();
            _newestFirst.Clear();
        }
    }
}
