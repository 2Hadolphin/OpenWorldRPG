using UnityEngine.Animations;

namespace Return.Animations
{
    public interface IAnimationStreamHandler: IAnimationStreamJob
    {
        TransformStreamHandle Handle { get; set; }
    }
}