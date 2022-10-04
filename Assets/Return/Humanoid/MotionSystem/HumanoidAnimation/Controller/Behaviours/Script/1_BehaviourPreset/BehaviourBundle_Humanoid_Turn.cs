using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.Humanoid.Animation
{
    [CreateAssetMenu(fileName = "TurnMotionBehaviour", menuName = "MyData/Animation/Bundle/Humanoid_Turn", order = 0)]
    public class BehaviourBundle_Humanoid_Turn : AnimationBehaviourBundle
    {
        public override void LoadBehaviour(IAdvanceAnimator animator, out IAnimationBehaviour playableBehaviour)
        {
            var behaviour = new Behaviour_Jump(this);
            behaviour.Init(animator);
            playableBehaviour = behaviour;
        }
    }

}
