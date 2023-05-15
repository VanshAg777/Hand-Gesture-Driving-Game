Shader "Unlit/MergeWithSplatmap"
{
    Properties
    {
        _MainTex("Base texture", 2D) = "white" {}
        _Texture1("Base texture", 2D) = "white" {}
        _Texture2("Base texture", 2D) = "white" {}
        _Texture3("Base texture", 2D) = "white" {}
        _Texture4("Base texture", 2D) = "white" {}
        //[Enum(Off,2,On,0)] _DoubleSidedEnable("Double Sided", Float) = 0 //"Back"
    }
    SubShader
    {
        Tags {"Queue" = "Geometry" "RenderType" = "Opaque" }
        //Cull[_DoubleSidedEnable]
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _Texture1;
            sampler2D _Texture2;
            sampler2D _Texture3;
            sampler2D _Texture4;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.uv.y = 1 - o.uv.y;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 baseColor = tex2D(_MainTex, i.uv);
                fixed4 blendColor = tex2D(_Texture1, i.uv);
                fixed4 blendColor2 = tex2D(_Texture2, i.uv);
                fixed4 blendColor3 = tex2D(_Texture3, i.uv);
                fixed4 blendColor4 = tex2D(_Texture4, i.uv);
                fixed4 rColor = baseColor + blendColor;

                fixed blendLength = blendColor.r + blendColor.g + blendColor.b + blendColor.a +
                    blendColor2.r + blendColor2.g + blendColor2.b + blendColor2.a +
                    blendColor3.r + blendColor3.g + blendColor3.b + blendColor3.a +
                    blendColor4.r + blendColor4.g + blendColor4.b + blendColor4.a;
                fixed baseLength = baseColor.r + baseColor.g + baseColor.b + baseColor.a;

                if (blendLength != 0)
                {
                    if (baseLength == 0)
                        rColor = blendColor;
                    else
                        rColor = fixed4((baseColor.r - ((baseColor.r / baseLength) * blendLength)) + blendColor.r,
                                        (baseColor.g - ((baseColor.g / baseLength) * blendLength)) + blendColor.g,
                                        (baseColor.b - ((baseColor.b / baseLength) * blendLength)) + blendColor.b,
                                        (baseColor.a - ((baseColor.a / baseLength) * blendLength)) + blendColor.a);
                }

                return rColor;
            }
            ENDCG
        }
    }
}
