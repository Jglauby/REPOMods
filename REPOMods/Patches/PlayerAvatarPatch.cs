using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;

namespace OpJosModREPO.OpModTesting.Patches
{
    [HarmonyPatch(typeof(PlayerAvatar))]
    internal class PlayerAvatarPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Jump")]
        [HarmonyPostfix]
        static void JumpPatch(PlayerAvatar __instance)
        {
            LogAllEnemyPrefabsInScene();
        }
        public static void LogAllEnemyPrefabsInScene()
        {
            var allGameObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var obj in allGameObjects)
            {
                if (obj.scene.IsValid() && obj.name.StartsWith("Enemy - ", StringComparison.OrdinalIgnoreCase))
                {
                    mls.LogWarning($"[EnemyPrefab] Name: {obj.name}, Path: {GetHierarchyPath(obj)}");
                }
            }
        }
        private static string GetHierarchyPath(GameObject obj)
        {
            string path = obj.name;
            Transform current = obj.transform;
            while (current.parent != null)
            {
                current = current.parent;
                path = current.name + "/" + path;
            }
            return path;
        }
    }
}
