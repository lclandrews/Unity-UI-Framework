Shader "UI/Default++" 
{
    Properties 
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        [Toggle(UNITY_UI_ROUNDED_CORNERS)] _RoundedCorners ("Use Round Corners", Float) = 0
        _Radii ("Corner Radii", Vector) = (0,0,0,0)
        _SizeAndBorder ("Element Size and Border Width", Vector) = (0,0,0,0)
        _BorderColor ("Border Color", Color) = (0,0,0,0)

        [Toggle(UNITY_UI_GRADIENT_FILL)] _GradientFill ("Use Gradient Fill", Float) = 0
        _GradientColor("Gradient Color", Color) = (1,1,1,1)
		_GradientAngle("Gradient Angle", Float) = 0
    }
    
    SubShader 
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass 
        {
            Name "Default++"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #include "RoundedCornerUtils.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #pragma multi_compile_local _ UNITY_UI_ROUNDED_CORNERS
            #pragma multi_compile_local _ UNITY_UI_GRADIENT_FILL

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float4 _Radii;
            float4 _SizeAndBorder;
            fixed4 _BorderColor;
            fixed4 _GradientColor;
            float _GradientAngle;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                #ifdef UNITY_UI_GRADIENT_FILL
                float r = _GradientAngle * 0.0174533;
				float y = sin(r);
				float x = cos(r);
				float a = (v.texcoord.x * x) + (v.texcoord.y * y);
				half4 gradient = lerp(float4(_GradientColor.xyz, _GradientColor.a * v.color.a), v.color, a);
				OUT.color = gradient * _Color;
                #else
                OUT.color = v.color * _Color;
                #endif

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                #ifdef UNITY_UI_ROUNDED_CORNERS
                color = GetRoundedBoxElementColor(_SizeAndBorder.xy, IN.texcoord, _Radii, _SizeAndBorder.z, color, _BorderColor);
                #endif

                return color;
            }
        ENDCG
        }
    }
}