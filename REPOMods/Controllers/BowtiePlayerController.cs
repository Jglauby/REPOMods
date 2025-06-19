using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using UnityEngine.InputSystem;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class BowtiePlayerController : EnemyControllerBase
    {
        public EnemyBowtie thisBowtie = null;

        public void Setup(int actorNumber, EnemyBowtie bowtie)
        {
            thisBowtie = bowtie;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(bowtie, "enemy");
            base.OnSetup(actorNumber, bowtie.gameObject, enemy, bowtie.transform);
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
                if (Keyboard.current[ConfigVariables.attackButtonKey].wasPressedThisFrame)
                {
                    if (thisBowtie.currentState == EnemyBowtie.State.Yell)
                    {
                        mls.LogInfo("Stopping bowtie attack mode");
                        ReflectionUtils.InvokeMethod(thisBowtie, "UpdateState", new object[] { EnemyBowtie.State.YellEnd });

                        DelayUtility.RunAfterDelay(0.25f, () =>
                        {
                            ReflectionUtils.InvokeMethod(thisBowtie, "UpdateState", new object[] { EnemyBowtie.State.Idle });
                        });
                    }
                    else if (ConfigVariables.allowAttackToggle)
                    {
                        mls.LogInfo("Starting bowtie attack mode");
                        ReflectionUtils.InvokeMethod(thisBowtie, "UpdateState", new object[] { EnemyBowtie.State.Yell });
                    }
                }
            }
            catch { }
        }  
    }
}
