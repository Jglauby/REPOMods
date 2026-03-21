using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using REPOMods;
using UnityEngine.InputSystem;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class BangerPlayerController : EnemyControllerBase
    {
        public EnemyBang thisBang = null;

        public void Setup(int actorNumber, EnemyBang bang)
        {
            thisBang = bang;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(bang, "enemy");

            var specs = new EnemySpecs
            {
                MoveSpeed = 2.7f,
                TurnSpeed = 3f,
                JumpForce = 0.5f,
                AttackDelay = 0.1f
            };
            base.OnSetup(actorNumber, bang.gameObject, enemy, bang.transform, specs);
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
                if (Keyboard.current[ConfigVariables.attackButtonKey].wasPressedThisFrame)
                {
                    DelayUtility.RunAfterDelay(attackDelay, () =>
                    {
                        ReflectionUtils.InvokeMethod(thisBang, "ExplodeRPC", new object[] { });
                    });
                }
            }
            catch { }
        }  
    }
}
