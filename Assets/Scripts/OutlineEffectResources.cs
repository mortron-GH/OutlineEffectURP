using System;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "NewOutlineEffectResources", menuName = "OutlineEffect/Resources")]
public class OutlineEffectResources : ScriptableObject
{
	[SerializeField] private Shader _maskShader;
	[SerializeField] private Shader _outlineShader;

	private Material _maskMaterial;
	private Material _outlineMaterial;
	private MaterialPropertyBlock _materialProps;

	private float[][] _gaussCache;


	public const string EffectName = "Outline";
	public const string MainTexName = "_MainTex";
	public const string MaskTexName = "_MaskTex";
	public const string TempTexName = "_TempTex";
	public const string ColorName = "_Color";
	public const string WidthName = "_Width";
	public const string IntensityName = "_Intensity";
	public const string GaussSamplesName = "_Gauss";


	public readonly RenderTargetIdentifier MainTex = new RenderTargetIdentifier(MainTexName);
	public readonly RenderTargetIdentifier MaskTex = new RenderTargetIdentifier(MaskTexName);
	public readonly RenderTargetIdentifier TempTex = new RenderTargetIdentifier(TempTexName);

	public readonly int MainTexId = Shader.PropertyToID(MainTexName);
	public readonly int MaskTexId = Shader.PropertyToID(MaskTexName);
	public readonly int TempTexId = Shader.PropertyToID(TempTexName);
	public readonly int ColorId = Shader.PropertyToID(ColorName);
	public readonly int WidthId = Shader.PropertyToID(WidthName);
	public readonly int IntensityId = Shader.PropertyToID(IntensityName);
	public readonly int GaussSamplesId = Shader.PropertyToID(GaussSamplesName);

	public bool HasShaders => _maskShader && _outlineShader;

	public Material MaskMaterial
	{
		get
		{
			if (_maskMaterial == null)
			{
				_maskMaterial = new Material(_maskShader);
			}

			return _maskMaterial;
		}
	}

	public Material OutlineMaterial
	{
		get
		{
			if (_outlineMaterial == null)
			{
				_outlineMaterial = new Material(_outlineShader);
			}

			return _outlineMaterial;
		}
	}

	public MaterialPropertyBlock GetMaterialPropertyBlock(OutlineEffectSettings settings)
	{
		if (_materialProps == null)
		{
			_materialProps = new MaterialPropertyBlock();
		}

		_materialProps.SetFloat(WidthId, settings.width);
		_materialProps.SetColor(ColorId, settings.color);
		_materialProps.SetFloat(IntensityId, settings.intensity);

		return _materialProps;
	}

	public float[] GetGaussSamples(int width)
	{
		var idx = Mathf.Clamp(width, 1, 32) - 1;

		if (_gaussCache == null)
		{
			_gaussCache = new float[32][];
		}

		if (_gaussCache[idx] == null)
		{
			_gaussCache[idx] = CacheGauss(width);
		}

		return _gaussCache[idx];
	}

	float[] CacheGauss(int width)
	{
		var s = width * 0.5f;
		var result = new float[32];

		for (var i = 0; i < width; i++)
		{
			result[i] = Gauss(i, s);
		}

		return result;
	}

	float Gauss(float x, float s)
	{
		var s2 = s * s * 2;
		var a = 1 / Mathf.Sqrt(Mathf.PI * s2);
		var gauss = a * Mathf.Pow((float)Math.E, -x * x / s2);

		return gauss;
	}
}
