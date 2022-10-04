using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoaxGames;


namespace Return.Humanoid.Motion
{
    public class StepHandler:BaseComponent
    {
        Transform leftFoot;
        Transform rightFoot;
        private void Start()
        {
            if(TryGetComponent<Animator>(out var anim))
            {
                leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
                rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
            }
        }
        [Range(0.1f,10f)]
        [SerializeField]
        float time=3f;

        public virtual void OnLeftFootStepStart(IKResult result)
        {
            Debug.Log("left "+result.surfacePoint);
            //Debug.DrawRay(result.gluedIKPos, result.normal, Color.red, time);
        }

        public virtual void OnRightFootStepStart(IKResult result)
        {
            Debug.Log("right "+result.surfacePoint);
            //Debug.DrawRay(result.gluedIKPos, result.normal, Color.yellow, time);
        }

        public virtual void OnLeftFootStepStop()
        {
            Debug.Log("left stop");
            if(leftFoot)
                Debug.DrawRay(leftFoot.position, transform.up, Color.cyan, time);
        }

        public virtual void OnRightFootStepStop()
        {
            Debug.Log("right stop");
            if (rightFoot)
                Debug.DrawRay(rightFoot.position, transform.up, Color.blue, time);
        }

        public virtual void OnStepGrounded(GroundedResult result)
        {
            Debug.Log("Grounded "+result.isValid);
        }

        public virtual void OnStepUngrounded()
        {
            Debug.Log(nameof(OnStepUngrounded));
        }
    }
}