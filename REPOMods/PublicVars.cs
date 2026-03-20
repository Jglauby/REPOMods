using UnityEngine;

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
                //TODO: Implement a way for the player to choose the next spawn type, maybe through a UI or by pressing a key to cycle through options.
                NextSpawnType = EnemyTypes.Duck;
            }
        }
    }
}
