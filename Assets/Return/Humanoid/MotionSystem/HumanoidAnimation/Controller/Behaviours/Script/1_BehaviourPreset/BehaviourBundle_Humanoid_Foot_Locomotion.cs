using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Sirenix.OdinInspector;

namespace Return.Humanoid.Animation
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "FootLocomotionBehaviour", menuName = "MyData/Animation/Bundle/Humanoid_Foot_Locomotion", order = 0)]
    public class BehaviourBundle_Humanoid_Foot_Locomotion : AnimationBehaviourBundle
    {
        public AnimationLibrary_Vector3 Library;

        public override void LoadBehaviour(IAdvanceAnimator animator, out IAnimationBehaviour humanoidBehaviour)
        {
            var behaviour = new FootLocomotionBehaviour(this);
            behaviour.library = Library;
            behaviour.Init(animator);
            humanoidBehaviour = behaviour;
            humanoidBehaviour.Mask = HumanoidMask;
        }
        
    }

    public class HumanoidHand_Solver : AnimationBehaviourBundle
    {
        public override void LoadBehaviour(IAdvanceAnimator animator, out IAnimationBehaviour humanoidBehaviour)
        {
            var behaviour = new FootLocomotionBehaviour(this);
            behaviour.Init(animator);

            humanoidBehaviour = behaviour;
            humanoidBehaviour.Mask = HumanoidMask;
        }

    }

    public class HumanoidBody_InertiaSytem : AnimationBehaviourBundle
    {
        public override void LoadBehaviour(IAdvanceAnimator animator, out IAnimationBehaviour humanoidBehaviour)
        {
            var behaviour = new FootLocomotionBehaviour(this);
            behaviour.Init(animator);

            humanoidBehaviour = behaviour;
            humanoidBehaviour.Mask = HumanoidMask;
        }

    }

}
