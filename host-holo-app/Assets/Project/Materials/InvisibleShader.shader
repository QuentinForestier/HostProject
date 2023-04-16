Shader "Custom/InvisibleShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry-1" }
        LOD 100
        Cull Off

        Blend Zero One

        Pass
        {

        }
    }
}