﻿Shader "Custom/Transparent Colored Overlay" {
    Properties{
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
    }

        SubShader{
            Tags 
            {
                "Queue" = "Transparent" 
                "IgnoreProjector" = "True" 
                "RenderType" = "Transparent"
            }

            ZTest Always
            Cull Off
            Lighting Off
            ZWrite On
            Fog { Mode Off }
            Blend SrcAlpha OneMinusSrcAlpha

            Pass {
                Color[_Color]
                SetTexture[_MainTex] { combine texture * primary }
            }
    }
}