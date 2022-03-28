
Shader "Hidden/OutlineEffect"
{
	HLSLINCLUDE

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		
		TEXTURE2D_X(_MainTex);
		SAMPLER(sampler_MainTex);
		float2 _MainTex_TexelSize;
		TEXTURE2D_X(_MaskTex);
		SAMPLER(sampler_MaskTex);
		
		float _Gauss[32];
		float4 _Color;
		int _Width;
		float _Intensity;

		struct Varyings
		{
			float4 positionCS : SV_POSITION;
			float2 uv : TEXCOORD0;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		struct Attributes
		{
			uint vertexID : SV_VertexID;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		Varyings VertexSimple(Attributes input)
		{
			Varyings output = (Varyings)0;

			UNITY_SETUP_INSTANCE_ID(input);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

			output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
			output.uv = GetFullScreenTriangleTexCoord(input.vertexID);

			return output;
		}

		float Gauss(float2 uv, float2 offset, int k)
		{
			return SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv + k * offset).r * _Gauss[abs(k)];
		}

		float Intensity(float2 uv, float2 offset)
		{
			float intensity = 0;

			for (int k = 1; k <= _Width; ++k)
			{
				intensity += Gauss(uv, offset, k);
				intensity += Gauss(uv, offset, -k);
			}

			return intensity;
		}

		float4 FragmentH(Varyings i) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

			float2 uv = UnityStereoTransformScreenSpaceTex(i.uv);
			float intensity = Intensity(uv, float2(_MainTex_TexelSize.x, 0));
			return float4(intensity, intensity, intensity, 1);
		}

		float4 FragmentV(Varyings i) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

			float2 uv = UnityStereoTransformScreenSpaceTex(i.uv);

			if (SAMPLE_TEXTURE2D_X(_MaskTex, sampler_MaskTex, uv).r > 0)
			{
				discard;
			}

			float intensity = Intensity(uv, float2(0, _MainTex_TexelSize.y)) * _Intensity;
			return float4(_Color.rgb, saturate(_Color.a * intensity));
		}

	ENDHLSL

	SubShader
	{
		Tags{"RenderPipeline" = "UniversalPipeline"}

		Cull Off
		ZWrite Off
		ZTest Always
		Lighting Off

		Pass
		{
			Name "HPass"

			HLSLPROGRAM

			#pragma target 3.5
			#pragma multi_compile_instancing
			#pragma vertex VertexSimple
			#pragma fragment FragmentH

			ENDHLSL
		}

		Pass
		{
			Name "VPass"
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM

			#pragma target 3.5
			#pragma multi_compile_instancing
			#pragma vertex VertexSimple
			#pragma fragment FragmentV

			ENDHLSL
		}
	}
}
