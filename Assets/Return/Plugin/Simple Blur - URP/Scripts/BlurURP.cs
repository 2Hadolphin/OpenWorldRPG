using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SimpleBlurURP
{
    /// <summary>
    /// Blur settings for UniversalRenderPipelineAsset_Renderer
    /// </summary>
    public class BlurURP : ScriptableRendererFeature
    {
        [SerializeField] private BlurSettings settings = new BlurSettings();

        /// <summary>
        /// Instance parameters
        /// </summary>
        [System.Serializable]
        public class BlurSettings
        {
            public Material blurMaterial;
            [Range(1, 25)] public int blurPasses = 1;
            [Range(1, 5)] public int downSample = 1;
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            public BlurType blurType = BlurType.Lit;

            public enum BlurType { Lit, Unlit }
        }

        /// <summary>
        /// Blurring and transferring data to the URP renderer
        /// </summary>
        private class CustomRenderPass : ScriptableRenderPass
        {
            public Material blurMaterial;
            public int passes;
            public int downsample;
            public string name;
            public string profilerTag;

            private readonly RenderTargetIdentifier[] renderTargetIdentifiers = new RenderTargetIdentifier[2];

            private RenderTargetIdentifier source;

            /// <summary>
            /// Installing the source
            /// </summary>
            /// <param name="source"></param>
            public void Setup(RTHandle source)
            {
                this.source = source;
            }

            /// <summary>
            /// Forming a blur area considering the width and height
            /// </summary>
            /// <param name="cmd"></param>
            /// <param name="cameraTextureDescriptor"></param>
            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                int width = cameraTextureDescriptor.width / downsample;
                int height = cameraTextureDescriptor.height / downsample;
                for (int i = 0; i < 2; i++)
                {
                    int id = Shader.PropertyToID("blurURP" + Random.Range(int.MinValue, int.MaxValue));
                    cmd.GetTemporaryRT(id, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                    renderTargetIdentifiers[i] = new RenderTargetIdentifier(id);
                    ConfigureTarget(renderTargetIdentifiers[i]);
                }
            }

            /// <summary>
            /// This is where the blur algorithm happens
            /// </summary>
            /// <param name="context"></param>
            /// <param name="renderingData"></param>
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
                RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
                opaqueDesc.depthBufferBits = 0;
                cmd.SetGlobalFloat("_offset", 1.5f);
                cmd.Blit(source, renderTargetIdentifiers[0], blurMaterial);
                for (int i = 0; i < passes; i++)
                {
                    cmd.SetGlobalFloat("_offset", 1.5f + i);
                    cmd.Blit(renderTargetIdentifiers[0], renderTargetIdentifiers[1], blurMaterial);
                    RenderTargetIdentifier rttmp = renderTargetIdentifiers[0];
                    renderTargetIdentifiers[0] = renderTargetIdentifiers[1];
                    renderTargetIdentifiers[1] = rttmp;
                }
                cmd.SetGlobalFloat("_offset", passes - 0.5f);
                cmd.Blit(renderTargetIdentifiers[0], renderTargetIdentifiers[1], blurMaterial);
                cmd.SetGlobalTexture(name, renderTargetIdentifiers[1]);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }
        }

        private CustomRenderPass scriptablePass;

        /// <summary>
        /// Adding URP rendering passes
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="renderingData"></param>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(scriptablePass);
        }

        /// <summary>
        /// Contains functionality from AddRenderPasses asset of previous versions of Unity to eliminate the bug with the display of blurring
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="renderingData"></param>
        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            scriptablePass.Setup(renderer.cameraColorTargetHandle);
        }

        /// <summary>
        /// Setting parameters when creating an instance from the inspector
        /// </summary>
        public override void Create()
        {
            scriptablePass = new CustomRenderPass();
            if (settings.blurType == BlurSettings.BlurType.Lit)
                scriptablePass.name = "_LitBlurTexture";
            else if (settings.blurType == BlurSettings.BlurType.Unlit)
                scriptablePass.name = "_UnlitBlurTexture";
            scriptablePass.profilerTag = "BlurURP";
            scriptablePass.blurMaterial = settings.blurMaterial;
            scriptablePass.passes = settings.blurPasses;
            scriptablePass.downsample = settings.downSample;
            scriptablePass.renderPassEvent = settings.renderPassEvent;
        }
    }
}