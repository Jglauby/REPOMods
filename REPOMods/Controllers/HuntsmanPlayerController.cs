using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;

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
        }  
    }
}
