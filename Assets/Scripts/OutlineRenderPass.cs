using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

internal class OutlineRenderPass : ScriptableRenderPass
{
	private readonly OutlineRendererFeature _feature;
	private readonly List<ShaderTagId> _shaderTags = new List<ShaderTagId>() { new ShaderTagId("UniversalForward") };

	private ScriptableRenderer _renderer;

	public OutlineRenderPass(OutlineRendererFeature feature)
	{
		_feature = feature;
	}

	public void Setup(ScriptableRenderer renderer)
	{
		_renderer = renderer;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		var resources = _feature.resources;
		var settings = _feature.settings;
		var camData = renderingData.cameraData;
		var depthTex = new RenderTargetIdentifier("_CameraDepthTexture");

		if (settings.layerMask != 0)
		{
			var drawSettings = CreateDrawingSettings(_shaderTags, ref renderingData, camData.defaultOpaqueSortFlags);
			drawSettings.overrideMaterial = resources.MaskMaterial;
			drawSettings.overrideMaterialPassIndex = 0;
			drawSettings.enableDynamicBatching = true;

			var cmd = CommandBufferPool.Get(_feature.FeatureName);
			var filter = new FilteringSettings(RenderQueueRange.all, settings.layerMask, 1);
			var renderState = new RenderStateBlock(RenderStateMask.Nothing);

			using (var outline = new OutlineEffectRenderer(cmd, resources, _renderer.cameraColorTarget, depthTex, camData.cameraTargetDescriptor))
			{
				outline.RenderMask(settings.depthTesting);
				context.ExecuteCommandBuffer(cmd);

				context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filter, ref renderState);

				cmd.Clear();
				outline.RenderOutline(settings);
			}

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
	}
}
