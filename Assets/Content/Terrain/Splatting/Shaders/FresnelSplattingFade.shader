Shader "Unlit/MeshSplattingFade"
{
    Properties
    {
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
                half3 worldNormal : TEXCOORD0;
            };

            fixed4 _Color;
            float _FadePower;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 ComputeFresnel(float3 Normal, float3 ViewDir, float Power)
            {
                return pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 Normal = i.worldNormal;
                float3 ViewDir = fixed3(0, 1, 0);
                float Power = _FadePower;
                fixed4 fresnel = ComputeFresnel(Normal, ViewDir, Power);
                fixed4 invFresnel = 1.0f - fresnel;

                //_Color.a = 1.0f;
                _Color = _Color * invFresnel;
                return _Color;
                //return fixed4(_Color.rgb, 1);

                //return normalize(fixed4(1, 0, 1, 1));
                //return fixed4(1, 0, 1, 1);
            }
            ENDCG
        }
    }
}
