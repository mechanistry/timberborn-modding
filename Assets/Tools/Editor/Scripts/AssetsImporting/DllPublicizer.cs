using Mono.Cecil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Timberborn.ModdingTools.AssetsImporting {
  internal class DllPublicizer {

    private static readonly string SerializedFieldAttribute = typeof(SerializeField).FullName;
    private static readonly string CompilerGeneratedAttribute =
        typeof(CompilerGeneratedAttribute).FullName;

    public static void PublicizeDLL(string path, DirectoryInfo dllsDirectory) {
      var tempPath = path + ".tmp";
      try {
        Publicize(path, tempPath, dllsDirectory);
        File.Copy(tempPath, path, true);
      } catch (Exception e) {
        Debug.LogWarning($"Failed to publicize {path}: {e}");
      } finally {
        File.Delete(tempPath);
      }
    }

    private static void Publicize(string source, string destination, DirectoryInfo dllsDirectory) {
      var defaultResolver = new DefaultAssemblyResolver();
      defaultResolver.AddSearchDirectory(dllsDirectory.FullName);
      using var assembly = AssemblyDefinition.ReadAssembly(source, new() {
          AssemblyResolver = defaultResolver
      });
      var types = assembly.MainModule.Types;
      Publicize(types, assembly.MainModule);
      assembly.Write(destination);
    }

    private static void Publicize(Collection<TypeDefinition> types, ModuleDefinition module) {
      var hideInInspectorConstructor = new Lazy<MethodReference>(
          () => module.ImportReference(typeof(HideInInspector).GetConstructor(Type.EmptyTypes)));
      var objectTypeDefinition = module.ImportReference(typeof(Object)).Resolve();

      foreach (var type in types) {
        if (IsNotCompilerGenerated(type)) {
          if (type.IsNested) {
            type.IsNestedPublic = true;
          } else {
            type.IsPublic = true;
          }
          foreach (var method in type.Methods) {
            if (IsNotCompilerGenerated(method)) {
              method.IsPublic = true;
            }
          }
          var isUnityObject = IsSubclassOf(type, objectTypeDefinition);
          foreach (var field in type.Fields) {
            if (IsNotCompilerGenerated(field)) {
              if (isUnityObject && HideInInspector(field)) {
                field.CustomAttributes.Add(new(hideInInspectorConstructor.Value));
              }
              field.IsPublic = true;
            }
          }

          Publicize(type.NestedTypes, module);
        }
      }
    }

    private static bool IsNotCompilerGenerated(ICustomAttributeProvider customAttributeProvider) {
      return customAttributeProvider.CustomAttributes.All(a => a.AttributeType.FullName
                                                               != CompilerGeneratedAttribute);
    }

    private static bool IsSubclassOf(TypeDefinition childTypeDefinition,
                                     TypeDefinition parentTypeDefinition) {
      return childTypeDefinition.MetadataToken != parentTypeDefinition.MetadataToken
             && EnumerateBaseClasses(childTypeDefinition)
                 .Any(b => b.MetadataToken == parentTypeDefinition.MetadataToken);
    }

    private static IEnumerable<TypeDefinition> EnumerateBaseClasses(TypeDefinition typeDefinition) {
      while (typeDefinition.BaseType != null) {
        typeDefinition = typeDefinition.BaseType.Resolve();
        yield return typeDefinition;
      }
    }

    private static bool HideInInspector(FieldDefinition field) {
      return field.IsPrivate
             && field.CustomAttributes.All(a => a.AttributeType.FullName
                                                != SerializedFieldAttribute);
    }

  }
}