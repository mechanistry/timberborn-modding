using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Timberborn.Buildings;
using Timberborn.EntitySystem;
using Timberborn.PrefabSystem;
using Timberborn.Timbermesh;
using UnityEditor;
using UnityEngine;

namespace Timberborn.ModdingTools {
  internal static class PrefabToBlueprintConverter {

    private static readonly string AssetPathPrefix = "assets/";
    private static readonly string ResourcesDirectory = "resources/";
    private static readonly string ModsDirectory = "Assets/Mods";

    [MenuItem("Timberborn/Export Prefab to Blueprint")]
    private static void ExportPrefabToBlueprint() {
      var gameObject = Selection.activeObject as GameObject;
      if (gameObject) {
        ConvertPrefab(gameObject);
        AssetDatabase.Refresh();
      }
    }

    [MenuItem("Timberborn/Export Prefab to Blueprint", true)]
    private static bool ExportPrefabToBlueprintValidate() {
      return Selection.activeObject is GameObject;
    }

    [MenuItem("Timberborn/Export all mods Prefabs to Blueprints")]
    private static void ExportPrefabsToBlueprints() {
      var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { ModsDirectory });
      foreach (var guid in prefabGuids) {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        var gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        ConvertPrefab(gameObject);
        Debug.Log($"Converted prefab {gameObject.name} to blueprint at {path}");
      }
      AssetDatabase.Refresh();
    }

    private static void ConvertPrefab(GameObject gameObject) {
      if (IsSerializablePrefab(gameObject)) {
        var json = new JObject();
        var prefabPath = AssetDatabase.GetAssetPath(gameObject);
        var prefab = PrefabUtility.LoadPrefabContents(prefabPath);
        try {
          AddGameObject(prefab, json, false);
          var jsonPath = prefabPath.Replace(".prefab", ".blueprint.json");
          File.WriteAllText(jsonPath, json.ToString(Formatting.Indented));
        } catch (Exception e) {
          Debug.LogError($"Error when exporting prefab {gameObject.name}: {e.Message}");
        } finally {
          PrefabUtility.UnloadPrefabContents(prefab);
        }
      }
    }

    private static bool IsSerializablePrefab(GameObject gameObject) {
      return gameObject.GetComponent<ParticleSystem>() == null
             && (gameObject.GetComponents<Component>().Length > 1
                 || gameObject.GetComponentInChildren<ParticleSystem>() == null)
             && gameObject.GetComponent<Light>() == null
             && gameObject.GetComponent<Camera>() == null;
    }

    private static void AddGameObject(GameObject gameObject, JObject parent, bool isNested) {
      AddComponents(gameObject, parent);
      if (gameObject.transform.childCount > 0) {
        var children = new JObject();
        foreach (Transform child in gameObject.transform) {
          var childObject = new JObject();
          var name = child.name;
          var prefabStatus = PrefabUtility.GetPrefabInstanceStatus(child.gameObject);
          var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
          if (prefabStatus == PrefabInstanceStatus.Connected && prefabAsset != null && !isNested) {
            AddNestedBlueprint(prefabAsset, child, childObject);
            name += "#nested";
          } else {
            AddGameObject(child.gameObject, childObject, isNested);
          }
          children.Add(name, childObject);
        }
        parent.Add("Children", children);
      }
    }

