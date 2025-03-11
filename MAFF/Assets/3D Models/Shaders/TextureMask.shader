Shader "Custom/TextureMask" {
    Properties {
        _MainTex ("Main Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "black" {}
        // 마스킹을 적용하기 위한 마스킹 채널의 임계값
        //_MaskCutoff ("Mask Cutoff", Range(0,1)) = 0.5
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
            float _MaskCutoff;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // 메인 텍스처 UV 변환
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // 메인 텍스처 샘플링
                fixed4 col = tex2D(_MainTex, i.uv);

                // 마스크 텍스처용 UV (필요시 별도의 UV 매핑 가능)
                float2 maskUV = TRANSFORM_TEX(i.uv, _MaskTex);
                // 마스크 텍스처의 빨간 채널을 사용 (다른 채널 사용도 가능)
                float mask = tex2D(_MaskTex, maskUV).r;

                // 방법 1: 마스크 값을 메인 텍스처 알파에 곱함 (부드러운 마스크 효과)
                col.a *= mask;

                // 방법 2: 마스크 컷오프를 사용해 픽셀을 클리핑하려면 아래 주석 해제
                // clip(mask - _MaskCutoff);

                return col;
            }
            ENDCG
        }
    }
}
