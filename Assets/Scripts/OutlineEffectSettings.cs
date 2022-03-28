using UnityEngine;

[System.Serializable]
public class OutlineEffectSettings
{
	[ColorUsage(false, true)] public Color color = Color.red + Color.yellow;
	[Range(1, 32)] public int width = 8;
	[Range(1, 64)] public float intensity = 1;
	public bool depthTesting = false;
	public LayerMask layerMask;
}
