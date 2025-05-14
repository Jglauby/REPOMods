using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.IAmDuckyHostOnly.Patches;
using UnityEngine.InputSystem;

namespace OpJosModREPO.IAmDuckyHostOnly
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosModBase : BaseUnityPlugin
    {
        private const string modGUID = "OpJosModREPO.IAmDuckyHostOnly";
        private const string modName = "IAmDuckyHostOnly";
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

            var configDuckDamage = Config.Bind("Duck Damage to Enemies",
                                        "DuckDamageToEnemies",
                                        20,
                                        "How much damage the duck does when attacking other enemies");

            var configLimitDucksPerLevel = Config.Bind("Limit Ducks Per Level",
                                        "LimitDucksPerLevel",
                                        false,
                                        "Should the duck be limited to 1 per level?");

            var configMaxDucksPerLevel = Config.Bind("Max Ducks Per Level",
                                        "MaxDucksPerLevel",
                                        1,
                                        "How many ducks can be spawned per level? per player");

            ConfigVariables.allowAttackToggle = configAllowAttackToggle.Value;
            ConfigVariables.attackToggleKey = configAttackToggleButton.Value;
            ConfigVariables.resetControlKey = configResetControlOnDuckButton.Value;
            ConfigVariables.selfDestructKey = configSelfDestructButton.Value;
            ConfigVariables.toggleBlendModeKey = configToggleBlendMode.Value;
            ConfigVariables.duckDamage = configDuckDamage.Value;
            ConfigVariables.limitDucksPerLevel = configLimitDucksPerLevel.Value;
            ConfigVariables.maxDucksPerLevel = configMaxDucksPerLevel.Value;
        }
    }
}
