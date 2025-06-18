using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class ClownPlayerController : EnemyControllerBase
    {
        public EnemyBeamer thisBeamer = null;

        public void Setup(int actorNumber, EnemyBeamer beamer)
        {
            thisBeamer = beamer;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(beamer, "enemy");
            base.OnSetup(actorNumber, beamer.gameObject, enemy, beamer.transform);
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
