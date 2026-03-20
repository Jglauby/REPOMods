using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Networking;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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
            HandleInput();
        }

        void FixedUpdate()
        {
            base.FixedUpdateLogic();

            var heldObject = ReflectionUtils.GetFieldValue<PhysGrabObject>(thisBaby, "valuableTarget");
            if (heldObject != null && PhotonNetwork.IsMasterClient)
            {
                ReflectionUtils.InvokeMethod(thisBaby, "ValuableTargetFollow", new object[] { });
            }
        }

        private void HandleInput()
        {
            if (controlActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
                return;

            try
            {
                if (Keyboard.current[ConfigVariables.attackButtonKey].wasPressedThisFrame && ConfigVariables.allowAttackToggle)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        TriggerPickupOrThrow();
                    }
                    else
                    {
                        EnemySpawnerNetwork.Instance.TriggerSpecialAttack(Vector3.zero, Vector3.zero, controlActorNumber);
                    }
                }
            }
            catch {}
        }

        public override void SpecialAttack(Vector3 pos, Vector3 angle)
        {
            TriggerPickupOrThrow();
        }

        public void TriggerPickupOrThrow()
        {
            var target = ReflectionUtils.GetFieldValue<PhysGrabObject>(thisBaby, "valuableTarget");
            if (target == null)
            {
                Collider[] hits = Physics.OverlapSphere(thisEnemyGameObject.transform.position, 2f, LayerMask.GetMask("PhysGrabObject"));
                foreach (var hit in hits)
                {
                    var valObj = hit.GetComponentInParent<ValuableObject>();
                    if (valObj != null && valObj.volumeType <= ValuableVolume.Type.Big)
                    {
                        target = ReflectionUtils.GetFieldValue<PhysGrabObject>(valObj, "physGrabObject");
                        ReflectionUtils.SetFieldValue(thisBaby, "valuableTarget", target);

                        target.OverrideZeroGravity();
                        target.OverrideMass(0.5f);
                        target.OverrideIndestructible();
                        target.OverrideBreakEffects(0.1f);
                        target.transform.position = thisBaby.pickupTarget.position;
                        target.transform.rotation = thisBaby.pickupTarget.rotation;
                        target.rb.velocity = Vector3.zero;
                        target.rb.angularVelocity = Vector3.zero;

                        ReflectionUtils.InvokeMethod(thisBaby, "UpdateState", new object[] { EnemyValuableThrower.State.PickUpTarget });
                        break;
                    }
                }
            }
            else
            {
                ReflectionUtils.InvokeMethod(thisBaby, "UpdateState", new object[] { EnemyValuableThrower.State.Throw });

                DelayUtility.RunAfterDelay(0.5f, () => {
                    Vector3 forwardDir = thisBaby.transform.forward;
                    Vector3 startPos = thisBaby.pickupTarget.position;
                    Vector3 targetSpot = startPos + forwardDir * 10f;
                    CustomThrowAtLocation(targetSpot);
                });

                DelayUtility.RunAfterDelay(0.75f, () => {
                    ReflectionUtils.InvokeMethod(thisBaby, "UpdateState", new object[] { EnemyValuableThrower.State.Idle });
                });
            }
        }

        private void CustomThrowAtLocation(Vector3 targetSpot)
        {
            var valuableTarget = ReflectionUtils.GetFieldValue<PhysGrabObject>(thisBaby, "valuableTarget");
            if (!valuableTarget)
            {
                return;
            }

            foreach (PhysGrabber item in valuableTarget.playerGrabbing.ToList())
            {
                if (!SemiFunc.IsMultiplayer())
                {
                    // Release locally with default values matching the RPC signature
                    item.ReleaseObjectRPC(false, 0.1f, 0);
                    continue;
                }

                item.photonView.RPC("ReleaseObjectRPC", RpcTarget.All, false, 0.1f);
            }

            Vector3 throwDir = targetSpot - valuableTarget.centerPoint;
            throwDir = Vector3.Lerp(thisBaby.transform.forward, throwDir, 0.5f);

            valuableTarget.ResetMass();
            float a = 20f * valuableTarget.rb.mass;
            a = Mathf.Min(a, 100f);
            valuableTarget.ResetIndestructible();
            valuableTarget.rb.AddForce(throwDir * a, ForceMode.Impulse);
            valuableTarget.rb.AddTorque(valuableTarget.transform.right * 0.5f, ForceMode.Impulse);
            PhysGrabObjectImpactDetector impactDetector = ReflectionUtils.GetFieldValue<PhysGrabObjectImpactDetector>(valuableTarget, "impactDetector");
            impactDetector.PlayerHurtMultiplier(5f, 2f);

            ReflectionUtils.SetFieldValue(thisBaby, "valuableTarget", null);
        }
    }
}