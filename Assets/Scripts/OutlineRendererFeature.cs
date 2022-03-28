using UnityEngine;
using UnityEngine.Rendering.Universal;

public class OutlineRendererFeature : ScriptableRendererFeature
{
	public OutlineEffectResources resources;
	public OutlineEffectSettings settings;
	public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;

	private OutlineRenderPass _outlinePass;
	private string _featureName;

	public string FeatureName => _featureName;

	public override void Create()
	{
		_featureName = OutlineEffectResources.EffectName;

		_outlinePass = new OutlineRenderPass(this)
		{
			renderPassEvent = this.renderPassEvent
		};
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (resources != null && resources.HasShaders)
		{
			_outlinePass.Setup(renderer);
			renderer.EnqueuePass(_outlinePass);
		}
	}
}
