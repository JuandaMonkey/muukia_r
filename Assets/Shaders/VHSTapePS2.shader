Shader "MUUKIA/VHSTapePS2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _DistortionIntensity ("Distortion Intensity", Range(0, 1)) = 0.05
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.35
        _Vignette ("Vignette", Range(0, 1)) = 0.4
        _ColorJitter ("Color Jitter", Range(0, 0.05)) = 0.005
        _GlitchSpike ("Glitch Spike", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "IgnoreProjector"="True" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

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

            sampler2D _MainTex;
            float _DistortionIntensity;
            float _ScanlineIntensity;
            float _Vignette;
            float _ColorJitter;
            float _GlitchSpike;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Pseudo-random noise
            float rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float t = _Time.y;

                // Tape tracking jitter horizontal
                float jitter = (rand(float2(t, floor(uv.y * 80.0))) - 0.5) * _DistortionIntensity;
                if (_GlitchSpike > 0.01)
                {
                    // Sudden glitch horizontal tear
                    float tear = sin(uv.y * 30.0 + t * 40.0) * _GlitchSpike * 0.08;
                    jitter += tear;
                }
                uv.x += jitter;

                // Scanline bar scrolling down
                float bar = sin(uv.y * 3.0 - t * 2.0);
                float barNoise = (bar > 0.95) ? 0.08 * _DistortionIntensity : 0.0;

                // CRT Scanlines
                float scanline = sin(uv.y * 480.0 * 3.14159) * _ScanlineIntensity;

                // Chromatic Aberration / RGB Split
                float shift = _ColorJitter + (_GlitchSpike * 0.02);
                float r = rand(uv + float2(shift, 0.0));
                float g = rand(uv);
                float b = rand(uv - float2(shift, 0.0));

                // PS2 Muted / Desaturated Noise Color
                float noise = (rand(uv * 100.0 + float2(t * 15.0, t * 23.0)) - 0.5) * 0.2;
                noise += barNoise;

                // Vignette dark edges
                float2 vigCoord = (i.uv - 0.5) * 2.0;
                float vig = 1.0 - dot(vigCoord, vigCoord) * _Vignette;
                vig = clamp(vig, 0.0, 1.0);

                // Base color with noise & scanlines
                fixed4 col;
                col.r = 0.05 + noise * 0.5 + (_GlitchSpike * 0.15);
                col.g = 0.05 + noise * 0.5;
                col.b = 0.06 + noise * 0.6;
                col.a = clamp(0.12 + (_ScanlineIntensity * 0.25) + (_GlitchSpike * 0.45) + (1.0 - vig) * 0.4, 0.0, 0.95);

                return col;
            }
            ENDCG
        }
    }
}
