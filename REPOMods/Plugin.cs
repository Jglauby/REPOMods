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


            var configFlashBangTime = Config.Bind("How long flash bang lasts",
                                        "FlashBangTime",
                                        2f,
                                        "how long flash bang lasts in seconds");

            var configDomainExpansionTime = Config.Bind("How long domain expansion lasts",
                                        "DomainExpansionTime",
                                        6f,
                                        "How long screen stays black in seconds");

            var configHeartEyesTime = Config.Bind("How long the heart eyes emoji stays up",
                                        "HeartEyesTime",
                                        1f,
                                        "how long heart eyes emoji stays up in seconds");

            var configQuesitonPingTime = Config.Bind("How long quesiton mark stays up",
                                        "QuestionPingTime",
                                        1f,
                                        "How long question mark ping stays up in seconds");

            ConfigVariables.flashBangKey = configFlashBangButton.Value;
            ConfigVariables.domainExpansionKey = configDomainExpansionButton.Value;
            ConfigVariables.heartEyesKey = configHeartEyesButton.Value;
            ConfigVariables.questionPingKey = configQuesitonPingButton.Value;

            ConfigVariables.flashBangTime = configFlashBangTime.Value;
            ConfigVariables.domainExpansionTime = configDomainExpansionTime.Value;
            ConfigVariables.heartEyesTime = configHeartEyesTime.Value;
            ConfigVariables.questionPingTime = configQuesitonPingTime.Value;

            Config.Save();
        }
    }
}
