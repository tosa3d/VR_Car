
Shader "Hidden / Pro Car Paint Utility shader"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_Sharp("Sharp", Float) = 81
		[Normal]_NormalMap("Normal Map", 2D) = "bump" {}
	}
	
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }
		LOD 100
		Cull Off
		Pass
		{
			CGPROGRAM
			#pragma target 3.0 
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "UnityStandardUtils.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 texcoord : TEXCOORD0;
			};

			uniform sampler2D _MainTex;
			uniform fixed4 _Color;
			uniform float _Sharp;
			uniform sampler2D _NormalMap;
			uniform float4 _NormalMap_ST;

			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
				float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
				float d = q.x - min( q.w, q.y );
				float e = 1.0e-10;
				return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}
			
			v2f vert ( appdata v )
			{
				v2f o;
				o.vertex.xyz +=  float3(0,0,0) ;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord.xy = v.texcoord.xy;
				o.texcoord.zw = v.texcoord1.xy;
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				fixed4 Var;
				float2 uv_NormalMap = i.texcoord.xy*_NormalMap_ST.xy + _NormalMap_ST.zw;
				float3 normalmap = UnpackScaleNormal( tex2D( _NormalMap, uv_NormalMap ) ,0.5 );
				float4 tex = (float4(normalmap.r , normalmap.g , 0.0 , 1.0));
				float3 tex2 = RGBToHSV( tex.rgb );
				Var = (saturate( ( _Sharp * tex2.z ) )).xxxx;
				return Var;
			}
			ENDCG
		}
	}
}