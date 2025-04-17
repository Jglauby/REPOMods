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
            var configAttackToggleButton = Config.Bind("Attack Mode Toggle",
                                        "AttackModeToggle",
                                        Key.E,
                                        "Button to toggle on and off duck's attack mode");

            var configResetControlOnDuckButton = Config.Bind("Reset Control On Duck Button",
                                        "ResetControlButton",
                                        Key.C,
                                        "Button to reset control on duck in case things get wack.");

            ConfigVariables.attackToggleKey = configAttackToggleButton.Value;
        }
    }
}
