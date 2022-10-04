using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Return.Items
{
    /// <summary>
    /// performer preset with posture adjust.
    /// </summary>
    public class ItemPerformerPreset : PerformerPreset
    {
        [SerializeField]
        [Tooltip("Whether timeline asset include camera animation.")]
        private bool m_useCameraAnimation=true;

        /// <summary>
        /// Return false when require procedural camera director.
        /// </summary>
        [System.Obsolete]
        public bool UseCameraAnimation { get => m_useCameraAnimation; set => m_useCameraAnimation = value; }



        public ItemPostureAdjustData AdjustData;

        public override Playable Create(PlayableGraph graph, GameObject go)
        {
            // inject adjust playable

            return base.Create(graph, go);
        }

    }
}