Shader "Unlit/MeshSplattingFade"
{
    Properties
    {
        _MainTex("Base texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _FadePower("Fade Power", Float) = 1
        [Enum(Off,2,On,0)] _DoubleSidedEnable("Double Sided", Float) = 0 //"Back"
    }
    SubShader
    {
        //Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
        //Cull[_DoubleSidedEnable]
        ////Blend SrcColor DstColor, SrcAlpha DstAlpha // Multiplicative
        //Blend One One
        ////Blend One One, One One
        ////Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
        ////Blend One OneMinusSrcAlpha // Premultiplied transparency
        ////Blend One One, Zero One// Additive color, no alpha
        ////Blend Zero One, One One// No color, Additive alpha
        ////Blend OneMinusDstColor One // Soft additive
        ////Blend DstColor Zero // Multiplicative
        ////Blend DstColor SrcColor // 2x multiplicative


        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
        Cull[_DoubleSidedEnable]
        //Blend SrcColor DstColor, SrcAlpha DstAlpha // Multiplicative
        //Blend One One
        Blend One One, One One
        //Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
        //Blend One OneMinusSrcAlpha // Premultiplied transparency
        //Blend One One, Zero One// Additive color, no alpha
        //Blend Zero One, One One// No color, Additive alpha
        //Blend OneMinusDstColor One // Soft additive
        //Blend DstColor Zero // Multiplicative
        //Blend DstColor SrcColor // 2x multiplicative
        ZWrite Off
        LOD 100
        ColorMask RGBA

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                half3 worldNormal : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _FadePower;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            float4 ComputeFresnel(float3 Normal, float3 ViewDir, float Power)
            {
                return pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
            }

            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 baseColor = tex2D(_MainTex, i.uv).rgba;
                return saturate((_Color * baseColor.rrrr) * _FadePower);
            }
            ENDCG
        }
    }
}
