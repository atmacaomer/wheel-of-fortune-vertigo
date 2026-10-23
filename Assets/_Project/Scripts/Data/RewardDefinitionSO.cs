using UnityEngine;

namespace VertigoCase.Data
{
    // One kind of reward (gold, cash, a weapon...) or the bomb. Shared by every wheel that uses it.
    [CreateAssetMenu(fileName = "Reward_", menuName = "VertigoCase/Reward Definition")]
    public sealed class RewardDefinitionSO : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private bool _isBomb;

        public string Id => _id;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public bool IsBomb => _isBomb;

        // Default the id to the asset name so a new reward works without typing it.
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_id)) _id = name;
        }
    }
}
