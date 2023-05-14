Shader "Unlit/DxShader"
{
    Properties
    {
       _MainTex ("Texture", 2D) = "white" {}
        [Enum(UnityEngine.Rendering.BlendMode)]
        _SrcFactor("Src Factor", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)]
        _DstFactor("Dst Factor", Float) = 10
        [Enum(UnityEngine.Rendering.BlendOp)]
        _Opp("Operation", Float) = 0
        
        
        _HandleY("HandleY", Float) = 0
        _HandleX("HandleX", Float) = 0
        _HandleWidth("HandleWidth", Float) = 0
        _HandleHeight("HandleHeight", Float) = 0
        _HandleColor("HandleColor", Color) = (1,1,1,1)
        
        _BallColor("Ball Color", Color) = (1,1,1,1)
        _BallPos("BallPos", Vector) = (0,0,0,0)
        _BallDiameter("Ball Diameter", Float) = .1
        
        _BoxColor("Box Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Blend [_SrcFactor] [_DstFactor]
        BlendOp [_Opp]

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            float _HandleY;
            float _HandleX;
            float _HandleWidth;
            float _HandleHeight;
            float4 _HandleColor;

            float4 _BallColor;
            float4 _BallPos;
            float _BallDiameter;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _BoxColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed3 col = fixed3(0,0,0);
                fixed alpha = 0;
                


                // handle
                if (i.uv.x > _HandleX && i.uv.x < _HandleX + _HandleWidth && i.uv.y > _HandleY && i.uv.y < _HandleY + _HandleHeight)
                {
                    col = _HandleColor.xyz;
                    alpha = 1;
                }

                //Ball
                fixed ballAlpha = step(pow((i.uv.x - _BallPos.x), 2) + pow((i.uv.y - _BallPos.y), 2), _BallDiameter);
                col += _BallColor.xyz * ballAlpha;
                alpha += ballAlpha;

                // Borders

                fixed leftBorder = step(i.uv.x, 0.01);
                col += fixed3(1,1,1) * leftBorder;
                alpha += leftBorder;
                
                fixed rightBorder = step(1 - i.uv.x, 0.01);
                col += fixed3(1,1,1) * rightBorder;
                alpha += rightBorder;


                fixed topBorder = step(i.uv.y, 0.01);
                col += fixed3(1,1,1) * topBorder;
                alpha += topBorder;

                fixed bottomBorder = step(1 - i.uv.y, 0.01);
                col += fixed3(1,1,1) * bottomBorder;
                alpha += bottomBorder;

                fixed4 tex = tex2D(_MainTex, i.uv);
                col += _BoxColor * tex.a;
                alpha += tex.a;
                alpha = clamp(0,1, alpha);
                
                return fixed4(col, alpha);
            }
            ENDCG
        }
    }
}
