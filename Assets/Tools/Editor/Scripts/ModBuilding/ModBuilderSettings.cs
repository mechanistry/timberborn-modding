namespace Timberborn.ModdingTools.ModBuilding {
  public readonly struct ModBuilderSettings {

    public bool BuildCode { get; }
    public bool BuildWindowsAssetBundle { get; }
    public bool BuildMacAssetBundle { get; }
    public bool DeleteFiles { get; }
    public bool BuildZipArchive { get; }
    public string CompatibilityVersion { get; }

    public ModBuilderSettings(bool buildCode,
                              bool buildWindowsAssetBundle,
                              bool buildMacAssetBundle,
                              bool deleteFiles,
                              bool buildZipArchive,
                              string compatibilityVersion) {
      BuildCode = buildCode;
      BuildWindowsAssetBundle = buildWindowsAssetBundle;
      BuildMacAssetBundle = buildMacAssetBundle;
      DeleteFiles = deleteFiles;
      BuildZipArchive = buildZipArchive;
      CompatibilityVersion = compatibilityVersion;
    }

  }
}