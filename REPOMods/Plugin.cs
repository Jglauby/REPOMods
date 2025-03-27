using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.BeeMovie.Patches;

namespace OpJosModREPO.BeeMovie
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosModBase : BaseUnityPlugin
    {
        private const string modGUID = "OpJosModREPO.BeeMovie";
        private const string modName = "BeeMovie";
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

            PlayerAvatarPatch.SetLogSource(mls);
            harmoy.PatchAll();
        }
    }
}