    private static void AddComponents(GameObject gameObject, JObject parent) {
      var colliders = new CollidersConverter();
      foreach (var component in gameObject.GetComponents<Component>()) {
        try {
          if (component is Collider collider) {
            colliders.Add(collider);
          } else if (component is Transform transform) {
            if (transform.localPosition != Vector3.zero
                || transform.localRotation != Quaternion.identity
                || transform.localScale != Vector3.one) {
              parent.Add("TransformSpec", ConvertTransformToJToken(transform));
            }
          } else {
            var json = JsonUtility.ToJson(component);
            var componentName = component.GetType().Name;
            if (componentName == nameof(TimbermeshDescription)) {
              json = json.Replace(@"\\", "/");
            }
            var componentCacheIndex = json.IndexOf("\"_componentCache\":",
                                                   StringComparison.Ordinal);
            if (componentCacheIndex != -1) {
              var nextCommaIndex = json.IndexOf(',', componentCacheIndex);
              var nextBraceIndex = json.IndexOf('}', componentCacheIndex);
              var endIndex = nextCommaIndex != -1
                  ? nextCommaIndex
                  : nextBraceIndex != -1
                      ? nextBraceIndex
                      : json.Length;
              json = json.Remove(componentCacheIndex,
                                 endIndex - componentCacheIndex + 1);
            }

            var jObject = ConvertObjectKeysToPropertyFormat(json, component.GetType());
            componentName = ReplaceOldNames(componentName, jObject);
            parent.Add(componentName, jObject);
          }
        } catch (Exception e) {
          var componentName = component != null ? component.GetType().Name : "null";
          Debug.LogError($"Error when exporting component {componentName} "
                         + $"({gameObject.transform.root.name}): {e.Message}");
        }
      }
      if (colliders.HasColliders()) {
        parent.Add("CollidersSpec", colliders.ToJToken());
      }
    }

    private static string ReplaceOldNames(string componentName, JObject componentObject) {
      if (componentName == nameof(PrefabSpec)) {
        componentName = "TemplateSpec";
        ReplacePropertyName(componentObject, "PrefabName", "TemplateName");
        ReplacePropertyName(componentObject, "BackwardCompatiblePrefabNames",
                            "BackwardCompatibleTemplateNames");
      } else if (componentName == nameof(TimbermeshDescription)) {
        componentName = "TimbermeshSpec";
        ReplacePropertyName(componentObject, "ModelName", "Model");
      } else if (componentName == nameof(LabeledEntitySpec)) {
        ReplacePropertyName(componentObject, "ImagePath", "Icon");
      } else if (componentName == nameof(BuildingSpec)) {
        var buildingCost = componentObject.Property("BuildingCost");
        var buildingCostArray = (JArray) buildingCost?.Value;
        if (buildingCostArray != null) {
          for (var i = buildingCostArray.Count - 1; i >= 0; i--) {
            var item = buildingCostArray[i];
            ReplacePropertyName((JObject) item, "GoodId", "Id");
          }
        }
      }
      return componentName;
    }

    private static void ReplacePropertyName(JObject componentObject, string propertyName,
                                            string newPropertyName) {
      var property = componentObject.Property(propertyName);
      if (property != null) {
        property.AddBeforeSelf(new JProperty(newPropertyName, property.Value));
        property.Remove();
      }
    }

    private static void AddNestedBlueprint(GameObject prefabAsset, Transform child,
                                           JObject childObject) {
      var path = AssetDatabase.GetAssetPath(prefabAsset);
      var originalJson = new JObject();
      var originalPrefab = PrefabUtility.LoadPrefabContents(path);
      AddGameObject(originalPrefab, originalJson, false);
      var nestedJson = new JObject();
      AddGameObject(child.gameObject, nestedJson, true);
      var diff = JsonDiffer.GenerateDiffJson(originalJson.ToString(), nestedJson.ToString());

      var relativePath = NormalizeAssetPath(path) + ".blueprint";
      childObject.Add(new JProperty("BlueprintPath", relativePath));
      if (!string.IsNullOrEmpty(diff)) {
        var rootPath = child.name;
        var tempChild = child;
        while (tempChild.parent != null) {
          rootPath = $"{tempChild.parent.name}/{rootPath}";
          tempChild = tempChild.parent;
        }
        childObject.Add(new JProperty("Modification", JObject.Parse(diff)));
      }
      PrefabUtility.UnloadPrefabContents(originalPrefab);
    }

