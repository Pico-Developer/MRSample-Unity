Shader "Custom/ShowWireframe"
{
    Properties
    {
        _LineColor ("LineColor", COLOR) = (1,1,0,1)
    }
    SubShader
    {
       
        Pass
        { 
			Tags { "RenderType"="Opaque" }
			LOD 200
			Cull Off

            CGPROGRAM
            #pragma target 5.0
			#pragma vertex VS_Main
            #pragma fragment FS_Main
			#pragma geometry GS_Main
            #include "UnityCG.cginc"

			fixed4 _LineColor;

            struct GS_INPUT
            {
                float4 pos	: POSITION;
            };

            struct FS_INPUT
            {
                
                float4 pos	: POSITION;
            };

			GS_INPUT VS_Main(appdata_full v)
			{
				GS_INPUT output = (GS_INPUT)0;
				output.pos = mul(unity_ObjectToWorld, v.vertex);
				return output;
			}
			 
            [maxvertexcount(6)]
			void GS_Main(triangle GS_INPUT p[3], inout LineStream<FS_INPUT> triStream)
			{
				FS_INPUT pOut;
				pOut.pos = mul(UNITY_MATRIX_VP, p[0].pos);
				triStream.Append(pOut);
				FS_INPUT pOut2;
				pOut2.pos = mul(UNITY_MATRIX_VP, p[1].pos);
				triStream.Append(pOut2);
				triStream.RestartStrip();

				FS_INPUT pOut3;
				pOut3.pos = mul(UNITY_MATRIX_VP, p[1].pos);
				triStream.Append(pOut3);
				FS_INPUT pOut4;
				pOut4.pos = mul(UNITY_MATRIX_VP, p[2].pos);
				triStream.Append(pOut4);
				triStream.RestartStrip();

				FS_INPUT pOut5;
				pOut5.pos = mul(UNITY_MATRIX_VP, p[2].pos);
				triStream.Append(pOut5);
				FS_INPUT pOut6;
				pOut6.pos = mul(UNITY_MATRIX_VP, p[0].pos);
				triStream.Append(pOut6);
				triStream.RestartStrip();

			}

			fixed4 FS_Main(FS_INPUT input)  : COLOR
			{
				return _LineColor;
			}

            ENDCG
        }
    }
}
