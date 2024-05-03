Shader "Hidden / Pro Car Paint Matcap - Lite"
{
	Properties
	{
		[HDR]_Color("Paint (RGB)", Color) = (0.5,0.5,0.5,1)
		[HDR]_SecondaryPaint("Secondary Paint (RGB)", Color) = (0,0,0,1)
		_PaintFresnelScale("Paint Fresnel Scale", Range( 0 , 1)) = 0
		_PaintFresnelPaintExponent("Paint Fresnel Paint Exponent", Range( 0 , 10)) = 1
		_MainTex("Albedo (RGB) Paints (A)", 2D) = "black" {}
		[NoScaleOffset]_MatcapTexture("Matcap Texture", 2D) = "gray" {}
		_MatcapIntensity("Matcap Intensity", Range( 0 , 10)) = 1
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
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry" "IsEmissive"=" true" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma multi_compile __ CLEARCOAT
		#pragma multi_compile __ CLEARCOAT_UNDER
		#pragma multi_compile __ DECAL
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			float2 uv_texcoord;
			float3 worldRefl;
			INTERNAL_DATA
		};

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
		uniform sampler2D _MatcapTexture;
		uniform float _MatcapIntensity;

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

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float fresnelDot = dot( i.worldNormal, normalize( UnityWorldSpaceViewDir( i.worldPos ) ) );
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2D148 = tex2D( _MainTex, uv_MainTex );
			float fresnel357 = _PaintFresnelScale * pow( 1.0 - fresnelDot, _PaintFresnelPaintExponent );
			float4 lerp251 = lerp( _SecondaryPaint , _Color , ( 1.0 - fresnel357 ));
			float4 Color2363 = lerp( lerp251 , tex2D148 , tex2D148.a);
			float3 hsvTorgb4_g1 = RGBToHSV( Color2363 );
			#ifdef CLEARCOAT
			float3 hsvTorgb12_g1 = HSVToRGB( float3(hsvTorgb4_g1.x,hsvTorgb4_g1.y,clamp( hsvTorgb4_g1.z + .15 , .15 , 1 ) ) );
			float fresnel14_g1 = _ReflectionIntensity * pow( 1.0 - fresnelDot, _ReflectionExponent );
			float4 Reflection1828 = ( _ReflectionColorOverride ? _ReflectionColor : float4( hsvTorgb12_g1 , 0.0 ) ) * texCUBElod( _ReflectionCubeMap, float4( normalize( i.worldRefl ), _ReflectionBlur) ) * fresnel14_g1;
			float ReflectionMask2215 = saturate( ( _ReflectionStrength * 2 ) * pow( 1.0 - fresnelDot, _ReflectionExponent / 2.5 ) );
			float4 BaseLayer54 = lerp( Color2363 , Reflection1828 , ReflectionMask2215);
			#else
			float4 BaseLayer54 = Color2363;
			#endif
			#ifdef DECAL
			#ifdef CLEARCOAT
			#ifdef CLEARCOAT_UNDER
			float4 BD = lerp( Reflection1828 + BaseLayer54 , Reflection1828 , ReflectionMask2215);
			#else
			float4 BD = BaseLayer54;
			#endif
			#else
			float4 BD = BaseLayer54;
			#endif
			#else
			#ifdef CLEARCOAT
			float4 BD = lerp( Reflection1828 + BaseLayer54 , Reflection1828 , ReflectionMask2215);
			#else
			float4 BD = BaseLayer54;
			#endif
			#endif
			#ifdef DECAL
			float2 uv_1Decal = i.uv_texcoord * _1Decal_ST.xy + _1Decal_ST.zw;
			float4 tex2D47_g729 = tex2D( _1Decal, uv_1Decal );
			float2 uv_2Decal = i.uv_texcoord * _2Decal_ST.xy + _2Decal_ST.zw;
			float4 tex2D52_g729 = tex2D( _2Decal, uv_2Decal );
			float2 uv_3Decal = i.uv_texcoord * _3Decal_ST.xy + _3Decal_ST.zw;
			float4 tex2D49_g729 = tex2D( _3Decal, uv_3Decal );
			float2 uv_4Decal = i.uv_texcoord * _4Decal_ST.xy + _4Decal_ST.zw;
			float4 tex2D55_g729 = tex2D( _4Decal, uv_4Decal );
			float2 uv_5Decal = i.uv_texcoord * _5Decal_ST.xy + _5Decal_ST.zw;
			float4 tex2D44_g729 = tex2D( _5Decal, uv_5Decal );
			float4 AD = lerp( lerp( lerp( lerp( lerp( BD , _1DecalColor * tex2D47_g729 , tex2D47_g729.a ) , _2DecalColor * tex2D52_g729 , tex2D52_g729.a ) , _3DecalColor * tex2D49_g729 , tex2D49_g729.a ) , _4DecalColor * tex2D55_g729 , tex2D55_g729.a) , _5DecalColor * tex2D44_g729 , tex2D44_g729.a );
			#else
			float4 AD = BD;
			#endif
			#ifdef DECAL
			#ifdef CLEARCOAT
			#ifdef CLEARCOAT_UNDER
			float4 Final_Albedo = AD;
			#else
			float4 Final_Albedo = lerp( Reflection1828 + AD , Reflection1828 , ReflectionMask2215);
			#endif
			#else
			float4 Final_Albedo = AD;
			#endif
			#else
			#ifdef CLEARCOAT
			float4 Final_Albedo = AD;
			#else
			float4 Final_Albedo = AD;
			#endif
			#endif
			o.Emission = Final_Albedo * tex2D( _MatcapTexture, ( ( mul( UNITY_MATRIX_V, float4( i.worldNormal , 0 ) ).xyz * .49 ) + .5 ).xy ) * _MatcapIntensity;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Standard"
	CustomEditor "ProCarPaintShader"
}
