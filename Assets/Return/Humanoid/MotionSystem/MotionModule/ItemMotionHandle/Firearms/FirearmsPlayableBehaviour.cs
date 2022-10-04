using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Collections.Generic;

public class FirearmsPlayableBehaviour : BasePlayableBehaviour
{
    AnimationClipPlayable AnimationClipPlayable;

    public Dictionary<int, float> AnimSpeedBinding;

    public void PlayClip(AnimationClip clip, float speed = 1f)
    {
        if (Self.HasInput(AnimationClipPlayable))
            AnimationClipPlayable.Destroy();


        AnimationClipPlayable = AnimationClipPlayable.Create(m_Graph, clip);

        if (Self.GetInputCount() > 0)
        {
            Self.ConnectInput(0, AnimationClipPlayable, 0, 1);
        }
        else
            Self.AddInput(AnimationClipPlayable, 0, 1);

        AnimationClipPlayable.Play();

        AnimationClipPlayable.SetTime(0);
        AnimationClipPlayable.SetSpeed(speed);
        AnimationClipPlayable.SetApplyPlayableIK(true);

    }

}
