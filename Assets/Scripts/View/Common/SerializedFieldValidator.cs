using UnityEngine;

namespace View
{
    public static class SerializedFieldValidator
    {
        public static void RequireAssigned(this Component owner, Object field, string fieldName)
        {
            if (field == null)
                Debug.LogWarning($"[{owner.GetType().Name}] '{fieldName}' is not assigned.", owner);
        }
    }
}
