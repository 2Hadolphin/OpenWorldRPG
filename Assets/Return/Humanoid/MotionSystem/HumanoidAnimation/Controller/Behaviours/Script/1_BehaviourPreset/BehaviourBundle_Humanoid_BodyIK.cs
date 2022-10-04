using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.Humanoid.Animation
{
    [CreateAssetMenu(fileName = "BodyIKBehaviour", menuName = "MyData/Animation/Bundle/Humanoid_BodyIK")]
    public class BehaviourBundle_Humanoid_BodyIK : AnimationBehaviourBundle
    {
        public const string Path = "BodyIKBehaviour";
        public override void LoadBehaviour(IAdvanceAnimator animator, out IAnimationBehaviour humanoidBehaviour)
        {
            var behaviour = new Behaviour_BodyIK(this);
            behaviour.Init(animator);
            humanoidBehaviour = behaviour;
        }
    }
}