using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using Photon.Realtime;
using System.Reflection;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class HuntsmanPlayerController : EnemyControllerBase
    {
        public EnemyHunter thisHunter = null;

        public void Setup(int actorNumber, EnemyHunter hunter)
        {
            thisHunter = hunter;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(hunter, "enemy");
            base.OnSetup(actorNumber, hunter.gameObject, enemy, hunter.transform);
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
            if (controlActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)//dont listen to keys if not your duck
                return;

            try
            {
                if (Keyboard.current[ConfigVariables.attackButtonKey].wasPressedThisFrame && ConfigVariables.allowAttackToggle)
                {
                    Vector3 targetPosition = Camera.main.transform.position + Camera.main.transform.forward * 10f;

                    ReflectionUtils.SetFieldValue(thisHunter, "investigatePoint", targetPosition);
                    ReflectionUtils.InvokeMethod(thisHunter, "UpdateState", new object[] { EnemyHunter.State.Idle });
                    ReflectionUtils.InvokeMethod(thisHunter, "UpdateState", new object[] { EnemyHunter.State.Aim });
                    mls.LogInfo("Set hunter to Aim at: " + targetPosition);

                    //wait after aim and then shoot
                    DelayUtility.RunAfterDelay(0.5f, () => {
                        ReflectionUtils.InvokeMethod(thisHunter, "StateShoot", null);

                        //wait after shooting and then set state to ShootEnd
                        DelayUtility.RunAfterDelay(0.25f, () => {
                            ReflectionUtils.InvokeMethod(thisHunter, "UpdateState", new object[] { EnemyHunter.State.ShootEnd });
                        });

                        //set it back to idle after a delay
                        DelayUtility.RunAfterDelay(0.5f, () =>
                        {
                            ReflectionUtils.InvokeMethod(thisHunter, "UpdateState", new object[] { EnemyHunter.State.Idle });
                            mls.LogInfo("Returned hunter to Idle.");
                        });
                    });
                }
            }
            catch (Exception e) { mls.LogError(e); }
        }
    }
}
