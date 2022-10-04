using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-so-postprocesslayersettings.html")]
    [CreateAssetMenu(fileName = "PostProcessLayerSettings", menuName = "NeoFPS/PostProcessing/PostProcessLayerSettings")]
    public class PostProcessLayerSettings : ScriptableObject
    {
#if UNITY_POST_PROCESSING_STACK_V2
        [SerializeField]
        private PostProcessLayer.Antialiasing m_AntiAliasing = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
        [SerializeField]
        private PostProcessResources m_Resources = null;

        public PostProcessLayer.Antialiasing antiAliasing
        {
            get { return m_AntiAliasing; }
        }

        public PostProcessResources resources
        {
            get { return m_Resources; }
            set { m_Resources = value; }
        }
#endif
    }
}