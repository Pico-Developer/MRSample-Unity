Shader "Custom/Wireframe"
{
    Properties
    {
        _LineColor("Line Color", Color) = (1,1,1,1)
        _WireThickness ("Wire Thickness", RANGE(0, 800)) = 100
        [Toggle(ENABLE_DRAWQUAD)]_DrawQuad("Draw Quad", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Blend Off
            ZWrite On
            Cull Off
            LOD 200
            
            CGPROGRAM
            #pragma target 4.0
            #pragma multi_compile __ ENABLE_DRAWQUAD
            #include "UnityCG.cginc"
            #include "Wireframe Function.cginc"
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            
            ENDCG
        }

        Pass
        {
            Blend Off
            ZWrite On
            Cull Off
            LOD 200
            
            CGPROGRAM
            #pragma target 4.0
            #pragma multi_compile __ ENABLE_DRAWQUAD
            #include "UnityCG.cginc"
            #include "Wireframe Function.cginc"
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            
            ENDCG
        }
    }
}
