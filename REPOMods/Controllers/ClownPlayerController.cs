using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Networking;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class ClownPlayerController : EnemyControllerBase
    {
        public EnemyBeamer thisBeamer = null;
        private Vector3 laserLocation;
        private Vector3 laserAngle;

        public void Setup(int actorNumber, EnemyBeamer beamer)
        {
            thisBeamer = beamer;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(beamer, "enemy");
            base.OnSetup(actorNumber, beamer.gameObject, enemy, beamer.transform);
            ReflectionUtils.InvokeMethod(thisBeamer, "UpdateState", new object[] { EnemyBeamer.State.Roam }); //roam instead of idle so it can get walk animations
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
            base.FixedUpdateLogic();

            if (thisBeamer.currentState == EnemyBeamer.State.Idle)
                ReflectionUtils.InvokeMethod(thisBeamer, "UpdateState", new object[] { EnemyBeamer.State.Roam });

            if (thisBeamer.currentState == EnemyBeamer.State.Attack)
            {
                if (isYourEnemy)
                {
                    laserAngle = cameraTransform.eulerAngles;
                    laserLocation = thisBeamer.laserStartTransform.position + cameraTransform.forward * 20f;

                    if (!PhotonNetwork.IsMasterClient)
                    {
                        if (syncTimer >= syncInterval)
                        {
                            EnemySpawnerNetwork.Instance.TriggerSpecialAttack(laserLocation, laserAngle, controlActorNumber);
                        }
                    }
                }

                if (PhotonNetwork.IsMasterClient)
                {
                    ShootLaser();
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
                    if (thisBeamer.currentState == EnemyBeamer.State.Attack)
                    {
                        mls.LogInfo("Stopping clown attack mode");
                        ReflectionUtils.InvokeMethod(thisBeamer, "UpdateState", new object[] { EnemyBeamer.State.AttackEnd });

                        DelayUtility.RunAfterDelay(0.5f, () =>
                        {
                            ReflectionUtils.InvokeMethod(thisBeamer, "UpdateState", new object[] { EnemyBeamer.State.Roam });
                        });
                    }
                    else if (ConfigVariables.allowAttackToggle)
                    {
                        mls.LogInfo("Starting clown attack mode");
                        ReflectionUtils.InvokeMethod(thisBeamer, "UpdateState", new object[] { EnemyBeamer.State.AttackStart });

                        DelayUtility.RunAfterDelay(0.5f, () =>
                        {
                            ReflectionUtils.InvokeMethod(thisBeamer, "UpdateState", new object[] { EnemyBeamer.State.Attack });
                        });
                    }
                }
            }
            catch { }
        }  

        public override void SpecialAttack(Vector3 pos, Vector3 angle)
        {
            //mls.LogInfo($"updating clown laser position to {pos} and angle to {angle}");
            laserAngle = angle;
            laserLocation = pos;
        }

        private void ShootLaser()
        {
            ReflectionUtils.SetFieldValue(thisBeamer, "aimHorizontalTarget", Quaternion.Euler(0f, laserAngle.y, 0f));
            ReflectionUtils.SetFieldValue(thisBeamer, "hitPosition", laserLocation);
            ReflectionUtils.InvokeMethod(thisBeamer, "RotationLogic", new object[] { });
            ReflectionUtils.InvokeMethod(thisBeamer, "VerticalAimLogic", new object[] { });
            ReflectionUtils.InvokeMethod(thisBeamer, "LaserLogic", new object[] { });
        }
    }
}
