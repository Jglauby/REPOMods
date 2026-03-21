using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using REPOMods;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class OoglyPlayerController : EnemyControllerBase
    {
        public EnemyOogly thisOogly = null;

        public void Setup(int actorNumber, EnemyOogly oogly)
        {
            thisOogly = oogly;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(oogly, "enemy");

            var specs = new EnemySpecs
            {
                MoveSpeed = 2.7f,
                TurnSpeed = 3f,
                JumpForce = 0.5f,
                AttackDelay = 0.1f
            };
            base.OnSetup(actorNumber, oogly.gameObject, enemy, oogly.transform, specs);
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
