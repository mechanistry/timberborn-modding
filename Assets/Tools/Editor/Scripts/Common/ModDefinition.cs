namespace Timberborn.ModdingTools.Common {
  public readonly struct ModDefinition {

    public string Name { get; }
    public string ProjectPath { get; }
    public string AbsolutePath { get; }

    public ModDefinition(string name,
                         string projectPath,
                         string absolutePath) {
      Name = name;
      ProjectPath = projectPath;
      AbsolutePath = absolutePath;
    }

  }
}