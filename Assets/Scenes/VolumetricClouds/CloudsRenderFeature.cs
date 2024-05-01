using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CloudsRenderFeature : ScriptableRendererFeature
{
    class CloudsRenderPass : ScriptableRenderPass
    {
        private Material material;
        private CloudsSettings materialSettings;

        private string profilerTag;
        
        private RenderTargetIdentifier source;
        private RenderTargetHandle tempTexture;
        
        public CloudsRenderPass(Material material, CloudsSettings settings, RenderPassEvent passEvent)
        {
            this.material = material;
            this.materialSettings = settings;

            this.renderPassEvent = passEvent;
            
            this.profilerTag = "Volumetric Clouds";
        }
        
        public void Setup(RenderTargetIdentifier source)
        {
            this.source = source;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor);
            ConfigureTarget(tempTexture.Identifier());
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
            cmd.Clear();

            if(material == null) return;

            try
            {
                materialSettings.BindMaterial(material);

                /* settings.material.SetFloat("_detailNoiseScale", Mathf.Abs(detailCloudSettings.DetailCloudScale));
                settings.material.SetVector("_detailNoiseWind", detailCloudSettings.DetailCloudWind);
                settings.material.SetTexture("_DetailNoise", detailCloudSettings.DetailCloudNoiseTexure);
                settings.material.SetFloat("_detailNoiseWeight", detailCloudSettings.detailCloudWeight);
                settings.material.SetTexture("_BlueNoise", blueNoiseSettings.BlueNoiseTexure);
                settings.material.SetFloat("_rayOffsetStrength", blueNoiseSettings.RayOffsetStrength); */

                cmd.Blit(source, tempTexture.Identifier());
                cmd.Blit(tempTexture.Identifier(), source, material, 0);

                context.ExecuteCommandBuffer(cmd);
            }
            catch
            {
                Debug.LogError("Error while executing clouds render pass!");
            }
            
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
    
    public Material material;
    public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingSkybox;
    public CloudsSettings settings;

    private CloudsRenderPass pass;
    private RenderTargetHandle renderTextureHandle;
    
    public override void Create()
    {
        pass = new CloudsRenderPass(material, settings, passEvent);
    }
    
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        var cameraColorTargetIdent = renderer.cameraColorTarget;
        pass.Setup(cameraColorTargetIdent);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}
