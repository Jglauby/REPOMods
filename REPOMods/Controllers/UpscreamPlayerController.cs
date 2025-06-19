using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class UpscreamPlayerController : EnemyControllerBase
    {
        public EnemyUpscream thisUpscream = null;

        public void Setup(int actorNumber, EnemyUpscream upscream)
        {
            thisUpscream = upscream;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(upscream, "enemy");
            base.OnSetup(actorNumber, upscream.gameObject, enemy, upscream.transform);
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
