using Timberborn.ModManagerScene;
using UnityEngine;

namespace Mods.HelloWorld.Scripts {
  internal class HelloWorldLogger : IModStarter {

    public void StartMod(IModEnvironment modEnvironment) {
      var playerLogPath = Application.persistentDataPath + "/Player.log";
      Debug.Log("Hello World, but in the Player.log file at: " + playerLogPath);
    }

  }
}