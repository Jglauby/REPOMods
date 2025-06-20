using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using System;
using UnityEngine.InputSystem;
using UnityEngine;
using OpJosModREPO.IAmEnemy.Networking;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class MentalistPlayerController : EnemyControllerBase
    {
        public EnemyFloater thisMentalist = null;

        public void Setup(int actorNumber, EnemyFloater mentalist)
        {
            thisMentalist = mentalist;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(mentalist, "enemy");
            base.OnSetup(actorNumber, mentalist.gameObject, enemy, mentalist.transform);
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
                if (Keyboard.current[ConfigVariables.attackButtonKey].wasPressedThisFrame && ConfigVariables.allowAttackToggle)
                {
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

            try
            {
                if (Keyboard.current[Key.Space].wasPressedThisFrame)
                {
                    if (isHost)
                    {
                        SpecialMovement(1);
                    }
                    else
                    {
                        EnemySpawnerNetwork.Instance.TriggerSpecialMovement(1, controlActorNumber);
                    }
                }
            }
            catch { }

            try
            {
                if (Keyboard.current[Key.LeftCtrl].wasPressedThisFrame)
                {
                    if (isHost)
                    {
                        SpecialMovement(0);
                    }
                    else
                    {
                        EnemySpawnerNetwork.Instance.TriggerSpecialMovement(0, controlActorNumber);
                    }
                }
            }
            catch { }
        }

        public override void SpecialMovement(int num)
        {
            //1 -> up, 0 -> down
            if (num == 1)
            {
                EnemyRigidbody erb = ReflectionUtils.GetFieldValue<EnemyRigidbody>(thisEnemyEnemy, "Rigidbody");
                Rigidbody rb = ReflectionUtils.GetFieldValue<Rigidbody>(erb, "rb");
                rb.AddForce(Vector3.up * 1f, ForceMode.Impulse);
            }
            else if (num == 0)
            {
                EnemyRigidbody erb = ReflectionUtils.GetFieldValue<EnemyRigidbody>(thisEnemyEnemy, "Rigidbody");
                Rigidbody rb = ReflectionUtils.GetFieldValue<Rigidbody>(erb, "rb");
                rb.AddForce(Vector3.down * 1f, ForceMode.Impulse);
            }
        }
    }
}
