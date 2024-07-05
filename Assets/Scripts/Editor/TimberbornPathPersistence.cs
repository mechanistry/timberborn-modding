using UnityEditor;

namespace ModBuilding.Editor {
  internal class TimberbornPathPersistence {

    private static readonly string TimberbornPathKey = "TimberbornPath";

    public void SavePath(string path) {
      if (!string.IsNullOrEmpty(path)) {
        EditorPrefs.SetString(TimberbornPathKey, path);
      }
    }

    public bool TryGetPath(out string path) {
      path = EditorPrefs.GetString(TimberbornPathKey);
      return !string.IsNullOrEmpty(path);
    }

  }
}