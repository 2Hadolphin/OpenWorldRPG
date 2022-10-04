using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Return.Humanoid.Animation
{
    [CreateAssetMenu(fileName = "JumpMotionBehaviour", menuName = "MyData/Animation/Bundle/Humanoid_Jump", order = 0)]
    public class BehaviourBundle_Humanoid_Jump : AnimationBehaviourBundle
    {
        public override void LoadBehaviour(IAdvanceAnimator animator, out IAnimationBehaviour playableBehaviour)
        {
            var behaviour = new Behaviour_Jump(this);
            behaviour.Init(animator);
            playableBehaviour = behaviour;
        }
    }

}

