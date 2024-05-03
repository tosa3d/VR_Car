Shader "Pro Car Paint"
{
	Properties
	{
		[HDR]_Color("Paint (RGB)", Color) = (0.5,0.5,0.5,1)
		[HDR]_SecondaryPaint("Secondary Paint (RGB)", Color) = (0,0,0,1)
		_PaintFresnelScale("Paint Fresnel Scale", Range( 0 , 1)) = 0
		_PaintFresnelPaintExponent("Paint Fresnel Paint Exponent", Range( 0 , 10)) = 1
		_MainTex("Albedo (RGB) Paints (A)", 2D) = "black" {}
		_BumpScale("Albedo Normal Map Scale", Float) = 1
		[Normal]_BumpMap("Albedo Normal", 2D) = "bump" {}
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Glossiness("Smoothness", Range( 0 , 1)) = 0
		[HDR]_EmissionColor("Emission (RGB)", Color) = (0,0,0,0)
		_MetallicGlossMap("Metallic (R) Smoothness (G) Emission (B)", 2D) = "white" {}
		_FlakeMask("Flake Mask", 2D) = "white" {}
		[Normal]_FlakeNormal("Flake Normal", 2D) = "bump" {}
		_FlakeMetallic("Flake Metallic", Range( 0 , 1)) = 0
		_FlakeSmoothness("Flake Smoothness", Range( 0 , 1)) = 0.5
		_FlakeNormalScale("Flake Normal Scale", Range( 0 , 10)) = 1
		_FlakeDistance("Flake Visible Distance", Range( 0 , 20)) = 1
		[Toggle]_FlakeColorOverride("Flake Color Override", Float) = 0
		_FlakeSize("Flake Size", Float) = 1
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
		_ReflectionStrength("Reflection Strength", Range( 0 , 1)) = 0
		[Toggle]_ReflectionColorOverride("Reflection ColorOverride", Float) = 1
		[HDR]_ReflectionColor("Reflection Color", Color) = (1,1,1,1)
		_ClearCoatSmoothness("Clear Coat Smoothness", Range( 0 , 1)) = 1
		_ClearCoatNormalScale("Clear Coat Normal Scale", Float) = 1
		[Normal]_ClearCoatNormal("Clear Coat Normal", 2D) = "bump" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry" "IsEmissive" = "true" }
		Cull Back
		CGPROGRAM
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#pragma target 4.0
		#pragma multi_compile __ CLEARCOAT
		#pragma multi_compile __ DECAL
		#pragma multi_compile __ CLEARCOAT_UNDER
		#pragma multi_compile __ CLEARCOAT_FIRST
		#pragma multi_compile __ DECAL_UNDER
		#pragma multi_compile __ FLAKE
		#pragma surface surf StandardCustomLighting keepalpha addshadow fullforwardshadows vertex:vertexDataFunc
		struct Input
		{
			float3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldRefl;
			float eyeDepth;
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
		uniform float4 _SecondaryPaint;
		uniform float4 _Color;
		uniform float _PaintFresnelScale;
		uniform float _PaintFresnelPaintExponent;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float4 _ReflectionColor;
		uniform samplerCUBE _ReflectionCubeMap;
		uniform half _ReflectionBlur;
		uniform float _ReflectionIntensity;
		uniform float _ReflectionExponent;
		uniform float _ReflectionStrength;
		uniform float _FlakeColorOverride;
		uniform sampler2D _FlakeMask;
		uniform float _FlakeSize;
		uniform float _FlakeDistance;
		uniform float _FlakeNormalScale;
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
		uniform sampler2D _FlakeNormal;
		uniform float4 _EmissionColor;
		uniform float _Metallic;
		uniform float _FlakeMetallic;
		uniform float _Glossiness;
		uniform float _FlakeSmoothness;
		uniform float _ClearCoatNormalScale;
		uniform sampler2D _ClearCoatNormal;
		uniform float4 _ClearCoatNormal_ST;
		uniform float _ClearCoatSmoothness;

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

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			o.eyeDepth = -UnityObjectToViewPos( v.vertex.xyz ).z;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 final = 0;
			SurfaceOutputStandard s2185 = (SurfaceOutputStandard ) 0;
			float3 worldViewDir = normalize( UnityWorldSpaceViewDir( i.worldPos ) );
			float3 worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float fresnelDot = dot( worldNormal, worldViewDir );
			float4 tex2D605 = tex2D( _MetallicGlossMap, i.uv_texcoord * _MetallicGlossMap_ST.xy + _MetallicGlossMap_ST.zw );
			float GlobalMetallic1480 = tex2D605.r;
			float GlobalSmoothness1481 = tex2D605.g;
			float GlobalEmission1532 = tex2D605.b;
			float4 tex2D148 = tex2D( _MainTex, i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw );
			float3 tex2D1360 = UnpackScaleNormal( tex2D( _BumpMap, i.uv_texcoord * _BumpMap_ST.xy + _BumpMap_ST.zw ), _BumpScale );
			float3 temp_cast_61 = _Metallic * GlobalMetallic1480;
			float3 temp_cast_83 = _Glossiness * GlobalSmoothness1481;
			float fresnel357 = _PaintFresnelScale * pow( 1.0 - fresnelDot, _PaintFresnelPaintExponent );
			float3 lerp251 = lerp( _SecondaryPaint , _Color , 1.0 - fresnel357);
			float3 Color2363 = lerp( lerp251 , tex2D148 , tex2D148.a);
			#ifdef CLEARCOAT
			float3 hsvTorgb4_g4 = RGBToHSV( Color2363 );
			float3 hsvTorgb12_g4 = HSVToRGB( float3(hsvTorgb4_g4.x,hsvTorgb4_g4.y,clamp( hsvTorgb4_g4.z + 0.15 , 0.15 , 1 )) );
			float3 worldReflection = normalize( WorldReflectionVector( i, float3( 0, 0, 1 ) ) );
			float fresnel14_g4 = _ReflectionIntensity * pow( 1.0 - fresnelDot, _ReflectionExponent );
			float3 Reflection1828 = ( _ReflectionColorOverride ? _ReflectionColor : hsvTorgb12_g4 ) * texCUBElod( _ReflectionCubeMap, float4( worldReflection, _ReflectionBlur) ) * fresnel14_g4 ;
			float ReflectionMask2215 = saturate( ( _ReflectionStrength * 2 ) * pow( 1.0 - fresnelDot, _ReflectionExponent / 2.5 ) );
			float3 BaseLayer54 = lerp( Color2363 , Reflection1828 , ReflectionMask2215);
			#else
			float3 BaseLayer54 = Color2363;
			#endif
			#ifdef FLAKE
			float2 uv_TexCoord6_g614 = i.uv_texcoord * _FlakeSize.xx;
			float4 tex2D15_g614 = tex2D( _FlakeMask, uv_TexCoord6_g614 );
			float3 hsvTorgb21_g614 = RGBToHSV( BaseLayer54 );
			float3 hsvTorgb26_g614 = HSVToRGB( float3(hsvTorgb21_g614.x, hsvTorgb21_g614.y, hsvTorgb21_g614.z * 1.25 ) );
			float3 FlakesColor1840 = _FlakeColorOverride ?  tex2D15_g614 : hsvTorgb26_g614 ;
			float cameraDepthFade5_g614 = ( i.eyeDepth -_ProjectionParams.y - 0.0 ) / _FlakeDistance;
			float temp_output_10_0_g614 = 1.0 - saturate( cameraDepthFade5_g614 );
			float FlanksMask1368 = saturate( temp_output_10_0_g614 * temp_output_10_0_g614 * tex2D15_g614.a * ceil( _FlakeNormalScale ) );
			float3 FlakesNormal1859 = UnpackScaleNormal( tex2D( _FlakeNormal, uv_TexCoord6_g614 ), _FlakeNormalScale );
			float3 temp_output_38_0_g607 = BlendNormals( tex2D1360 , FlakesNormal1859 );
			#endif
			#ifdef FLAKE
			#ifdef DECAL
			#ifdef DECAL_UNDER
			#ifdef CLEARCOAT
			#ifdef CLEARCOAT_UNDER
			#ifdef CLEARCOAT_FIRST
			float3 BD = BaseLayer54;
			#else
			float3 BD = lerp( GlobalSmoothness1481 * Reflection1828 + BaseLayer54 , Reflection1828 , ReflectionMask2215);
			#endif
			#else
			float3 BD = BaseLayer54;
			#endif
			#else
			float3 BD = BaseLayer54;
			#endif
			#else
			#ifdef CLEARCOAT
			#ifdef CLEARCOAT_UNDER
			float3 BD = lerp( lerp( GlobalSmoothness1481 * Reflection1828 + BaseLayer54 , Reflection1828 , ReflectionMask2215) , FlakesColor1840 , FlanksMask1368);
			#else
			#ifdef CLEARCOAT_FIRST
			float3 BD = lerp( GlobalSmoothness1481 *  Reflection1828 + lerp( BaseLayer54 , FlakesColor1840 , FlanksMask1368) ,  Reflection1828 , ReflectionMask2215);
			#else
			float3 BD = lerp( BaseLayer54 , FlakesColor1840 , FlanksMask1368);
			#endif
			#endif
			#else
			float3 BD = lerp( BaseLayer54 , FlakesColor1840 , FlanksMask1368);
			#endif
			#endif
			#else
			#ifdef CLEARCOAT
			#ifdef CLEARCOAT_UNDER
			float3 BD = lerp( lerp( GlobalSmoothness1481 * Reflection1828 + BaseLayer54 , Reflection1828 , ReflectionMask2215) , FlakesColor1840 , FlanksMask1368);
			#else
			float3 BD = lerp( GlobalSmoothness1481 *  Reflection1828 + lerp( BaseLayer54 , FlakesColor1840 , FlanksMask1368) ,  Reflection1828 , ReflectionMask2215);
			#endif
			#else
			float3 BD = lerp( BaseLayer54 , FlakesColor1840 , FlanksMask1368);
			#endif
			#endif
			#else
			#ifdef DECAL
			#ifdef CLEARCOAT
			#ifdef CLEARCOAT_UNDER
			float3 BD = lerp( GlobalSmoothness1481 * Reflection1828 + BaseLayer54 , Reflection1828 , ReflectionMask2215);
			#else
			float3 BD = BaseLayer54;
			#endif
			#else
			float3 BD = BaseLayer54;
			#endif
			#else
			#ifdef CLEARCOAT
			float3 BD = lerp( GlobalSmoothness1481 * Reflection1828 + BaseLayer54 , Reflection1828 , ReflectionMask2215);
			#else
			float3 BD = BaseLayer54;
			#endif
			#endif
			#endif
			#ifdef DECAL
			float2 uv_1Decal = i.uv_texcoord * _1Decal_ST.xy + _1Decal_ST.zw;
			float4 tex2D47_g553 = tex2D( _1Decal, uv_1Decal );
			float2 uv_2Decal = i.uv_texcoord * _2Decal_ST.xy + _2Decal_ST.zw;
			float4 tex2D52_g553 = tex2D( _2Decal, uv_2Decal );
			float2 uv_3Decal = i.uv_texcoord * _3Decal_ST.xy + _3Decal_ST.zw;
			float4 tex2D49_g553 = tex2D( _3Decal, uv_3Decal );
			float2 uv_4Decal = i.uv_texcoord * _4Decal_ST.xy + _4Decal_ST.zw;
			float4 tex2D55_g553 = tex2D( _4Decal, uv_4Decal );
			float2 uv_5Decal = i.uv_texcoord * _5Decal_ST.xy + _5Decal_ST.zw;
			float4 tex2D44_g553 = tex2D( _5Decal, uv_5Decal );
			float3 AD = lerp( lerp( lerp( lerp( lerp( BD , _1DecalColor * tex2D47_g553 , tex2D47_g553.a ) , _2DecalColor * tex2D52_g553 , tex2D52_g553.a ) , _3DecalColor * tex2D49_g553 , tex2D49_g553.a ) , _4DecalColor * tex2D55_g553 , tex2D55_g553.a ) , _5DecalColor * tex2D44_g553 , tex2D44_g553.a );
			float DecalsMask1627 = saturate( ( tex2D47_g553.a + tex2D52_g553.a + tex2D49_g553.a + tex2D55_g553.a + tex2D44_g553.a ) );
			#else
			float3 AD = BD;
			#endif
			#ifdef FLAKE
			#ifdef DECAL
			#ifdef DECAL_UNDER
			#ifdef CLEARCOAT
			#ifdef CLEARCOAT_UNDER
			float localShaderSwitch17_g610 = lerp( temp_cast_61 , _FlakeMetallic , FlanksMask1368);
			float localShaderSwitch17_g606 = lerp( temp_cast_83 , _FlakeSmoothness , FlanksMask1368);
			float3 localShaderSwitch17_g608 = lerp( tex2D1360 , temp_output_38_0_g607 , FlanksMask1368);
			#ifdef CLEARCOAT_FIRST
			float3 Final_Albedo = lerp( lerp( GlobalSmoothness1481 * Reflection1828 + AD , Reflection1828 , ReflectionMask2215) , FlakesColor1840 , FlanksMask1368);
			#else
			float3 Final_Albedo = lerp( AD , FlakesColor1840 , FlanksMask1368);
			#endif
			#else
			float3 Final_Albedo = lerp( GlobalSmoothness1481 * Reflection1828 + lerp( AD , FlakesColor1840 , FlanksMask1368) , Reflection1828 , ReflectionMask2215);
			float3 localShaderSwitch17_g608 = lerp( tex2D1360 , temp_output_38_0_g607 , saturate( ( FlanksMask1368 - ReflectionMask2215 ) ));
			float localShaderSwitch17_g610 = lerp( temp_cast_61 , _FlakeMetallic , saturate( ( FlanksMask1368 - ReflectionMask2215 ) ));
			float localShaderSwitch17_g606 = lerp( temp_cast_83 , _FlakeSmoothness , saturate( ( FlanksMask1368 - ReflectionMask2215 ) ));
			#endif
			#else
			float3 Final_Albedo = lerp( AD , FlakesColor1840 , FlanksMask1368);
			float3 localShaderSwitch17_g608 = lerp( tex2D1360 , temp_output_38_0_g607 , FlanksMask1368);
			float localShaderSwitch17_g610 = lerp( temp_cast_61 , _FlakeMetallic , FlanksMask1368);
			float localShaderSwitch17_g606 = lerp( temp_cast_83 , _FlakeSmoothness , FlanksMask1368);
			#endif
			#else
			#ifdef CLEARCOAT
			#ifdef CLEARCOAT_UNDER
			float3 Final_Albedo = AD;
			float3 localShaderSwitch17_g608 = lerp( tex2D1360 , temp_output_38_0_g607 , saturate( ( FlanksMask1368 - DecalsMask1627 ) ));
			float localShaderSwitch17_g610 = lerp( temp_cast_61 , _FlakeMetallic , saturate( ( FlanksMask1368 - DecalsMask1627 ) ));
			float localShaderSwitch17_g606 = lerp( temp_cast_83 , _FlakeSmoothness , saturate( ( FlanksMask1368 - DecalsMask1627 ) ));
			#else
			float3 localShaderSwitch17_g608 = lerp( tex2D1360 , temp_output_38_0_g607 , saturate( ( FlanksMask1368 - saturate( ( DecalsMask1627 + ReflectionMask2215 ) ) ) ));
			float localShaderSwitch17_g610 = lerp( temp_cast_61 , _FlakeMetallic , saturate( ( FlanksMask1368 - saturate( ( DecalsMask1627 + ReflectionMask2215 ) ) ) ));
			float localShaderSwitch17_g606 = lerp( temp_cast_83 , _FlakeSmoothness , saturate( ( FlanksMask1368 - saturate( ( DecalsMask1627 + ReflectionMask2215 ) ) ) ));
			#ifdef CLEARCOAT_FIRST
			float3 Final_Albedo = AD;
			#else
			float3 Final_Albedo = lerp( GlobalSmoothness1481 * Reflection1828 + AD , Reflection1828 , ReflectionMask2215);
			#endif
			#endif
			#else
			float3 Final_Albedo = AD;
			float3 localShaderSwitch17_g608 = lerp( tex2D1360 , temp_output_38_0_g607 , saturate( ( FlanksMask1368 - DecalsMask1627 ) ));
			float localShaderSwitch17_g610 = lerp( temp_cast_61 , _FlakeMetallic , saturate( ( FlanksMask1368 - DecalsMask1627 ) ));
			float localShaderSwitch17_g606 = lerp( temp_cast_83 , _FlakeSmoothness , saturate( ( FlanksMask1368 - DecalsMask1627 ) ));
			#endif
			#endif
			#else
			float3 Final_Albedo = AD;
			#ifdef CLEARCOAT
			#ifdef CLEARCOAT_UNDER
			float3 localShaderSwitch17_g608 = lerp( tex2D1360 , temp_output_38_0_g607 , FlanksMask1368);
			float localShaderSwitch17_g610 = lerp( temp_cast_61 , _FlakeMetallic , FlanksMask1368);
			float localShaderSwitch17_g606 = lerp( temp_cast_83 , _FlakeSmoothness , FlanksMask1368);
			#else
			float3 localShaderSwitch17_g608 = lerp( tex2D1360 , temp_output_38_0_g607 , saturate( ( FlanksMask1368 - ReflectionMask2215 ) ));
			float localShaderSwitch17_g610 = lerp( temp_cast_61 , _FlakeMetallic , saturate( ( FlanksMask1368 - ReflectionMask2215 ) ));
			float localShaderSwitch17_g606 = lerp( temp_cast_83 , _FlakeSmoothness , saturate( ( FlanksMask1368 - ReflectionMask2215 ) ));
			#endif
			#else
			float3 localShaderSwitch17_g608 = lerp( tex2D1360 , temp_output_38_0_g607 , FlanksMask1368);
			float localShaderSwitch17_g610 = lerp( temp_cast_61 , _FlakeMetallic , FlanksMask1368);
			float localShaderSwitch17_g606 = lerp( temp_cast_83 , _FlakeSmoothness , FlanksMask1368);
			#endif
			#endif
			#else
			float3 localShaderSwitch17_g608 = tex2D1360;
			float localShaderSwitch17_g610 = temp_cast_61;
			float localShaderSwitch17_g606 = temp_cast_83;
			#ifdef DECAL
			#ifdef CLEARCOAT
			#ifdef CLEARCOAT_UNDER
			float3 Final_Albedo = AD;
			#else
			float3 Final_Albedo = lerp( GlobalSmoothness1481 * Reflection1828 + AD , Reflection1828 , ReflectionMask2215);
			#endif
			#else
			float3 Final_Albedo = AD;
			#endif
			#else
			float3 Final_Albedo = AD;
			#endif
			#endif
			s2185.Albedo = Final_Albedo;
			s2185.Normal = WorldNormalVector( i , localShaderSwitch17_g608 );
			s2185.Metallic = localShaderSwitch17_g610;
			s2185.Smoothness = localShaderSwitch17_g606.x;
			s2185.Emission = _EmissionColor * GlobalEmission1532;
			s2185.Occlusion = 1;
			data.light = gi.light;
			UnityGI gi2185 = gi;
			#ifdef UNITY_PASS_FORWARDBASE
			Unity_GlossyEnvironmentData g2185 = UnityGlossyEnvironmentSetup( s2185.Smoothness, data.worldViewDir, s2185.Normal, float3(0,0,0));
			gi2185 = UnityGlobalIllumination( data, s2185.Occlusion, s2185.Normal, g2185 );
			#endif
			float3 surf2185 = LightingStandard ( s2185, viewDir, gi2185 ).rgb;
			surf2185 += s2185.Emission;
			#ifdef UNITY_PASS_FORWARDADD
			surf2185 -= s2185.Emission;
			#endif
			#ifdef CLEARCOAT
			SurfaceOutputStandardSpecular s14_g611 = (SurfaceOutputStandardSpecular ) 0;
			s14_g611.Albedo = float3( 0,0,0 );
			float2 uv_ClearCoatNormal = i.uv_texcoord * _ClearCoatNormal_ST.xy + _ClearCoatNormal_ST.zw;
			float3 tex2D2_g611 = UnpackScaleNormal( tex2D( _ClearCoatNormal, uv_ClearCoatNormal ), _ClearCoatNormalScale );
			s14_g611.Normal = WorldNormalVector( i , tex2D2_g611 );
			s14_g611.Emission = float3( 0,0,0 );
			s14_g611.Specular = GlobalMetallic1480;
			s14_g611.Smoothness = GlobalSmoothness1481 * _ClearCoatSmoothness;
			s14_g611.Occlusion = 1;
			data.light = gi.light;
			UnityGI gi14_g611 = gi;
			#ifdef UNITY_PASS_FORWARDBASE
			Unity_GlossyEnvironmentData g14_g611 = UnityGlossyEnvironmentSetup( s14_g611.Smoothness, data.worldViewDir, s14_g611.Normal, float3(0,0,0));
			gi14_g611 = UnityGlobalIllumination( data, s14_g611.Occlusion, s14_g611.Normal, g14_g611 );
			#endif
			float3 surf14_g611 = LightingStandardSpecular ( s14_g611, viewDir, gi14_g611 ).rgb;
			surf14_g611 += s14_g611.Emission;
			#ifdef UNITY_PASS_FORWARDADD
			surf14_g611 -= s14_g611.Emission;
			#endif
			float fresnel11_g611 = 0.05 + 1 * pow( 1.0 - fresnelDot, 5 );
			float3 lerp18_g611 = lerp( surf2185 , surf14_g611 , fresnel11_g611);
			final.rgb = lerp18_g611;
			#else
			final.rgb = surf2185;
			#endif
			final.a = 1;
			return final;
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