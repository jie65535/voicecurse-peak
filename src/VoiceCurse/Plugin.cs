using BepInEx;
using BepInEx.Logging;

namespace VoiceCurse;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin {
    internal static ManualLogSource Log { get; set; } = null!;

    private void Awake() {
        Log = Logger;
        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}
