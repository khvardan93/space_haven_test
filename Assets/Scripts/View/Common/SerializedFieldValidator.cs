using UnityEngine;

namespace View
{
    /// <summary>Shared OnValidate helper so views can warn about missing inspector references instead of null-checking them at runtime.</summary>
    public static class SerializedFieldValidator
    {
        public static void RequireAssigned(this Component owner, Object field, string fieldName)
        {
            if (field == null)
                Debug.LogWarning($"[{owner.GetType().Name}] '{fieldName}' is not assigned.", owner);
        }
    }
}
