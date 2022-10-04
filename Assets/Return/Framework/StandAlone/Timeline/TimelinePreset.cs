using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Return
{
    /// <summary>
    /// Data of Timeline asset bundle.
    /// </summary>
    [Serializable]
    public abstract class TimelinePreset : PresetDatabase
    {
        public abstract TimelineAsset GetAsset();

        public virtual Playable Create(PlayableGraph graph,GameObject go)
        {
            return GetAsset().CreatePlayable(graph, go);
        }
    }
}