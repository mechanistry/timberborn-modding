namespace ModBuilding.Editor {
  public readonly struct ModBuilderSettings {

    public bool BuildCode { get; }
    public bool BuildWindowsAssetBundle { get; }
    public bool BuildMacAssetBundle { get; }
    public bool DeleteFiles { get; }

    public ModBuilderSettings(bool buildCode,
                              bool buildWindowsAssetBundle,
                              bool buildMacAssetBundle,
                              bool deleteFiles) {
      BuildCode = buildCode;
      BuildWindowsAssetBundle = buildWindowsAssetBundle;
      BuildMacAssetBundle = buildMacAssetBundle;
      DeleteFiles = deleteFiles;
    }

  }
}