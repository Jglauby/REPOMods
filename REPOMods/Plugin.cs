using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.Controllers.IAmEnemy;
using OpJosModREPO.IAmEnemy.Patches;
using OpJosModREPO.IAmEnemy.Util;
using UnityEngine.InputSystem;

namespace OpJosModREPO.IAmEnemy
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosModBase : BaseUnityPlugin
    {
        private const string modGUID = "OpJosModREPO.IAmEnemy";
        private const string modName = "IAmEnemy";
        private const string modVersion = "0.7.0";

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
            EnemyControllerBase.SetLogSource(mls);
            GeneralUtil.SetLogSource(mls);
            harmoy.PatchAll();
        }

        private void setupConfig()
        {
            var configWhatEnemySpawn = Config.Bind("What Enemy You Spawn As",
                                        "WhatEnemySpawnAs",
                                        EnemyTypes.Random,
                                        "Which enemy do you want to spawn as?");

            var configLimitEnemiesPerLevel = Config.Bind("Limit Enemy Spawns Per Level",
                            "LimitEnemySpawnPerLevel",
                            false,
                            "Should the player be limited on how many times they can be an enemy per level?");

            var configMaxEnemiesPerLevel = Config.Bind("Max Enemies Per Level",
                                        "MaxEnemiesPerLevel",
                                        1,
                                        "How many times a player can become an enemy? per player");

            var configAllowAttackToggle = Config.Bind("Allow Attack Toggle",
                                        "AllowAttackToggle",
                                        true,
                                        "Should the enemy be able to manually start attack mode?");

            var configAttackButton = Config.Bind("Attack Mode Button",
                                        "AttackModeButton",
                                        Key.E,
                                        "Button to toggle on and off enemy's attack or to trigger an attack (depends on enemy)");

            var configSelfDestructButton = Config.Bind("Self Destruct Button",
                                        "SelfDestructButton",
                                        Key.K,
                                        "Button to self destruct the controlled enemy");

            var configToggleBlendMode = Config.Bind("Toggle Blend Mode Button",
                                        "ToggleBlendModeButton",
                                        Key.B,
                                        "Button to turn on the enemy's AI to blend in as a normal enemy");

            ConfigVariables.whatEnemyYouSpawnAs = configWhatEnemySpawn.Value;
            ConfigVariables.allowAttackToggle = configAllowAttackToggle.Value;
            ConfigVariables.attackButtonKey = configAttackButton.Value;
            ConfigVariables.selfDestructKey = configSelfDestructButton.Value;
            ConfigVariables.toggleBlendModeKey = configToggleBlendMode.Value;
            ConfigVariables.limitEnemiesPerLevel = configLimitEnemiesPerLevel.Value;
            ConfigVariables.maxEnemiesPerLevel = configMaxEnemiesPerLevel.Value;
        }
    }
}
