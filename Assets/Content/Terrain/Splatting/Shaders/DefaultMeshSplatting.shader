Shader "Unlit/DefaultMeshSplatting"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        [Enum(Off,2,On,0)] _DoubleSidedEnable("Double Sided", Float) = 0 //"Back"
    }
    SubShader
    {
        Tags {"Queue" = "Geometry" "RenderType" = "Opaque" }
        Cull[_DoubleSidedEnable]
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
            };

            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
