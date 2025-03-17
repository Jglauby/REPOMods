using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.RainbowPlayer.Patches;

namespace OpJosModREPO.RainbowPlayer
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosModBase : BaseUnityPlugin
    {
        private const string modGUID = "OpJosModREPO.RainbowPlayer";
        private const string modName = "RainbowPlayer";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmoy = new Harmony(modGUID);
        private static OpJosModBase Instance;
        internal ManualLogSource mls;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo($"{modName} has started!");

            PlayerAvaterPatch.SetLogSource(mls);
            harmoy.PatchAll();
        }
    }
}