    private static string NormalizeAssetPath(string assetPath) {
      var rootIndex =
          assetPath.LastIndexOf(ResourcesDirectory, StringComparison.OrdinalIgnoreCase);
      if (rootIndex != -1) {
        assetPath = assetPath[(rootIndex + ResourcesDirectory.Length)..];
      } else if (assetPath.StartsWith(AssetPathPrefix, StringComparison.OrdinalIgnoreCase)) {
        assetPath = assetPath[AssetPathPrefix.Length..];
      }
      var extension = Path.GetExtension(assetPath);
      assetPath = extension != null ? assetPath[..^extension.Length] : assetPath;
      assetPath = assetPath.Replace('\\', '/');
      if (assetPath.StartsWith("/")) {
        assetPath = assetPath[1..];
      }
      return assetPath;
    }

    private static JToken ConvertTransformToJToken(Transform transform) {
      return new JObject {
          {
              "Position", new JObject {
                  { "X", transform.localPosition.x },
                  { "Y", transform.localPosition.y },
                  { "Z", transform.localPosition.z }
              }
          }, {
              "Rotation", new JObject {
                  { "X", transform.localRotation.eulerAngles.x },
                  { "Y", transform.localRotation.eulerAngles.y },
                  { "Z", transform.localRotation.eulerAngles.z }
              }
          }, {
              "Scale", new JObject {
                  { "X", transform.localScale.x },
                  { "Y", transform.localScale.y },
                  { "Z", transform.localScale.z }
              }
          }
      };
    }

    private static JObject ConvertObjectKeysToPropertyFormat(string json, Type objectType) {
      var jObject = JObject.Parse(json);
      foreach (var property in jObject.Properties().ToList()) {
        var fieldType = objectType.GetField(property.Name,
                                            BindingFlags.Instance
                                            | BindingFlags.Public
                                            | BindingFlags.NonPublic)?.FieldType;
        if (fieldType == typeof(AnimationCurve)) {
          property.Value = ConvertAnimationCurve((JObject) property.Value);
        } else if (property.Value.Type is JTokenType.Object) {
          property.Value = ConvertObjectKeysToPropertyFormat(property.Value.ToString(),
                                                             fieldType);
        } else if (property.Value.Type is JTokenType.Array) {
          property.Value = ConvertArrayKeysToPropertyFormat(property.Value.ToString(),
                                                            fieldType);
        } else if (TryConvertEnumPropertyToString(property, objectType, out var enumValue)) {
          property.Value = enumValue;
        }
        property.AddBeforeSelf(NormalizePropertyName(property));
        property.Remove();
      }
      return jObject;
    }

    private static JToken ConvertAnimationCurve(JObject animationCurve) {
      var keys = (JArray) animationCurve["m_Curve"];
      var array = new JArray();
      foreach (var keyframe in keys) {
        array.Add(ConvertAnimationKeyframe((JObject) keyframe));
      }
      animationCurve["Keys"] = array;
      animationCurve.Remove("m_Curve");
      animationCurve["PreWrapMode"] = GetWrapModeName(animationCurve["m_PreInfinity"].Value<int>());
      animationCurve.Remove("m_PreInfinity");
      animationCurve["PostWrapMode"] =
          GetWrapModeName(animationCurve["m_PostInfinity"].Value<int>());
      animationCurve.Remove("m_PostInfinity");
      animationCurve.Remove("serializedVersion");
      animationCurve.Remove("m_RotationOrder");

      return animationCurve;
    }

