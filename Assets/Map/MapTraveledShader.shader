Shader "CustomRenderTexture/MapTraveledShader"
{
    Properties
    {  
        _Coordinate("Coordinate", Vector) = (0.5, 0.5, 0, 0)
        _Size("Size", Float) = 100
        _Fade("Fade", Float) = 100
    }

     SubShader
     {
        Lighting Off
        Blend One Zero

        Pass
        {
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            fixed4 _Coordinate;
            half _Size;
            half _Fade;

            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                return saturate(pow(saturate(1 - distance(IN.localTexcoord, _Coordinate.xy)), _Size) * _Fade + tex2D(_SelfTexture2D, IN.localTexcoord.xy));
            }
            ENDCG
            }
    }
}