using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using REPOMods;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class ShadowChildPlayerController : EnemyControllerBase
    {
        public EnemyThinMan thisThinMan = null;

        public void Setup(int actorNumber, EnemyThinMan thinMan)
        {
            thisThinMan = thinMan;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(thinMan, "enemy");

            var specs = new EnemySpecs
            {
                MoveSpeed = 2.7f,
                TurnSpeed = 3f,
                JumpForce = 0.5f,
                AttackDelay = 0.1f
            };
            base.OnSetup(actorNumber, thinMan.gameObject, enemy, thinMan.transform, specs);
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
