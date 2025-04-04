using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod;
using OpJosModREPO.TTSPranks.Patches;
using UnityEngine.InputSystem;

namespace OpJosModREPO.TTSPranks
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosModBase : BaseUnityPlugin
    {
        private const string modGUID = "OpJosModREPO.TTSPranks";
        private const string modName = "TTSPranks";
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
            setupConfig();

            PlayerAvatarPatch.SetLogSource(mls);
            harmoy.PatchAll();
        }

        private void setupConfig()
        {
            var configFlashBangButton = Config.Bind("Flash whole screen white.",
                                        "FlashBangButton",
                                        Key.F,
                                        "Button to briefly turn the whole screen white to players near you");

            var configDomainExpansionButton = Config.Bind("Turn whole screen black",
                                        "DomainExpansionButton",
                                        Key.G,
                                        "Button to turn the whole screen black for players near you");

            var configHeartEyesButton = Config.Bind("Display giant heart eyes emoji",
                                        "HeartEyesButton",
                                        Key.H,
                                        "Button to display giant heart eyes emoji");

            var configQuesitonPingButton = Config.Bind("Display big question mark",
                                        "QuestionPingButton",
                                        Key.J,
                                        "Button to display a quesiton mark");

            ConfigVariables.flashBangKey = configFlashBangButton.Value;
            ConfigVariables.domainExpansionKey = configDomainExpansionButton.Value;
            ConfigVariables.heartEyesKey = configHeartEyesButton.Value;
            ConfigVariables.questionPingKey = configQuesitonPingButton.Value;

            Config.Save();
        }
    }
}
