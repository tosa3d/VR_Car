Shader "Hidden / Pro Car Paint Transparent"
{
	Properties
	{
		[HDR]_Color("Paint (RGB)", Color) = (0.5,0.5,0.5,1)
		_Metallic("Metallic", Range( 0 , 1)) = 0.5
		_Glossiness("Smoothness", Range( 0 , 1)) = 0.5
		_Cutoff("Transparency", Range( 0 , 1)) = 0.5
		_Translucent("Translucent", Range( 0 , 0.1)) = 0.05
		_CutoffMap("Transparent (A)", 2D) = "white" {}
		_MetallicGlossMap("Metallic (R) Smoothness (G) Transparent (B) ", 2D) = "white" {}
		[HDR]_EmissionColor("Emission (RGB)", Color) = (0,0,0,0)
		_MainTex("Albedo (RGB) Paints (A)", 2D) = "black" {}
		_BumpScale("Albedo Normal Map Scale", Float) = 1
		[Normal]_BumpMap("Albedo Normal", 2D) = "bump" {}
		_1Decal("1 Decal ", 2D) = "black" {}
		[HDR]_1DecalColor("1 Decal Color", Color) = (1,1,1,1)
		_2Decal("2 Decal ", 2D) = "black" {}
		[HDR]_2DecalColor("2 Decal Color ", Color) = (1,1,1,1)
		_3Decal("3 Decal ", 2D) = "black" {}
		[HDR]_3DecalColor("3 Decal Color ", Color) = (1,1,1,1)
		_4Decal("4 Decal ", 2D) = "black" {}
		[HDR]_4DecalColor("4 Decal Color ", Color) = (1,1,1,1)
		_5Decal("5 Decal ", 2D) = "black" {}
		[HDR]_5DecalColor("5 Decal Color ", Color) = (1,1,1,1)
		_ReflectionCubeMap("Reflection CubeMap", CUBE) = "black" {}
		_ReflectionIntensity("Reflection Intensity", Range( 0 , 10)) = 1
		_ReflectionExponent("Reflection Exponent", Range( 0 , 10)) = 1
		_ReflectionBlur("Reflection Blur", Range( 0 , 8)) = 1
		_ReflectionStrength("Reflection Strength", Range( 0 , 1)) = 0.5
		[Toggle]_ReflectionColorOverride("Reflection ColorOverride", Float) = 1
		[HDR]_ReflectionColor("Reflection Color", Color) = (1,1,1,1)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Transparent" "IgnoreProjector" = "True" "IsEmissive" = "true" }
		LOD 200
		Cull Back
		AlphaToMask On
		GrabPass{ }
		CGPROGRAM
		#include "UnityPBSLighting.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 4.0
		#pragma multi_compile __ CLEARCOAT_UNDER
		#pragma multi_compile __ DECAL
		#pragma multi_compile __ CLEARCOAT
		#pragma multi_compile __ BLUR
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#pragma surface surf StandardCustomLighting keepalpha addshadow fullforwardshadows
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldRefl;
			float3 worldPos;
			float4 screenPos;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform sampler2D _MetallicGlossMap;
		uniform float4 _MetallicGlossMap_ST;
		uniform float _ReflectionColorOverride;
		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float4 _ReflectionColor;
		uniform samplerCUBE _ReflectionCubeMap;
		uniform half _ReflectionBlur;
		uniform float _ReflectionIntensity;
		uniform float _ReflectionExponent;
		uniform float _ReflectionStrength;
		uniform sampler2D _5Decal;
		uniform float4 _5Decal_ST;
		uniform sampler2D _1Decal;
		uniform float4 _1Decal_ST;
		uniform sampler2D _2Decal;
		uniform float4 _2Decal_ST;
		uniform sampler2D _3Decal;
		uniform float4 _3Decal_ST;
		uniform sampler2D _4Decal;
		uniform float4 _4Decal_ST;
		uniform float4 _1DecalColor;
		uniform float4 _2DecalColor;
		uniform float4 _3DecalColor;
		uniform float4 _4DecalColor;
		uniform float4 _5DecalColor;
		uniform float _BumpScale;
		uniform sampler2D _BumpMap;
		uniform float4 _BumpMap_ST;
		uniform float4 _EmissionColor;
		uniform float _Metallic;
		uniform float _Glossiness;
		DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform float _Translucent;
		uniform float _Cutoff;
		uniform sampler2D _CutoffMap;
		uniform float4 _CutoffMap_ST;

		float3 HSVToRGB( float3 x )
		{
			float4 a = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 b = abs( frac( x.xxx + a.xyz ) * 6.0 - a.www );
			return x.z * lerp( a.xxx, saturate( b - a.xxx ), x.y );
		}

		float3 RGBToHSV( float3 x )
		{
			float4 a = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 b = lerp( float4( x.bg, a.wz ), float4( x.gb, a.xy ), step( x.b, x.g ) );
			float4 c = lerp( float4( b.xyw, x.r ), float4( x.r, b.yzx ), step( b.x, x.r ) );
			float d = c.x - min( c.w, c.y );
			float e = 1.0e-10;
			return float3( abs(c.z + (c.w - c.y) / (6.0 * d + e)), d / (c.x + e), c.x);
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			SurfaceOutputStandard s1265 = (SurfaceOutputStandard ) 0;
			float2 uv_MetallicGlossMap = i.uv_texcoord * _MetallicGlossMap_ST.xy + _MetallicGlossMap_ST.zw;
			float4 tex2D605 = tex2D( _MetallicGlossMap, uv_MetallicGlossMap );
			float GlobalEmission1395 = tex2D605.b;
			float GlobalMetallic1394 = tex2D605.r;
			float GlobalSmoothness1364 = tex2D605.g;
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2D1344 = tex2D( _MainTex, uv_MainTex );
			float4 Color1347 = lerp( _Color , tex2D1344 , tex2D1344.a);
			#ifdef CLEARCOAT
			float3 hsvTorgb4_g36 = RGBToHSV( Color1347.rgb );
			float clamp7_g36 = clamp( ( hsvTorgb4_g36.z + 0.15 ) , 0.15 , 1 );
			float3 hsvTorgb12_g36 = HSVToRGB( float3(hsvTorgb4_g36.x,hsvTorgb4_g36.y,clamp7_g36) );
			float3 worldReflection = normalize( WorldReflectionVector( i, float3( 0, 0, 1 ) ) );
			float3 worldViewDir = normalize( UnityWorldSpaceViewDir( i.worldPos ) );
			float3 worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float fresnelDot = dot( worldNormal, worldViewDir );
			float fresnel14_g36 = _ReflectionIntensity * pow( 1.0 - fresnelDot, _ReflectionExponent );
			float3 Reflection1350 = ( _ReflectionColorOverride ?  _ReflectionColor : hsvTorgb12_g36 ) * texCUBElod( _ReflectionCubeMap, float4( worldReflection, _ReflectionBlur) ) * fresnel14_g36;
			float fresnel61_g36 = _ReflectionStrength * pow( 1.0 - fresnelDot, _ReflectionExponent / 2.5 );
			float ReflectionMask1351 = saturate( fresnel61_g36 * _ReflectionStrength );
			float3 BaseLayer1354 = lerp( Color1347 , Reflection1350 , ReflectionMask1351);
			#else
			float3 BaseLayer1354 = Color1347;
			#endif
			#ifdef DECAL
			#ifdef CLEARCOAT
			#ifdef CLEARCOAT_UNDER
			float3 BD = lerp( GlobalSmoothness1364 * Reflection1350 + BaseLayer1354 , Reflection1350 , ReflectionMask1351);
			#else
			float3 BD = BaseLayer1354;
			#endif
			#else
			float3 BD = BaseLayer1354;
			#endif
			#else
			#ifdef CLEARCOAT
			float3 BD = lerp( GlobalSmoothness1364 * Reflection1350 + BaseLayer1354 , Reflection1350 , ReflectionMask1351);
			#else
			float3 BD = BaseLayer1354;
			#endif
			#endif
			#ifdef DECAL
			float2 uv_5Decal = i.uv_texcoord * _5Decal_ST.xy + _5Decal_ST.zw;
			float4 tex2D44_g39 = tex2D( _5Decal, uv_5Decal );
			float2 uv_1Decal = i.uv_texcoord * _1Decal_ST.xy + _1Decal_ST.zw;
			float4 tex2D47_g39 = tex2D( _1Decal, uv_1Decal );
			float2 uv_2Decal = i.uv_texcoord * _2Decal_ST.xy + _2Decal_ST.zw;
			float4 tex2D52_g39 = tex2D( _2Decal, uv_2Decal );
			float2 uv_3Decal = i.uv_texcoord * _3Decal_ST.xy + _3Decal_ST.zw;
			float4 tex2D49_g39 = tex2D( _3Decal, uv_3Decal );
			float2 uv_4Decal = i.uv_texcoord * _4Decal_ST.xy + _4Decal_ST.zw;
			float4 tex2D55_g39 = tex2D( _4Decal, uv_4Decal );
			float3 AD = lerp( lerp( lerp( lerp( lerp( BD , _1DecalColor * tex2D47_g39 ,  tex2D47_g39.a ) , _2DecalColor * tex2D52_g39 ,  tex2D52_g39.a ) , _3DecalColor * tex2D49_g39 ,  tex2D49_g39.a ) , _4DecalColor * tex2D55_g39 ,  tex2D55_g39.a ), _5DecalColor * tex2D44_g39 , tex2D44_g39.a );
			#else
			float3 AD = BD;
			#endif
			#ifdef DECAL
			#ifdef CLEARCOAT
			#ifdef CLEARCOAT_UNDER
			float3 Final_Albedo = AD;
			#else
			float3 Final_Albedo = lerp( ( ( GlobalSmoothness1364 * Reflection1350 ) + AD ) , Reflection1350 , (ReflectionMask1351).xxxx);
			#endif
			#else
			float3 Final_Albedo = AD;
			#endif
			#else
			float3 Final_Albedo = AD;
			#endif
			s1265.Albedo = Final_Albedo;
			s1265.Normal = WorldNormalVector( i , UnpackScaleNormal( tex2D( _BumpMap, i.uv_texcoord * _BumpMap_ST.xy + _BumpMap_ST.zw ), _BumpScale ) );
			s1265.Emission = GlobalEmission1395 * _EmissionColor ;
			s1265.Metallic = GlobalMetallic1394 * _Metallic;
			s1265.Smoothness = GlobalSmoothness1364 * _Glossiness;
			s1265.Occlusion = 1.0;
			data.light = gi.light;
			UnityGI gi1265 = gi;
			#ifdef UNITY_PASS_FORWARDBASE
			Unity_GlossyEnvironmentData g1265 = UnityGlossyEnvironmentSetup( s1265.Smoothness, data.worldViewDir, s1265.Normal, float3(0,0,0));
			gi1265 = UnityGlobalIllumination( data, s1265.Occlusion, s1265.Normal, g1265 );
			#endif
			float3 surf1265 = LightingStandard ( s1265, viewDir, gi1265 ).rgb;
			surf1265 += s1265.Emission;
			#ifdef UNITY_PASS_FORWARDADD
			surf1265 -= s1265.Emission;
			#endif
			float4 screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 screenPosNorm = screenPos / screenPos.w;
			screenPosNorm.z = UNITY_NEAR_CLIP_VALUE >= 0 ? screenPosNorm.z : screenPosNorm.z * 0.5 + 0.5;
			float4 screenColor22_g42 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,screenPosNorm.xy/screenPosNorm.w);
			#ifdef BLUR
			float temp_output_3_0_g42 = distance( unity_WorldTransformParams , float4( _WorldSpaceCameraPos , 0 ) );
			float myVarName08_g42 = _Translucent / ( temp_output_3_0_g42 / log10( temp_output_3_0_g42 ) );
			float2 append13_g42 = float2(0 , myVarName08_g42);
			float2 append11_g42 = float2(myVarName08_g42 , 0);
			float2 append17_g42 = float2(myVarName08_g42 , myVarName08_g42);
			float2 append18_g42 = float2(myVarName08_g42 , -myVarName08_g42);
			float2 append12_g42 = float2(-myVarName08_g42 , myVarName08_g42);
			float4 screenColor32_g42 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( screenPosNorm + float4( append13_g42, 0 , 0 ) ).xy/( screenPosNorm + float4( append13_g42, 0 , 0 ) ).w);
			float4 screenColor35_g42 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( screenPosNorm + float4( append11_g42, 0 , 0 ) ).xy/( screenPosNorm + float4( append11_g42, 0 , 0 ) ).w);
			float4 screenColor36_g42 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( screenPosNorm + float4( append17_g42, 0 , 0 ) ).xy/( screenPosNorm + float4( append17_g42, 0 , 0 ) ).w);
			float4 screenColor34_g42 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( screenPosNorm - float4( append18_g42, 0 , 0 ) ).xy/( screenPosNorm - float4( append18_g42, 0 , 0 ) ).w);
			float4 screenColor30_g42 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( screenPosNorm - float4( append13_g42, 0 , 0 ) ).xy/( screenPosNorm - float4( append13_g42, 0 , 0 ) ).w);
			float4 screenColor31_g42 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( screenPosNorm - float4( append11_g42, 0 , 0 ) ).xy/( screenPosNorm - float4( append11_g42, 0 , 0 ) ).w);
			float4 screenColor37_g42 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( screenPosNorm - float4( append17_g42, 0 , 0 ) ).xy/( screenPosNorm - float4( append17_g42, 0 , 0 ) ).w);
			float4 screenColor33_g42 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( float4( append12_g42, 0 , 0 ) - screenPosNorm ).xy/( float4( append12_g42, 0 , 0 ) - screenPosNorm ).w);
			float4 staticSwitch1318 = ( screenColor22_g42 + screenColor32_g42 + screenColor35_g42 + screenColor36_g42 + screenColor34_g42 + screenColor30_g42 + screenColor31_g42 + screenColor37_g42 + screenColor33_g42 ) / 9;
			#else
			float4 staticSwitch1318 = screenColor22_g42;
			#endif
			float2 uv_CutoffMap = i.uv_texcoord * _CutoffMap_ST.xy + _CutoffMap_ST.zw;
			float cutoff = _Cutoff * tex2D( _CutoffMap, uv_CutoffMap ).a;
			float4 lerp1307 = lerp( float4( surf1265 , 0 ) , staticSwitch1318 , cutoff );
			c.rgb = lerp1307.rgb;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
		}

		ENDCG
	}
	Fallback "Standard"
	CustomEditor "ProCarPaintShader"
}
