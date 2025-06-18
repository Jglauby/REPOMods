using OpJosModREPO.Controllers.IAmEnemy;
using System;
using System.Collections.Generic;

namespace OpJosModREPO.IAmEnemy
{
    public static class EnemyMaps
    {
        private static readonly Dictionary<EnemyTypes, string> prefabPaths = new Dictionary<EnemyTypes, string>()
        {
            { EnemyTypes.Duck, "Enemies/Enemy - Duck" },
            //{ EnemyTypes.Peeper, "Enemies/Enemy - Ceiling Eye" },
            { EnemyTypes.ShadowChild, "Enemies/Enemy - Thin Man" },
            { EnemyTypes.Gnome, "Enemies/Enemy - Gnome" },
            { EnemyTypes.Spewer, "Enemies/Enemy - Slow Mouth" },
            { EnemyTypes.Baby, "Enemies/Enemy - Valuable Thrower" },
            { EnemyTypes.Animal, "Enemies/Enemy - Animal" },
            { EnemyTypes.Upscream, "Enemies/Enemy - Upscream" },
            { EnemyTypes.Chef, "Enemies/Enemy - Chef" },
            { EnemyTypes.Hidden, "Enemies/Enemy - Hidden" },
            { EnemyTypes.Bowtie, "Enemies/Enemy - Bowtie" },
            { EnemyTypes.Mentalist, "Enemies/Enemy - Mentalist" },
            { EnemyTypes.Banger, "Enemies/Enemy - Banger" },
            { EnemyTypes.Headman, "Enemies/Enemy - Head" },
            { EnemyTypes.Robe, "Enemies/Enemy - Robe" },
            { EnemyTypes.Huntsman, "Enemies/Enemy - Hunter" },
            { EnemyTypes.Reaper, "Enemies/Enemy - Runner" },
            { EnemyTypes.Clown, "Enemies/Enemy - Beamer" },
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

        private static readonly Dictionary<EnemyTypes, Type> enemyComponentTypes = new Dictionary<EnemyTypes, Type>()
        {
            { EnemyTypes.Duck, typeof(EnemyDuck) },
            //{ EnemyTypes.Peeper, typeof(EnemyCeilingEye) },
            { EnemyTypes.ShadowChild, typeof(EnemyThinMan) },
            { EnemyTypes.Gnome, typeof(EnemyGnome) },
            { EnemyTypes.Spewer, typeof(EnemySlowMouth) },
            { EnemyTypes.Baby, typeof(EnemyValuableThrower) },
            { EnemyTypes.Animal, typeof(EnemyAnimal) },
            { EnemyTypes.Upscream, typeof(Enemy) },
            { EnemyTypes.Chef, typeof(Enemy) },
            { EnemyTypes.Hidden, typeof(Enemy) },
            { EnemyTypes.Bowtie, typeof(EnemyBowtie) },
            { EnemyTypes.Mentalist, typeof(Enemy) },
            { EnemyTypes.Banger, typeof(Enemy) },
            { EnemyTypes.Headman, typeof(EnemyHeadController) },
            { EnemyTypes.Robe, typeof(EnemyRobe) },
            { EnemyTypes.Huntsman, typeof(EnemyHunter) },
            { EnemyTypes.Reaper, typeof(EnemyRunner) },
            { EnemyTypes.Clown, typeof(EnemyBeamer) },
            { EnemyTypes.Trudge, typeof(Enemy) }
        };

        public static Type GetEnemyType(EnemyTypes type)
        {
            if (enemyComponentTypes.TryGetValue(type, out Type obj))
            {
                return obj;
            }

            return null;
        }

        public static EnemyTypes? GetEnemyTypeFromInstance(Enemy enemyInstance)
        {
            foreach (var kvp in enemyComponentTypes)
            {
                if (enemyInstance.GetComponent(kvp.Value) != null)
                {
                    return kvp.Key;
                }
            }

            return null;
        }

        public static IReadOnlyDictionary<EnemyTypes, Type> ControllerTypes => enemyControllerTypes;
        private static readonly Dictionary<EnemyTypes, Type> enemyControllerTypes = new Dictionary<EnemyTypes, Type>()
        {
            { EnemyTypes.Duck, typeof(DuckPlayerController) },
            { EnemyTypes.Robe, typeof(RobePlayerController) },
            { EnemyTypes.Spewer, typeof(SpewerPlayerController) },
            { EnemyTypes.Reaper, typeof(ReaperPlayerController) },
            { EnemyTypes.Clown, typeof(ClownPlayerController) },
            //{ EnemyTypes.Peeper, typeof(PeeperPlayerController) },
            { EnemyTypes.Gnome, typeof(GnomePlayerController) },
            { EnemyTypes.ShadowChild, typeof(ShadowChildPlayerController) },
            { EnemyTypes.Headman, typeof(HeadManPlayerController) },
            { EnemyTypes.Huntsman, typeof(HuntsmanPlayerController) },
            { EnemyTypes.Baby, typeof(BabyPlayerController) },
        };

        public static Type GetControllerType(EnemyTypes type)
        {
            if (enemyControllerTypes.TryGetValue(type, out Type controllerType))
            {
                return controllerType;
            }

            return null;
        }
    }
}
