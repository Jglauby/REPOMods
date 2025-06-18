using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class HeadManPlayerController : EnemyControllerBase
    {
        public EnemyHeadController thisHead = null;

        public void Setup(int actorNumber, EnemyHeadController head)
        {
            thisHead = head;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(head, "enemy");
            base.OnSetup(actorNumber, head.gameObject, enemy, head.transform);
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
        }  
    }
}
