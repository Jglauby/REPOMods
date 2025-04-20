using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.IAmDucky.Patches;
using UnityEngine.InputSystem;

namespace OpJosModREPO.IAmDucky
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosModBase : BaseUnityPlugin
    {
        private const string modGUID = "OpJosModREPO.IAmDucky";
        private const string modName = "IAmDucky";
        private const string modVersion = "1.3.0";

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
            EnemyHealthPatch.SetLogSource(mls);
            DuckPlayerController.SetLogSource(mls);
            GeneralUtil.SetLogSource(mls);
            harmoy.PatchAll();
        }

        private void setupConfig()
        {
            var configAllowAttackToggle = Config.Bind("Allow Attack Toggle",
                                        "AllowAttackToggle",
                                        true,
                                        "Should the duck be able to manually start attack mode?");

            var configAttackToggleButton = Config.Bind("Attack Mode Toggle",
                                        "AttackModeToggle",
                                        Key.E,
                                        "Button to toggle on and off duck's attack mode");

            var configResetControlOnDuckButton = Config.Bind("Reset Control On Duck Button",
                                        "ResetControlButton",
                                        Key.C,
                                        "Button to reset control on duck in case things get wack.");

            var configSelfDestructButton = Config.Bind("Self Destruct Button",
                                        "SelfDestructButton",
                                        Key.K,
                                        "Button to self destruct the duck");

            var configToggleBlendMode = Config.Bind("Toggle Blend Mode Button",
                                        "ToggleBlendModeButton",
                                        Key.B,
                                        "Button to turn on duck AI to blend in as a normal duck");

            ConfigVariables.allowAttackToggle = configAllowAttackToggle.Value;
            ConfigVariables.attackToggleKey = configAttackToggleButton.Value;
            ConfigVariables.resetControlKey = configResetControlOnDuckButton.Value;
            ConfigVariables.selfDestructKey = configSelfDestructButton.Value;
            ConfigVariables.toggleBlendModeKey = configToggleBlendMode.Value;
        }
    }
}
