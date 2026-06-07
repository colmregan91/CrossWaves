using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class Water_Volume : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        private Material _material;

        class PassData
        {
            public TextureHandle source;
            public Material material;
        }

        public CustomRenderPass(Material mat)
        {
            _material = mat;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_material == null) return;

            var cameraData   = frameData.Get<UniversalCameraData>();
            var resourceData = frameData.Get<UniversalResourceData>();

            if (cameraData.cameraType == CameraType.Reflection) return;

            TextureHandle src = resourceData.activeColorTexture;

            var desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            TextureHandle temp = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph, desc, "_TempWaterRT", false);

            // Pass 1: apply water material into temp
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Water_Volume_Apply", out var passData))
            {
                passData.source   = src;
                passData.material = _material;
                builder.UseTexture(src, AccessFlags.Read);
                builder.SetRenderAttachment(temp, 0, AccessFlags.Write);
                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                    Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0));
            }

            // Pass 2: copy temp back to camera color
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Water_Volume_Copy", out var passData))
            {
                passData.source   = temp;
                passData.material = null;
                builder.UseTexture(temp, AccessFlags.Read);
                builder.SetRenderAttachment(src, 0, AccessFlags.Write);
                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                    Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false));
            }
        }
    }

    [System.Serializable]
    public class _Settings
    {
        public Material material = null;
        public RenderPassEvent renderPass = RenderPassEvent.AfterRenderingSkybox;
    }

    public _Settings settings = new _Settings();
    private CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        if (settings.material == null)
            settings.material = (Material)Resources.Load("Water_Volume");

        m_ScriptablePass = new CustomRenderPass(settings.material);
        m_ScriptablePass.renderPassEvent = settings.renderPass;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
