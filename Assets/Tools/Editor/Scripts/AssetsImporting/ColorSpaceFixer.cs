using UnityEditor;
using UnityEngine;

namespace Timberborn.ModdingTools.AssetsImporting {
  [InitializeOnLoad]
  internal class ColorSpaceFixer {

    private static readonly string ColorSpaceCheckedKey = "Timberborn.ColorSpaceChecked";
    private static readonly string DontAskAgainKey = "Timberborn.DontAskAboutColorSpace2";

    static ColorSpaceFixer() {
      if (PlayerSettings.colorSpace == ColorSpace.Linear
          && !EditorPrefs.GetBool(DontAskAgainKey, false)
          && !SessionState.GetBool(ColorSpaceCheckedKey, false)) {
        SessionState.SetBool(ColorSpaceCheckedKey, true);
        switch (ShowDialog()) {
          case 0:
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            AssetDatabase.SaveAssets();
            break;
          case 1:
            break;
          case 2:
            EditorPrefs.SetBool(DontAskAgainKey, true);
            break;
        }
      }
    }

    private static int ShowDialog() {
      return EditorUtility.DisplayDialogComplex(
          "Color Space Setting",
          "This project is currently using Linear Color Space, "
          + "which isn't compatible with Timberborn.\nWould you like to switch to Gamma?",
          "Yes, switch to Gamma",
          "No",
          "Don't ask again"
      );
    }

  }
}