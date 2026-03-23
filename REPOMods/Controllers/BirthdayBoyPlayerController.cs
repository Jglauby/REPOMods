using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using REPOMods;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class BirthdayBoyPlayerController : EnemyControllerBase
    {
        public EnemyBirthdayBoy thisBoy = null;

        public void Setup(int actorNumber, EnemyBirthdayBoy boy)
        {
            thisBoy = boy;
            var enemy = ReflectionUtils.GetFieldValue<Enemy>(boy, "enemy");

            var specs = new EnemySpecs
            {
                MoveSpeed = 2.9f,
                TurnSpeed = 3f,
                JumpForce = 2f,
                AttackDelay = 0.1f
            };
            base.OnSetup(actorNumber, boy.gameObject, enemy, boy.transform, specs);
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
