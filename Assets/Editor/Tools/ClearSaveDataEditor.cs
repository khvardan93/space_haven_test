using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor.Tools
{
    public static class ClearSaveDataEditor
    {
        [MenuItem("Tools/Save Data/Delete Saved Game")]
        private static void DeleteSavedGame()
        {
            var dir = Application.persistentDataPath;
            if (!Directory.Exists(dir))
            {
                Debug.Log("[Save] Nothing to delete, persistentDataPath does not exist.");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Delete Saved Game",
                    $"This will delete all files in:\n{dir}\n\nThis cannot be undone. Continue?",
                    "Delete", "Cancel"))
                return;

            foreach (var file in Directory.GetFiles(dir))
                File.Delete(file);

            Debug.Log($"[Save] Deleted saved data in {dir}");
        }

        [MenuItem("Tools/Save Data/Open Save Folder")]
        private static void OpenSaveFolder()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
    }
}
