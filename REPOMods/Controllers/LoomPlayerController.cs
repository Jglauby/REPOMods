using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using REPOMods;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class LoomPlayerController : EnemyControllerBase
    {
        public EnemyShadow thisLoom = null;

        public void Setup(int actorNumber, EnemyShadow loom)
        {
            thisLoom = loom;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(loom, "enemy");

            var specs = new EnemySpecs
            {
                MoveSpeed = 2.7f,
                TurnSpeed = 3f,
                JumpForce = 4f,
                AttackDelay = 0.1f
            };
            base.OnSetup(actorNumber, loom.gameObject, enemy, loom.transform, specs);
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
