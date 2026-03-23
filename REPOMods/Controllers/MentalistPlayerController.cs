using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Networking;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using REPOMods;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class MentalistPlayerController : EnemyControllerBase
    {
        public EnemyFloater thisMentalist = null;

        public void Setup(int actorNumber, EnemyFloater mentalist)
        {
            thisMentalist = mentalist;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(mentalist, "enemy");

            var specs = new EnemySpecs
            {
                MoveSpeed = 2.7f,
                TurnSpeed = 3f,
                AttackDelay = 0.1f,
                FlyingEnemy = true,
            };
            base.OnSetup(actorNumber, mentalist.gameObject, enemy, mentalist.transform, specs);
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
                    if (thisMentalist.currentState == EnemyFloater.State.Attack)
                    {
                        mls.LogInfo("Stopping mentalist attack");
                        ReflectionUtils.InvokeMethod(thisMentalist, "UpdateState", new object[] { EnemyFloater.State.Idle });
                    }
                    else
                    {
                        mls.LogInfo("Starting mentalist attack");
                        ReflectionUtils.InvokeMethod(thisMentalist, "UpdateState", new object[] { EnemyFloater.State.ChargeAttack });

                        DelayUtility.RunAfterDelay(0.5f, () =>
                        {
                            ReflectionUtils.InvokeMethod(thisMentalist, "UpdateState", new object[] { EnemyFloater.State.Attack });
                        });
                    }
                }
            }
            catch { }
        }    
    }
}
