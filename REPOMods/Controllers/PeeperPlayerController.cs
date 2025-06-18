using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class PeeperPlayerController : EnemyControllerBase
    {
        public EnemyCeilingEye thisEye = null;

        public void Setup(int actorNumber, EnemyCeilingEye eye)
        {
            thisEye = eye;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(eye, "enemy");
            base.OnSetup(actorNumber, eye.gameObject, enemy, eye.transform);
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
