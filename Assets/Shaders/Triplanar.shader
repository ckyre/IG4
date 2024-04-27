Shader "Custom/Triplanar"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _SecondaryTex ("Secondary Texture", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _SecondaryTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 tp = tex2D(_MainTex, IN.worldPos.xz * 0.1);
            fixed4 bt = tex2D(_SecondaryTex, IN.worldPos.xz * 0.1);
            fixed4 lr = tex2D(_SecondaryTex, IN.worldPos.yz * 0.1);
            fixed4 fb = tex2D(_SecondaryTex, IN.worldPos.xy * 0.1);
            
            fixed4 up_col = max(0, IN.worldNormal.y * tp);
            fixed4 bt_col = max(0, -IN.worldNormal.y * bt);
            fixed4 lr_col = IN.worldNormal.x * lr;
            fixed4 fr_col = IN.worldNormal.z * fb;
            
            // Albedo comes from a texture tinted by color
            // fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            fixed4 c = (up_col + bt_col + abs(lr_col)+ abs(fr_col )) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
