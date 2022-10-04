using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-postprocesslayerfix.html")]
    public class PostProcessLayerFix : MonoBehaviour
    {
#if UNITY_POST_PROCESSING_STACK_V2

        private static PostProcessLayer.Antialiasing s_AntiAliasing = PostProcessLayer.Antialiasing.None;
        private static PostProcessResources s_Resources = null;

        protected void Awake()
        {
            var existing = GetComponent<PostProcessLayer>();
            if (existing == null)
            {
                // CheckAdd if resources have been set (first time a PPL is added)
                if (s_Resources == null)
                {
                    // Load and record the post processing settings
                    var ppls = Resources.Load<PostProcessLayerSettings>("PostProcessLayerSettings");
                    if (ppls != null)
                    {
                        // Add a post processing layer
                        var ppl = gameObject.AddComponent<PostProcessLayer>();
                        if (ppl == null)
                        {
                            // Bizarrely the above can return null so attempt to re-grab
                            ppl = gameObject.GetComponent<PostProcessLayer>();
                            if (ppl == null)
                                return;
                        }

                        // Set and record antialiasing
                        s_AntiAliasing = ppls.antiAliasing;
                        ppl.antialiasingMode = s_AntiAliasing;

                        // CheckAdd if resources is stored
                        if (ppls.resources == null)
                        {
                            // If not, get the layer to generate new resources via Init(null) and then retrieve via reflection
                            ppl.Init(null);
                            var t = ppl.GetType();
                            var field = t.GetField("m_Resources", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            s_Resources = field.GetValue(ppl) as PostProcessResources;

                            // Store the resources
                            ppls.resources = s_Resources;
                        }
                        else
                        {
                            // Get the resources from the settings and apply
                            s_Resources = ppls.resources;
                            ppl.Init(s_Resources);
                        }

                        // Set the volume settings
                        ppl.volumeTrigger = transform;
                        ppl.volumeLayer = PhysicsFilter.LayerFilter.PostProcessingVolumes;
                    }
                }
                else
                {
                    // Add a post processing layer
                    var ppl = gameObject.AddComponent<PostProcessLayer>();

                    // Set the properties based on stored
                    ppl.volumeLayer = PhysicsFilter.LayerFilter.PostProcessingVolumes;
                    ppl.volumeTrigger = transform;
                    ppl.antialiasingMode = s_AntiAliasing;
                    ppl.Init(s_Resources);
                }
            }
        }
#else
        protected void Awake()
        {
            Destroy(this);
        }
#endif
    }
}