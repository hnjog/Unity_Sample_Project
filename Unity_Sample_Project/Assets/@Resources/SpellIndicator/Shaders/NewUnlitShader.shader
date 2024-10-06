Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert // vs에서 사용할 함수
            #pragma fragment frag // fs에서 사용할 함수
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // 버텍스 쉐이더 (정점)
            v2f vert (appdata v)
            {
                // 정점마다 실행될 함수 내용들
                // 보통은 좌표 변환
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            // 프래그 쉐이더 (픽셀 쉐이더)
            fixed4 frag (v2f i) : SV_Target
            {
                // 픽셀마다 적용될 함수 내용들
                // 보통 색 적용 등
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return fixed4(1,0,0,0);
            }
            ENDCG
        }
    }
}
