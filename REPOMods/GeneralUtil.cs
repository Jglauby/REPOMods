using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace REPOMods
{
    public static class GeneralUtil
    {
        public static GameObject FindClosestDuck(Vector3 pos)
        {
            GameObject cloestDuck = null;
            float closestDistance = float.MaxValue;

            foreach (var enemy in GameObject.FindObjectsOfType<Enemy>())
            {
                GameObject enemyObj = enemy.gameObject;

                if (enemyObj.name == "Enemy - Duck")
                {
                    float distance = Vector3.Distance(enemyObj.transform.position, pos);

                    if (distance < closestDistance)
                    {
                        cloestDuck = enemyObj;
                        closestDistance = distance;
                    }
                }
            }

            return cloestDuck;
        }
    }
}
