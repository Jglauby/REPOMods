using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using System;
using UnityEngine.InputSystem;
using UnityEngine;

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
        }  
    }
}
