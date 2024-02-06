Shader "OCB/ButtonShader"
{
    Properties
    {
        _Emission ("Emission", Color) = (0,0,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM

        // Create variant to enable emission
        #pragma multi_compile _ EMISSION_ON
        // Enable fog shader variants
        // #pragma multi_compile_fog

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
        // Can't define an empty struct
            float __unused__;
        };

        // Material configs
        half _Glossiness;
        half _Metallic;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Emission)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Get the instanced color property via unity macro
            fixed4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Emission);
            o.Albedo = color.rgb;
            #ifdef EMISSION_ON
            o.Emission = o.Albedo;
            #endif
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
