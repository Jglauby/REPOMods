using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
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
            base.OnSetup(actorNumber, bang.gameObject, enemy, bang.transform);
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
                if (Keyboard.current[ConfigVariables.attackToggleKey].wasPressedThisFrame)
                {
                    ReflectionUtils.InvokeMethod(thisBang, "ExplodeRPC", new object[] { });
                }
            }
            catch { }
        }  
    }
}
