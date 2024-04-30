using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CloudsRenderFeature : ScriptableRendererFeature
{
    class CloudsRenderPass : ScriptableRenderPass
    {
        private Material material;
        private RTHandle renderTexture;
        private CloudsSettings settings;

        public CloudsRenderPass(Material mat)
        {
            material = mat;
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public void Setup(RTHandle h, CloudsSettings s)
        {
            renderTexture = h;
            settings = s;
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(name: "CloudsRenderPass");
            
            material.SetTexture("_ShapeNoise", settings.cloudShapeTexture);
            material.SetColor("_CloudColor", settings.cloudColor);
            
            material.SetFloat("_NumSteps", settings.steps);
            material.SetFloat("_CloudScale", settings.cloudScale);
            material.SetFloat("_CloudSmooth", settings.cloudSmooth);
            
            material.SetVector("_ContainerBoundsMin", settings.containerBoundsMin);
            material.SetVector("_ContainerBoundsMax", settings.containerBoundsMax);
            material.SetFloat("_ContainerEdgeFadeDst", settings.containerEdgeFadeDst);
            
            material.SetVector("_Wind", settings.wind);
            material.SetFloat("_DensityThreshold", settings.densityThreshold);
            material.SetFloat("_DensityMultiplier", settings.densityMultiplier);
            
            Blitter.BlitCameraTexture(cmd, renderTexture, renderTexture, material, 0);
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    public class CloudsSettings
    {
        public Texture2D cloudShapeTexture;
        public Color cloudColor;
        public int steps = 10;
        public float cloudScale = 1.0f;
        public float cloudSmooth = 5.0f;

        public Vector3 containerBoundsMin;
        public Vector3 containerBoundsMax;
        public float containerEdgeFadeDst = 45.0f;
    
        public Vector3 wind = new Vector3(1, 0, 0);
        public float densityThreshold = 0.25f;
        public float densityMultiplier = 1;

    }

    public Material material;
    
    public Texture2D cloudShapeTexture;
    public Color cloudColor;
    public int steps = 10;
    public float cloudScale = 1.0f;
    public float cloudSmooth = 5.0f;

    public Vector3 containerBoundsMin;
    public Vector3 containerBoundsMax;
    public float containerEdgeFadeDst = 45.0f;
    
    public Vector3 wind = new Vector3(1, 0, 0);
    public float densityThreshold = 0.25f;
    public float densityMultiplier = 1;
    
    private CloudsRenderPass pass;
    
    public override void Create()
    {
        // material = CoreUtils.CreateEngineMaterial(shader);
        pass = new CloudsRenderPass(material);
    }
    
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        CloudsSettings settings = new CloudsSettings();
        settings.cloudShapeTexture = cloudShapeTexture;
        settings.cloudColor = cloudColor;
        settings.steps = steps;
        settings.cloudScale = cloudScale;
        settings.cloudSmooth = cloudSmooth;
        settings.containerBoundsMin = containerBoundsMin;
        settings.containerBoundsMax = containerBoundsMax;
        settings.containerEdgeFadeDst = containerEdgeFadeDst;
        settings.wind = wind;
        settings.densityThreshold = densityThreshold;
        settings.densityMultiplier = densityMultiplier;
            
        pass.ConfigureInput(ScriptableRenderPassInput.Color);
        pass.Setup(renderer.cameraColorTargetHandle, settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material != null)
        {
            renderer.EnqueuePass(pass);
        }
    }
    
    protected override void Dispose(bool disposing)
    {
        // CoreUtils.Destroy(material);
    }
}
