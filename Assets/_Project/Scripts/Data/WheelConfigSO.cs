using System;
using System.Collections.Generic;
using UnityEngine;
using VertigoCase.Gameplay;

namespace VertigoCase.Data
{
    // Look and slice contents of one wheel type (bronze / silver / golden), editable in the Inspector.
    [CreateAssetMenu(fileName = "Wheel_", menuName = "VertigoCase/Wheel Config")]
    public sealed class WheelConfigSO : ScriptableObject
    {
        // The wheel prefab has 8 slice slots.
        public const int SliceCount = 8;

        [Serializable]
        public sealed class SliceData
        {
            [SerializeField] private RewardDefinitionSO _reward;
            [SerializeField, Min(0)] private int _baseAmount = 1;
            [SerializeField, Min(0)] private int _weight = 1;

            public RewardDefinitionSO Reward => _reward;
            public int BaseAmount => _baseAmount;
            public int Weight => _weight;
        }

        [SerializeField] private string _title = "BRONZE SPIN";
        [SerializeField] private Color _titleColor = Color.white;
        [SerializeField] private Sprite _baseSprite;
        [SerializeField] private Sprite _indicatorSprite;
        [Tooltip("Rotating light rays behind the wheel, e.g. only on safe and super wheels.")]
        [SerializeField] private bool _showRays;
        [SerializeField] private bool _rotateRays;
        [SerializeField] private Color _raysColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] private List<SliceData> _slices = new List<SliceData>();

        public string Title => _title;
        public Color TitleColor => _titleColor;
        public Sprite BaseSprite => _baseSprite;
        public Sprite IndicatorSprite => _indicatorSprite;
        public bool ShowRays => _showRays;
        public bool RotateRays => _rotateRays;
        public Color RaysColor => _raysColor;
        public IReadOnlyList<SliceData> Slices => _slices;

        // Plain data for the rules; the gameplay assembly never sees ScriptableObjects or sprites.
        public IReadOnlyList<WheelSlice> CreateSlices()
        {
            // The wheel art has 8 fixed slots; any other count would make the pointer and the payout disagree.
            if (_slices.Count != SliceCount)
                throw new InvalidOperationException($"{name}: wheel needs exactly {SliceCount} slices, has {_slices.Count}.");

            var result = new WheelSlice[_slices.Count];
            for (int i = 0; i < _slices.Count; i++)
            {
                SliceData data = _slices[i];
                result[i] = new WheelSlice(data.Reward.Id, data.BaseAmount, data.Weight, data.Reward.IsBomb);
            }
            return result;
        }

        private void OnValidate()
        {
            if (_slices.Count != SliceCount)
                Debug.LogWarning($"{name}: wheel needs exactly {SliceCount} slices, has {_slices.Count}.", this);
        }
    }
}
