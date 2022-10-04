using UnityEngine.Animations;

namespace Return.Animations
{
    public interface IAnimationStreamJob
    {
        void ProcessStream(ref AnimationStream stream);
    }
}