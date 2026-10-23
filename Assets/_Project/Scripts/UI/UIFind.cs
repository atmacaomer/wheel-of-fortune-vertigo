using UnityEngine;

namespace VertigoCase.UI
{
    // Used from OnValidate to wire references by the ui_ naming convention instead of drag and drop.
    public static class UIFind
    {
        public static T Child<T>(Component root, string name) where T : Component
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name) return child.GetComponent<T>();
            }
            return null;
        }
    }
}
