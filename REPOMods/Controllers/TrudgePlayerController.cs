using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using REPOMods;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class TrudgePlayerController : EnemyControllerBase
    {
        public EnemySlowWalker thisTrudge = null;

        public void Setup(int actorNumber, EnemySlowWalker trudge)
        {
            thisTrudge = trudge;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(trudge, "enemy");

            var specs = new EnemySpecs
            {
                MoveSpeed = 0.7f,
                TurnSpeed = 3f,
                JumpForce = 3f,
                AttackDelay = 10f
            };
            base.OnSetup(actorNumber, trudge.gameObject, enemy, trudge.transform, specs);
        }

        void Update()
        {
            if (!isYourEnemy)
                return;

            base.UpdateLogic();
            handleInput();
        }

        void FixedUpdate()
        {
            if (PublicVars.EnemyInBlendMode || isInBlendMode)
                return;

            base.FixedUpdateLogic();

            if (thisTrudge.currentState == EnemySlowWalker.State.Idle)
                ReflectionUtils.InvokeMethod(thisTrudge, "UpdateState", new object[] { EnemySlowWalker.State.Roam });
        }

        private void handleInput()
        {
            if (controlActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)//dont listen to keys if not your enemy
                return;

            try
            {
                if (Keyboard.current[ConfigVariables.attackButtonKey].wasPressedThisFrame && ConfigVariables.allowAttackToggle && timeSinceLastAttack >= attackDelay)
                {
                    lastAttackTime = Time.time;
                    ReflectionUtils.InvokeMethod(thisTrudge, "UpdateState", new object[] { EnemySlowWalker.State.Attack });

                    DelayUtility.RunAfterDelay(4f, () =>
                    {
                        attackNearbyEnemies();
                        DelayUtility.RunAfterDelay(1f, () =>
                        {
                            ReflectionUtils.InvokeMethod(thisTrudge, "UpdateState", new object[] { EnemySlowWalker.State.Idle });
                        });
                    });
                }
            }
            catch { }
        }

        private void attackNearbyEnemies()
        {
            List<Enemy> closeEnemies = GeneralUtil.FindCloseEnemies(thisTrudge.transform.position, 6f);
            foreach (var enemy in closeEnemies)
            {
                if (enemy != null && enemy.GetInstanceID() != thisTrudge.enemy.GetInstanceID())//not controlled enemy
                {
                    Vector3 toEnemy = (enemy.transform.position - thisTrudge.transform.position).normalized;
                    float angle = Vector3.Angle(thisTrudge.transform.forward, toEnemy);

                    if (angle < 50f)
                    {
                        EnemyHealth healthComponent = ReflectionUtils.GetFieldValue<EnemyHealth>(enemy, "Health");
                        if (healthComponent != null)
                        {
                            // Get direction from this enemy to enemy beign attacked
                            Vector3 hurtDir = (enemy.transform.position - thisTrudge.transform.position).normalized;

                            // Call internal method "Hurt"
                            healthComponent.Hurt(120, hurtDir);
                        }
                        else
                        {
                            mls.LogError($"Health component not found for enemy: {enemy.name}");
                        }
                    }
                }
            }
        }
    }
}
