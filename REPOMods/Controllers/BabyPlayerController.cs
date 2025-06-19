using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class BabyPlayerController : EnemyControllerBase
    {
        public EnemyValuableThrower thisBaby = null;
        private bool runUpdate = false;

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
            handleInput();
        }

        void FixedUpdate()
        {
            base.FixedUpdateLogic();

            var heldObject = ReflectionUtils.GetFieldValue<PhysGrabObject>(thisBaby, "valuableTarget");
            if (heldObject != null)
            {
                Vector3 midPoint = heldObject.midPoint;
                midPoint.y = heldObject.transform.position.y;
                Vector3 targetPos = thisBaby.pickupTarget.position;

                heldObject.OverrideZeroGravity();
                heldObject.OverrideMass(0.5f);
                heldObject.OverrideIndestructible();
                heldObject.OverrideBreakEffects(0.1f);

                Vector3 followForce = SemiFunc.PhysFollowPosition(midPoint, targetPos, heldObject.rb.velocity, 5f);
                heldObject.rb.AddForce(followForce * (5f * Time.fixedDeltaTime), ForceMode.Impulse);

                Vector3 torque = SemiFunc.PhysFollowRotation(heldObject.transform, thisBaby.pickupTarget.rotation, heldObject.rb, 0.5f);
                heldObject.rb.AddTorque(torque * (5f * Time.fixedDeltaTime), ForceMode.Impulse);
            }
        }

        private void handleInput()
        {
            if (controlActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)//dont listen to keys if not your duck
                return;

            try
            {
                if (Keyboard.current[ConfigVariables.attackButtonKey].wasPressedThisFrame && ConfigVariables.allowAttackToggle)
                {
                    if (thisBaby != null)
                    {
                        var target = ReflectionUtils.GetFieldValue<PhysGrabObject>(thisBaby, "valuableTarget");
                        if (target == null)
                        {
                            // Try to find and pick up a new object
                            Collider[] hits = Physics.OverlapSphere(thisEnemyGameObject.transform.position, 5f, LayerMask.GetMask("PhysGrabObject"));
                            foreach (var hit in hits)
                            {
                                var valObj = hit.GetComponentInParent<ValuableObject>();
                                if (valObj != null && valObj.volumeType <= ValuableVolume.Type.Big)
                                {
                                    target = ReflectionUtils.GetFieldValue<PhysGrabObject>(valObj, "physGrabObject");
                                    ReflectionUtils.SetFieldValue(thisBaby, "valuableTarget", target);

                                    // Attach target visually to hand
                                    target.OverrideZeroGravity();
                                    target.OverrideMass(0.5f);
                                    target.OverrideIndestructible();
                                    target.OverrideBreakEffects(0.1f);
                                    target.transform.position = thisBaby.pickupTarget.position;
                                    target.transform.rotation = thisBaby.pickupTarget.rotation;
                                    target.rb.velocity = Vector3.zero;
                                    target.rb.angularVelocity = Vector3.zero;

                                    break;
                                }
                            }
                        }
                        else
                        {
                            var playerTarget = ReflectionUtils.GetFieldValue<PlayerAvatar>(thisBaby, "playerTarget");
                            if (playerTarget != null)
                            {
                                foreach (var grabber in target.playerGrabbing.ToList())
                                {
                                    grabber.photonView?.RPC("ReleaseObjectRPC", RpcTarget.All, false, 0.1f);
                                }

                                Vector3 toTarget = playerTarget.PlayerVisionTarget.VisionTransform.position - target.centerPoint;
                                Vector3 forceDir = Vector3.Lerp(thisBaby.transform.forward, toTarget, 0.5f).normalized;
                                float force = Mathf.Min(20f * target.rb.mass, 100f);

                                target.ResetMass();
                                target.ResetIndestructible();
                                target.rb.AddForce(forceDir * force, ForceMode.Impulse);
                                target.rb.AddTorque(target.transform.right * 0.5f, ForceMode.Impulse);
                                PhysGrabObjectImpactDetector impactDetector = ReflectionUtils.GetFieldValue<PhysGrabObjectImpactDetector>(target, "impactDetector");
                                impactDetector.PlayerHurtMultiplier(5f, 2f);

                                ReflectionUtils.SetFieldValue(thisBaby, "valuableTarget", null);
                            }
                        }
                    }
                }
            }
            catch { }
        }  
    }
}
