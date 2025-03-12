Shader "Custom/TextureBlend" {
    Properties {
        _MainTex ("Main Texture", 2D) = "white" {}   // 기본 텍스처 (할당 안되면 흰색)
        _MaskTex ("Overlay Texture", 2D) = "black" {}  // 덧씌울 텍스처 (할당 안되면 검정색)
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        // Traditional Alpha Blending
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _MaskTex;
            float4 _MaskTex_ST;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // _MainTex의 UV 계산
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // 기본 텍스처 색상
                fixed4 mainColor = tex2D(_MainTex, i.uv);
                // _MaskTex의 UV를 따로 계산 (필요에 따라 별도의 UV 매핑 가능)
                float2 maskUV = TRANSFORM_TEX(i.uv, _MaskTex);
                fixed4 maskColor = tex2D(_MaskTex, maskUV);

                // maskColor의 특정 채널을 사용하여 두 텍스처를 혼합함.
                // 채널 값이 0이면 mainColor, 1이면 maskColor가 그대로 보임.
                fixed4 finalColor = lerp(mainColor, maskColor, maskColor.r);
                return finalColor;
            }
            ENDCG
        }
    }
}
