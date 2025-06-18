using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class SpewerPlayerController : EnemyControllerBase
    {
        public EnemySlowMouth thisMouth = null;

        public void Setup(int actorNumber, EnemySlowMouth mouth)
        {
            thisMouth = mouth;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(mouth, "enemy");
            base.OnSetup(actorNumber, mouth.gameObject, enemy, mouth.transform);
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
