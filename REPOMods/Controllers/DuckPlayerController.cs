using BepInEx.Logging;
using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Networking;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using REPOMods;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class DuckPlayerController : EnemyControllerBase
    {
        public EnemyDuck thisDuck = null;

        public void Setup(int actorNumber, EnemyDuck duck)
        {
            thisDuck = duck;

            var specs = new EnemySpecs
            {
                MoveSpeed = 2.7f,
                TurnSpeed = 3f,
                JumpForce = 0.7f,
                AttackDelay = 3f
            };
            base.OnSetup(actorNumber, duck.gameObject, duck.enemy, duck.transform, specs);
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
            if (controlActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)//dont listen to keys if not your duck
                return;

            try
            {
                if (Keyboard.current[ConfigVariables.attackButtonKey].wasPressedThisFrame)
                {
                    DelayUtility.RunAfterDelay(attackDelay, () => { 
                        if (thisDuck.currentState == EnemyDuck.State.AttackStart)
                        {
                            mls.LogInfo("Stopping duck attack mode");
                            ReflectionUtils.InvokeMethod(thisDuck, "UpdateState", new object[] { EnemyDuck.State.Idle });
                        }
                        else if (ConfigVariables.allowAttackToggle)
                        {
                            mls.LogInfo("Starting duck attack mode");
                            ReflectionUtils.InvokeMethod(thisDuck, "UpdateState", new object[] { EnemyDuck.State.AttackStart });
                        }
                    }
                }
            }
            catch { }
        }

        private void attackNearbyEnemies()
        {
            if (thisDuck.currentState == EnemyDuck.State.AttackStart)
            {
                List<Enemy> closeEnemies = GeneralUtil.FindCloseEnemies(thisDuck.transform.position, 2.25f);
                foreach (var enemy in closeEnemies)
                {
                    if (enemy != null && enemy.GetInstanceID() != thisDuck.enemy.GetInstanceID())//not controlled duck
                    {
                        Vector3 toEnemy = (enemy.transform.position - thisDuck.transform.position).normalized;
                        float angle = Vector3.Angle(thisDuck.transform.forward, toEnemy);

                        if (angle < 50f)
                        {
                            EnemyHealth healthComponent = ReflectionUtils.GetFieldValue<EnemyHealth>(enemy, "Health");
                            if (healthComponent != null)
                            {
                                // Get direction from duck to enemy
                                Vector3 hurtDir = (enemy.transform.position - thisDuck.transform.position).normalized;

                                // Call internal method "Hurt"
                                healthComponent.Hurt(20, hurtDir);
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
