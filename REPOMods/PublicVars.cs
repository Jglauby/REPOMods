using UnityEngine;
using Photon.Pun;
using OpJosModREPO.IAmEnemy.Networking;
using OpJosModREPO.IAmEnemy.Util;

namespace OpJosModREPO.IAmEnemy
{
    public static class PublicVars
    {
        public static int TimesSpawnedEnemy = 0;
        public static bool EnemyCleanupInProgress = false;
        public static bool EnemyInBlendMode = false;
        public static EnemyTypes NextSpawnType;

        // Interactive selection state
        public static bool IsChoosingEnemy = false;
        public static Vector3 PendingSpawnPosition = Vector3.zero;

        public static void SetNextSpawnType()
        {
            NextSpawnType = ConfigVariables.whatEnemyYouSpawnAs;
            if (ConfigVariables.whatEnemyYouSpawnAs == EnemyTypes.Random)
            {
                NextSpawnType = (EnemyTypes)Random.Range(1, System.Enum.GetValues(typeof(EnemyTypes)).Length);
            }
            else if (ConfigVariables.whatEnemyYouSpawnAs == EnemyTypes.ChooseEnemy)
            {
                // Default fallback if interactive selection is not used
                NextSpawnType = EnemyTypes.Duck;
            }
        }

        // Start interactive selection UI. This will create a persistent object that shows the chooser.
        public static void BeginChooseEnemy(Vector3 spawnPosition)
        {
            if (IsChoosingEnemy) return;

            IsChoosingEnemy = true;
            PendingSpawnPosition = spawnPosition;

            var go = new GameObject("IAmEnemy_Selector");
            GameObject.DontDestroyOnLoad(go);
            go.AddComponent<UI.EnemySelectorBehaviour>();
        }

        // Called by the selector when the player confirms their choice.
        public static void CompleteEnemySelection(EnemyTypes selected)
        {
            NextSpawnType = selected;
            IsChoosingEnemy = false;

            var selector = GameObject.Find("IAmEnemy_Selector");
            if (selector != null)
            {
                GameObject.Destroy(selector);
            }

            // Enforce limits and perform spawn using the same logic as PlayerAvatarPatch
            if (ConfigVariables.limitEnemiesPerLevel && TimesSpawnedEnemy >= ConfigVariables.maxEnemiesPerLevel)
            {
                Debug.Log("[IAmEnemy] Can't spawn enemy again, set to spectate");
                GeneralUtil.ReleaseEnemyControlToSpectate();
                return;
            }

            TimesSpawnedEnemy += 1;

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("[IAmEnemy] Confirmed selection, spawning enemy as host");
                GeneralUtil.SpawnEnemyAt(PendingSpawnPosition, 1, NextSpawnType);
            }
            else
            {
                Debug.Log("[IAmEnemy] Confirmed selection, sending spawn enemy request to host");
                Debug.Log($"[CLIENT] Sending enemy spawn request. My actor number: {PhotonNetwork.LocalPlayer.ActorNumber}");
                EnemySpawnerNetwork.Instance.RequestSpawnEnemy(PendingSpawnPosition, NextSpawnType);
            }
        }
    }
}
