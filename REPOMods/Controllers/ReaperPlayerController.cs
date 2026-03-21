using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using REPOMods;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class ReaperPlayerController : EnemyControllerBase
    {
        public EnemyRunner thisRunner = null;

        public void Setup(int actorNumber, EnemyRunner runner)
        {
            thisRunner = runner;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(runner, "enemy");

            var specs = new EnemySpecs
            {
                MoveSpeed = 1.7f,
                TurnSpeed = 3f,
                JumpForce = 0.5f,
                AttackDelay = 0.1f
            };
            base.OnSetup(actorNumber, runner.gameObject, enemy, runner.transform, specs);
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

            if (thisRunner.currentState == EnemyRunner.State.Idle)
                ReflectionUtils.InvokeMethod(thisRunner, "UpdateState", new object[] { EnemyRunner.State.Roam });

            if (isYourEnemy)
            {
                if (attackCooldown > 0f)
                    attackCooldown -= Time.fixedDeltaTime;

                if (attackCooldown <= 0f)
                {
                    attackNearbyEnemies();
                    attackCooldown = 0.75f;
                }
            }
        }

        private void handleInput()
        {
            if (controlActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)//dont listen to keys if not your enemy
                return;

            try
            {
                if (Keyboard.current[ConfigVariables.attackButtonKey].wasPressedThisFrame)
                {
                    DelayUtility.RunAfterDelay(attackDelay, () =>
                    {
                        if (thisRunner.currentState == EnemyRunner.State.AttackPlayer)
                        {
                            mls.LogInfo("Stopping reaper attack mode");
                            ReflectionUtils.InvokeMethod(thisRunner, "UpdateState", new object[] { EnemyRunner.State.Idle });
                        }
                        else if (ConfigVariables.allowAttackToggle)
                        {
                            mls.LogInfo("Starting reaper attack mode");
                            ReflectionUtils.InvokeMethod(thisRunner, "UpdateState", new object[] { EnemyRunner.State.AttackPlayer });
                        }
                    });
                }
            }
            catch { }
        }

        private void attackNearbyEnemies()
        {
            if (thisRunner.currentState == EnemyRunner.State.AttackPlayer)
            {
                List<Enemy> closeEnemies = GeneralUtil.FindCloseEnemies(thisRunner.transform.position, 3.5f);
                foreach (var enemy in closeEnemies)
                {
                    if (enemy != null && enemy.GetInstanceID() != thisRunner.enemy.GetInstanceID())//not controlled enemy
                    {
                        Vector3 toEnemy = (enemy.transform.position - thisRunner.transform.position).normalized;
                        float angle = Vector3.Angle(thisRunner.transform.forward, toEnemy);

                        if (angle < 50f)
                        {
                            EnemyHealth healthComponent = ReflectionUtils.GetFieldValue<EnemyHealth>(enemy, "Health");
                            if (healthComponent != null)
                            {
                                // Get direction from this enemy to enemy beign attacked
                                Vector3 hurtDir = (enemy.transform.position - thisRunner.transform.position).normalized;

                                // Call internal method "Hurt"
                                healthComponent.Hurt(40, hurtDir);
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
}
