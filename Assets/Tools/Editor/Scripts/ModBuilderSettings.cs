namespace Timberborn.ModdingTools {
  public readonly struct ModBuilderSettings {

    public bool BuildCode { get; }
    public bool BuildWindowsAssetBundle { get; }
    public bool BuildMacAssetBundle { get; }
    public bool DeleteFiles { get; }
    public string CompatibilityVersion { get; }

    public ModBuilderSettings(bool buildCode,
                              bool buildWindowsAssetBundle,
                              bool buildMacAssetBundle,
                              bool deleteFiles,
                              string compatibilityVersion) {
      BuildCode = buildCode;
      BuildWindowsAssetBundle = buildWindowsAssetBundle;
      BuildMacAssetBundle = buildMacAssetBundle;
      DeleteFiles = deleteFiles;
      CompatibilityVersion = compatibilityVersion;
    }

  }
}