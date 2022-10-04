using System;
using UnityEngine.Assertions;
using UnityEngine.Timeline;
using Return.Database;
using UnityEngine;

namespace Return
{

    /// <summary>
    /// Time line preset and event callback
    /// **AnimClips **??EventCallback **??Speed 
    /// </summary>
    [Serializable]
    public class PerformerPreset : TimelinePreset
    {
        //public AnimationType AnimationType;
        //public FireBlendMethod FireBlendMethod;

        [SerializeField]
        public TimelineAsset TimelinePreset;

        public override TimelineAsset GetAsset()
        {
            Assert.IsNotNull(TimelinePreset);
            return TimelinePreset;
        }
    }


}