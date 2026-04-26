Shader "Hidden/ArtefactBaker"
{
    SubShader
    {
        Cull Off // Very important: don't hide the back of the triangles!
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNorm : TEXCOORD1;
            };

            v2f vert (appdata v) {
                v2f o;
                
                // Ignore the camera. Force the vertices onto a flat 2D grid based on their UVs by mapping
                // them from [0, 1] to [-1, 1] (clip space)
                // Y is inverted because apparently Unity's Y axis originates at the bottom and not the top??
                // Set Z-Depth between 0 and 1, it doesn't matter
                // Set w to 1.0 so the vector doesn't change when converted from clip space to NDC (dividing by w)
                o.pos = float4(v.uv.x * 2.0 - 1.0, (1.0 - v.uv.y) * 2.0 - 1.0, 0.5, 1.0);
                
                // Store the real 3D coordinates to pass to the fragment stage
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNorm = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            // Output to TWO targets simultaneously! Target0 = Position, Target1 = Normal
            struct fragOut {
                float4 posTarget : SV_Target0;
                float4 normTarget : SV_Target1;
            };

            fragOut frag (v2f i) {
                fragOut o;
                // Output the XYZ position as RGB colors. Set Alpha to 1 to mark it as a valid pixel.
                o.posTarget = float4(i.worldPos, 1.0); 
                // Output the Normal direction
                o.normTarget = float4(normalize(i.worldNorm), 1.0);
                return o;
            }
            ENDCG
        }
    }
}