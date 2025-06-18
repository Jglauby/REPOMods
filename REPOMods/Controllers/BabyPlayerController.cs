using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class BabyPlayerController : EnemyControllerBase
    {
        public EnemyValuableThrower thisBaby = null;

        public void Setup(int actorNumber, EnemyValuableThrower baby)
        {
            thisBaby = baby;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(baby, "enemy");
            base.OnSetup(actorNumber, baby.gameObject, enemy, baby.transform);
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
