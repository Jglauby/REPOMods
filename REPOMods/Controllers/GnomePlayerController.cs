using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class GnomePlayerController : EnemyControllerBase
    {
        public EnemyGnome thisGnome = null;

        public void Setup(int actorNumber, EnemyGnome gnome)
        {
            thisGnome = gnome;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(gnome, "enemy");
            base.OnSetup(actorNumber, gnome.gameObject, enemy, gnome.transform);
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
