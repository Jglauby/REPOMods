using System.Collections.Generic;

namespace OpJosModREPO.IAmEnemy
{
    public static class EnemyPrefabMap
    {
        private static readonly Dictionary<EnemyTypes, string> prefabPaths = new Dictionary<EnemyTypes, string>()
        {
            { EnemyTypes.Duck, "Enemies/Enemy - Duck" },
            { EnemyTypes.Peeper, "Enemies/Enemy - Peeper" },
            { EnemyTypes.ShadowChild, "Enemies/Enemy - ShadowChild" },
            { EnemyTypes.Gnome, "Enemies/Enemy - Gnome" },
            { EnemyTypes.Spewer, "Enemies/Enemy - Spewer" },
            { EnemyTypes.Baby, "Enemies/Enemy - Baby" },
            { EnemyTypes.Animal, "Enemies/Enemy - Animal" },
            { EnemyTypes.Upscream, "Enemies/Enemy - Upscream" },
            { EnemyTypes.Chef, "Enemies/Enemy - Chef" },
            { EnemyTypes.Hidden, "Enemies/Enemy - Hidden" },
            { EnemyTypes.Bowtie, "Enemies/Enemy - Bowtie" },
            { EnemyTypes.Mentalist, "Enemies/Enemy - Mentalist" },
            { EnemyTypes.Banger, "Enemies/Enemy - Banger" },
            { EnemyTypes.Headman, "Enemies/Enemy - Headman" },
            { EnemyTypes.Robe, "Enemies/Enemy - Robe" },
            { EnemyTypes.Huntsman, "Enemies/Enemy - Huntsman" },
            { EnemyTypes.Reaper, "Enemies/Enemy - Reaper" },
            { EnemyTypes.Clown, "Enemies/Enemy - Clown" },
            { EnemyTypes.Trudge, "Enemies/Enemy - Trudge" }
        };

        public static string GetPrefabPath(EnemyTypes type)
        {
            if (prefabPaths.TryGetValue(type, out string path))
            {
                return path;
            }

            return null;
        }
    }
}
