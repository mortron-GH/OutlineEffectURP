using System;
using UnityEngine;
using UnityEngine.Rendering;

public readonly struct OutlineEffectRenderer : IDisposable
{
	private readonly TextureDimension _rtDimension;
	private readonly RenderTargetIdentifier _rt;
	private readonly RenderTargetIdentifier _depth;
	private readonly CommandBuffer _cmdBuffer;
	private readonly OutlineEffectResources _resources;

	public const CameraEvent RenderEvent = CameraEvent.AfterSkybox;
	public const RenderTextureFormat RtFormat = RenderTextureFormat.R8;

	public OutlineEffectRenderer(CommandBuffer cmd, OutlineEffectResources resources, RenderTargetIdentifier dst, RenderTargetIdentifier depth, RenderTextureDescriptor rtDesc)
	{
		rtDesc.colorFormat = RtFormat;
		rtDesc.depthBufferBits = 0;
		rtDesc.msaaSamples = 1;
		rtDesc.shadowSamplingMode = ShadowSamplingMode.None;

		cmd.GetTemporaryRT(resources.MaskTexId, rtDesc, FilterMode.Bilinear);
		cmd.GetTemporaryRT(resources.TempTexId, rtDesc, FilterMode.Bilinear);

		_rtDimension = rtDesc.dimension;
		_rt = dst;
		_depth = depth;
		_cmdBuffer = cmd;
		_resources = resources;
	}

	// IDisposable
	public void Dispose()
	{
		_cmdBuffer.ReleaseTemporaryRT(_resources.TempTexId);
		_cmdBuffer.ReleaseTemporaryRT(_resources.MaskTexId);
	}

	public void RenderMask(bool depthTest = false)
	{
		if (depthTest)
		{
			if (_rtDimension == TextureDimension.Tex2DArray)// XR
			{
				_cmdBuffer.SetRenderTarget(_resources.MaskTex, _depth, 0, CubemapFace.Unknown, -1);
			}
			else
			{
				_cmdBuffer.SetRenderTarget(_resources.MaskTex, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, _depth, RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare);
			}
		}
		else
		{
			if (_rtDimension == TextureDimension.Tex2DArray)
			{
				_cmdBuffer.SetRenderTarget(_resources.MaskTex, 0, CubemapFace.Unknown, -1);
			}
			else
			{
				_cmdBuffer.SetRenderTarget(_resources.MaskTex, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
			}
		}

		_cmdBuffer.ClearRenderTarget(false, true, Color.clear);
	}

	public void RenderOutline(OutlineEffectSettings settings)
	{
		var mat = _resources.OutlineMaterial;
		var props = _resources.GetMaterialPropertyBlock(settings);

		_cmdBuffer.SetGlobalFloatArray(_resources.GaussSamplesId, _resources.GetGaussSamples(settings.width));

		if (_rtDimension == TextureDimension.Tex2DArray)
		{
			_cmdBuffer.SetRenderTarget(_resources.TempTex, 0, CubemapFace.Unknown, -1);
			Blit(_resources.MaskTex, 0, mat, props);

			_cmdBuffer.SetRenderTarget(_rt, 0, CubemapFace.Unknown, -1);
			Blit(_resources.TempTex, 1, mat, props);
		}
		else
		{
			_cmdBuffer.SetRenderTarget(_resources.TempTex, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
			Blit(_resources.MaskTex, 0, mat, props);

			_cmdBuffer.SetRenderTarget(_rt, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
			Blit(_resources.TempTex, 1, mat, props);
		}
	}

	private void Blit(RenderTargetIdentifier src, int shaderPass, Material mat, MaterialPropertyBlock props)
	{
		_cmdBuffer.SetGlobalTexture(_resources.MainTexId, src);
		_cmdBuffer.DrawProcedural(Matrix4x4.identity, mat, shaderPass, MeshTopology.Triangles, 3, 1, props);
	}
}
