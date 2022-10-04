using UnityEngine;
using System;

namespace Return.Humanoid
{
    /// <summary>
    /// Provide animator component and OnAnimatorIK event.
    /// </summary>
    public interface IAnimator
    {
        Animator Animator { get; }
        event Action<int> OnAnimatorIKPhase;
    }
}