    private static JToken ConvertAnimationKeyframe(JObject animationKeyframe) {
      animationKeyframe["Time"] = animationKeyframe["time"];
      animationKeyframe.Remove("time");
      animationKeyframe["Value"] = animationKeyframe["value"];
      animationKeyframe.Remove("value");
      animationKeyframe["InTangent"] = animationKeyframe["inSlope"];
      animationKeyframe.Remove("inSlope");
      animationKeyframe["OutTangent"] = animationKeyframe["outSlope"];
      animationKeyframe.Remove("outSlope");
      animationKeyframe["WeightedMode"] = animationKeyframe["weightedMode"];
      animationKeyframe.Remove("weightedMode");
      animationKeyframe["InWeight"] = animationKeyframe["inWeight"];
      animationKeyframe.Remove("inWeight");
      animationKeyframe["OutWeight"] = animationKeyframe["outWeight"];
      animationKeyframe.Remove("outWeight");
      animationKeyframe.Remove("tangentMode");
      animationKeyframe.Remove("serializedVersion");
      return animationKeyframe;
    }

    private static string GetWrapModeName(int wrapMode) {
      return wrapMode switch {
          0 => nameof(WrapMode.PingPong),
          1 => nameof(WrapMode.Loop),
          2 => nameof(WrapMode.ClampForever),
          _ => throw new ArgumentOutOfRangeException(nameof(wrapMode), wrapMode, null)
      };
    }

    private static JArray ConvertArrayKeysToPropertyFormat(string json, Type objectType) {
      var jArray = JArray.Parse(json);
      var genericType = objectType.IsArray
          ? objectType.GetElementType()
          : objectType.GetGenericArguments().Single();
      for (var index = 0; index < jArray.Count; index++) {
        var item = jArray[index];
        if (item.Type is JTokenType.Object) {
          jArray.Insert(index, ConvertObjectKeysToPropertyFormat(item.ToString(), genericType));
          jArray.RemoveAt(index + 1);
        } else if (item.Type is JTokenType.Array) {
          jArray.Insert(index, ConvertArrayKeysToPropertyFormat(item.ToString(), genericType));
          jArray.RemoveAt(index + 1);
        }
      }
      return jArray;
    }

    private static bool TryConvertEnumPropertyToString(JProperty property, Type ownerType,
                                                       out string enumValue) {
      if (property.Value.Type == JTokenType.Integer) {
        var enumType = ownerType.GetField(property.Name,
                                          BindingFlags.Instance
                                          | BindingFlags.Public
                                          | BindingFlags.NonPublic)?.FieldType;
        if (enumType is { IsEnum: true }) {
          enumValue = Enum.ToObject(enumType, property.Value.ToObject<int>()).ToString();
          return true;
        }
      }
      enumValue = null;
      return false;
    }

    private static JProperty NormalizePropertyName(JProperty property) {
      var key = property.Name;
      key = key.StartsWith('_') ? key[1..] : key;
      key = char.ToUpperInvariant(key[0]) + key[1..];
      return new(key, property.Value);
    }

    private static class JsonDiffer {

      private static readonly string Append = "#append";
      private static readonly string Remove = "#remove";
      private static readonly string Delete = "#delete";

      public static string GenerateDiffJson(string originalJson, string modifiedJson) {
        var original = JsonConvert.DeserializeObject<JToken>(originalJson);
        var modified = JsonConvert.DeserializeObject<JToken>(modifiedJson);

        var diff = DiffObject(original as JObject, modified as JObject);

        return diff == null ? string.Empty : JsonConvert.SerializeObject(diff, Formatting.Indented);
      }

      private static JObject DiffObject(JObject original, JObject modified) {
        var result = new JObject();

        foreach (var prop in modified.Properties()) {
          var originalProp = original.Property(prop.Name);
          if (originalProp == null) {
            result[prop.Name] = prop.Value;
          } else {
            var diff = DiffToken(originalProp.Value, prop.Value);
            if (diff != null) {
              if (originalProp.Value.Type == JTokenType.Array) {
                foreach (var diffProperty in ((JObject) diff).Properties()) {
                  result[$"{prop.Name}{diffProperty.Name}"] = diffProperty.Value;
                }
              } else {
                result[prop.Name] = diff;
              }
            }
          }
        }

        foreach (var prop in original.Properties()) {
          if (modified.Property(prop.Name) == null) {
            result[$"{prop.Name}{Delete}"] = new JObject();
          }
        }

        return result.HasValues ? result : null;
      }

