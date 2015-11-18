Shader "Custom/Crosshair" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
		_Crosshair ("Crosshair", 2D) = "" {}
	}
	SubShader {
		Tags { "Queue" = "Transparent" }
		Pass {	 		
	 		Blend SrcAlpha OneMinusSrcAlpha
	 		
	 		SetTexture[_MainTex]
			SetTexture[_Crosshair] {
				Combine texture
			}
		}		
	} 
	FallBack off
}
