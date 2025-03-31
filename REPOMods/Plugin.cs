using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.IAmDucky.Networking;
using OpJosModREPO.IAmDucky.Patches;
using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace OpJosModREPO.IAmDucky
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosModBase : BaseUnityPlugin
    {
        private const string modGUID = "OpJosModREPO.IAmDucky";
        private const string modName = "IAmDucky";
        private const string modVersion = "1.1.1";

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
            EnemyHealthPatch.SetLogSource(mls);
            DuckPlayerController.SetLogSource(mls);
            GeneralUtil.SetLogSource(mls);
            harmoy.PatchAll();
        }
    }
}
