using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using KinematicCharacterController;
using Return.Framework.PhysicController;

namespace Return
{
    public class KCCTeleport : BaseComponent
    {
        public KCCTeleport TeleportTo;

        public UnityAction<IControllerMotor> OnCharacterTeleport;

        public bool isBeingTeleportedTo { get; set; }

        private void OnTriggerEnter(Collider other)
        {
            if (!isBeingTeleportedTo)
            {
                var cc = other.GetComponent<IControllerMotor>();
                if (cc.NotNull())
                {
                    cc.SetPositionAndRotation(TeleportTo.transform.position, TeleportTo.transform.rotation);

                    if (OnCharacterTeleport != null)
                    {
                        OnCharacterTeleport(cc);
                    }

                    TeleportTo.isBeingTeleportedTo = true;
                }
            }

            isBeingTeleportedTo = false;
        }
    }
}
