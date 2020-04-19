Shader "Custom/UnlitCutout" {

Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _MainTex("Texture", 2D) = "white" {}
    _Cutoff("Cutoff", Range(0,1)) = 1
}

SubShader {
Tags { "Queue" = "Transparent" "RenderType" = "Opaque" }

Cull Off
CGPROGRAM

#pragma surface surf Unlit alphatest:_Cutoff

half4 LightingUnlit(SurfaceOutput surf, half3 dir, half atten)
{
    return half4(surf.Albedo, 1);
}

struct Input {
    float2 uv_MainTex;
};

fixed4 _Color;
sampler2D _MainTex;

void surf(Input IN, inout SurfaceOutput o)
{
    half4 tex = tex2D(_MainTex, IN.uv_MainTex);
    o.Albedo = tex.rgb * _Color.rgb;
    o.Alpha = tex.a;
}

ENDCG

} Fallback "Diffuse"

}
