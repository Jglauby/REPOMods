using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using UnityEngine.InputSystem;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class GnomePlayerController : EnemyControllerBase
    {
        public EnemyGnome thisGnome = null;

        public void Setup(int actorNumber, EnemyGnome gnome)
        {
            thisGnome = gnome;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(gnome, "enemy");
            base.OnSetup(actorNumber, gnome.gameObject, enemy, gnome.transform);
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
                    if (thisGnome.currentState == EnemyGnome.State.Attack)
                    {
                        mls.LogInfo("Stopping gnome attack mode");
                        ReflectionUtils.InvokeMethod(thisGnome, "UpdateState", new object[] { EnemyGnome.State.Idle });
                    }
                    else if (ConfigVariables.allowAttackToggle)
                    {
                        mls.LogInfo("Starting gnome attack mode");
                        ReflectionUtils.InvokeMethod(thisGnome, "UpdateState", new object[] { EnemyGnome.State.Attack });
                    }
                }
            }
            catch { }
        }  
    }
}
