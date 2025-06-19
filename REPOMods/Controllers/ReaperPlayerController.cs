using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class ReaperPlayerController : EnemyControllerBase
    {
        public EnemyRunner thisRunner = null;

        public void Setup(int actorNumber, EnemyRunner runner)
        {
            thisRunner = runner;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(runner, "enemy");
            base.OnSetup(actorNumber, runner.gameObject, enemy, runner.transform);
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
