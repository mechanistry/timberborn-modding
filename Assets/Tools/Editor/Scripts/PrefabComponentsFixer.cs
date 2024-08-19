using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Exception = System.Exception;

namespace Timberborn.ModdingTools {
  internal class PrefabComponentsFixer : EditorWindow {

    [MenuItem("Timberborn/Fix Prefab Components")]
    private static void FixPrefabComponents() {
      var gameObject = Selection.activeObject as GameObject;
      if (gameObject) {
        foreach (var component in gameObject.GetComponentsInChildren<Component>(true)) {
          try {
            if (TryGetSpecComponentId(component, out var componentFileId,
                                      out var specComponentFileId)) {
              Debug.Log($"Replacing component {component.GetType()} in prefab {gameObject.name}");
              ReplaceScriptFileIdInPrefab(gameObject, componentFileId, specComponentFileId);
            }
          } catch (Exception e) {
            Debug.LogError($"Error when fixing component {component.name}: " + e.Message);
          }
        }
        Debug.Log("Components fixed");
      } else {
        Debug.LogError("Select a GameObject to fix its components");
      }
    }

    [MenuItem("Timberborn/Fix Prefab Components", true)]
    private static bool FixPrefabComponentsValidate() {
      return Selection.activeObject is GameObject;
    }

    private static bool TryGetSpecComponentId(Component component,
                                              out string componentFileId,
                                              out string specComponentFileId) {
      var type = component.GetType();
      var assembly = type.Assembly;
      var specType = assembly.GetType(type.FullName + "Spec");
      if (specType != null) {
        return TryGetSpecComponentId(assembly, type, specType, out componentFileId,
                                     out specComponentFileId);
      }
      specComponentFileId = componentFileId = null;
      return false;
    }

    private static bool TryGetSpecComponentId(Assembly assembly, Type componentType, Type specType,
                                              out string componentFileId,
                                              out string specComponentFileId) {
      componentFileId = specComponentFileId = null;
      var dllPath = "Assets" + assembly.Location[Application.dataPath.Length..];
      foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(dllPath)) {
        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out _, out var fileId)) {
          if (obj.name == componentType.Name) {
            componentFileId = fileId.ToString();
          } else if (obj.name == specType.Name) {
            specComponentFileId = fileId.ToString();
          }
        }
      }
      return componentFileId != null && specComponentFileId != null;
    }

    private static void ReplaceScriptFileIdInPrefab(GameObject gameObject, string oldFileId,
                                                    string newFileId) {
      var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
      var format = "fileID: {0}";
      var oldField = string.Format(format, oldFileId);
      var newField = string.Format(format, newFileId);
      var newText = File.ReadAllText(prefabPath).Replace(oldField, newField);
      File.WriteAllText(prefabPath, newText);
      AssetDatabase.ImportAsset(prefabPath);
    }

  }
}