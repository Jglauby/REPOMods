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
    }
}