      private static JToken DiffToken(JToken original, JToken modified) {
        if (original.Type != modified.Type) {
          return modified;
        }

        switch (original.Type) {
          case JTokenType.Object:
            return DiffObject(original as JObject, modified as JObject);
          case JTokenType.Array:
            return DiffArray(original as JArray, modified as JArray);
          default:
            return JToken.DeepEquals(original, modified) ? null : modified;
        }
      }

      private static JObject DiffArray(JArray original, JArray modified) {
        var originalSet = new HashSet<string>(original.Select(x => x.ToString(Formatting.None)));
        var modifiedSet = new HashSet<string>(modified.Select(x => x.ToString(Formatting.None)));

        var removed = originalSet.Except(modifiedSet).ToList();
        var added = modifiedSet.Except(originalSet).ToList();

        var diff = new JObject();

        if (removed.Any()) {
          diff[Remove] = new JArray(removed.Select(JToken.Parse));
        }

        if (added.Any()) {
          diff[Append] = new JArray(added.Select(JToken.Parse));
        }

        return diff.HasValues ? diff : null;
      }

    }

    private class CollidersConverter {

      private readonly List<BoxCollider> _boxColliders = new();
      private readonly List<SphereCollider> _sphereColliders = new();
      private readonly List<CapsuleCollider> _capsuleColliders = new();

      public void Add(Collider collider) {
        if (collider is BoxCollider boxCollider) {
          _boxColliders.Add(boxCollider);
        } else if (collider is SphereCollider sphereCollider) {
          _sphereColliders.Add(sphereCollider);
        } else if (collider is CapsuleCollider capsuleCollider) {
          _capsuleColliders.Add(capsuleCollider);
        } else {
          Debug.LogWarning("Unknown collider: " + collider.GetType());
        }
      }

      public bool HasColliders() {
        return _boxColliders.Count > 0 || _sphereColliders.Count > 0 || _capsuleColliders.Count > 0;
      }

      public JToken ToJToken() {
        return new JObject {
            {
                "BoxColliders",
                new JArray(_boxColliders.Select(ConvertBoxColliderToJToken))
            }, {
                "SphereColliders",
                new JArray(_sphereColliders.Select(ConvertSphereColliderToJToken))
            }, {
                "CapsuleColliders",
                new JArray(_capsuleColliders.Select(ConvertCapsuleColliderToJToken))
            }
        };
      }

      private static JToken ConvertBoxColliderToJToken(BoxCollider collider) {
        return new JObject {
            {
                "Center",
                new JObject {
                    { "X", collider.center.x }, { "Y", collider.center.y },
                    { "Z", collider.center.z }
                }
            }, {
                "Size",
                new JObject {
                    { "X", collider.size.x }, { "Y", collider.size.y }, { "Z", collider.size.z }
                }
            }
        };
      }

      private static JToken ConvertSphereColliderToJToken(SphereCollider collider) {
        return new JObject {
            {
                "Center", new JObject {
                    { "X", collider.center.x }, { "Y", collider.center.y },
                    { "Z", collider.center.z }
                }
            }, {
                "Radius", collider.radius
            }
        };
      }

      private static JToken ConvertCapsuleColliderToJToken(CapsuleCollider collider) {
        return new JObject {
            {
                "Center", new JObject {
                    { "X", collider.center.x }, { "Y", collider.center.y },
                    { "Z", collider.center.z }
                }
            }, {
                "Radius",
                collider.radius
            }, {
                "Height",
                collider.height
            }, {
                "Direction",
                collider.direction switch {
                    0 => "X",
                    1 => "Y",
                    2 => "Z",
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(collider.direction), "Invalid capsule direction")
                }
            }
        };
      }

    }

  }
}