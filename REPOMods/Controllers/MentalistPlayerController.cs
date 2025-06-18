using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class MentalistPlayerController : EnemyControllerBase
    {
        public EnemyFloater thisMentalist = null;

        public void Setup(int actorNumber, EnemyFloater mentalist)
        {
            thisMentalist = mentalist;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(mentalist, "enemy");
            base.OnSetup(actorNumber, mentalist.gameObject, enemy, mentalist.transform);
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
