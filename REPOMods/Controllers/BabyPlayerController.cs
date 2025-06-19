using OpJosModREPO.IAmEnemy;
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

        private void HandleInput()
        {
            if (controlActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
                return;

            try
            {
                if (Keyboard.current[ConfigVariables.attackButtonKey].wasPressedThisFrame && ConfigVariables.allowAttackToggle && thisBaby != null)
                {
                    var target = ReflectionUtils.GetFieldValue<PhysGrabObject>(thisBaby, "valuableTarget");

                    if (target == null)
                    {
                        Collider[] hits = Physics.OverlapSphere(thisEnemyGameObject.transform.position, 5f, LayerMask.GetMask("PhysGrabObject"));
                        foreach (var hit in hits)
                        {
                            var valObj = hit.GetComponentInParent<ValuableObject>();
                            if (valObj != null && valObj.volumeType <= ValuableVolume.Type.Big)
                            {
                                target = ReflectionUtils.GetFieldValue<PhysGrabObject>(valObj, "physGrabObject");
                                ReflectionUtils.SetFieldValue(thisBaby, "valuableTarget", target);

                                // Visual attach
                                target.OverrideZeroGravity();
                                target.OverrideMass(0.5f);
                                target.OverrideIndestructible();
                                target.OverrideBreakEffects(0.1f);
                                target.transform.position = thisBaby.pickupTarget.position;
                                target.transform.rotation = thisBaby.pickupTarget.rotation;
                                target.rb.velocity = Vector3.zero;
                                target.rb.angularVelocity = Vector3.zero;

                                // Try playing pickup animation
                                var anim = ReflectionUtils.GetFieldValue<EnemyValuableThrowerAnim>(thisBaby, "anim");
                                if (anim != null && anim.isActiveAndEnabled)
                                {
                                    ReflectionUtils.InvokeMethod(anim, "Pickup", new object[] { });
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        ReflectionUtils.InvokeMethod(thisBaby, "Throw", new object[] { });
                    }
                }
            }
            catch {}
        }
    }
}