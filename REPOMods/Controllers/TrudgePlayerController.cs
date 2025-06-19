using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class TrudgePlayerController : EnemyControllerBase
    {
        public EnemySlowWalker thisTrudge = null;

        public void Setup(int actorNumber, EnemySlowWalker trudge)
        {
            thisTrudge = trudge;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(trudge, "enemy");
            base.OnSetup(actorNumber, trudge.gameObject, enemy, trudge.transform);
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
        }  
    }
}
