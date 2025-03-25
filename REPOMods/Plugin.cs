using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.godmode.Patches;

namespace OpJosModREPO.godmode
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosModBase : BaseUnityPlugin
    {
        private const string modGUID = "OpJosModREPO.godmode";
        private const string modName = "godmode";
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
