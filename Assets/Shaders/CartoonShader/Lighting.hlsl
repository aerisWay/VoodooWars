#ifndef CUSTOM_LIGHTNING_INCLUDED
#define CUSTOM_LIGHTNING_INCLUDED



#ifndef SHADERGRAPH_PREVIEW
float3 CustomGlobalIllumination() {
    

}
#endif
void MainLight_float(float3 WorldPos, out float3 Direction, out float3 Color, 
	out float DistanceAtten, out float ShadowAtten)
{
#ifdef SHADERGRAPH_PREVIEW
    Direction = normalize(float3(0.5f, 0.5f, 0.25f));
    Color = half3(1.0f, 1.0f, 1.0f);
    DistanceAtten = 1.0f;
    ShadowAtten = 1.0f;
#else
    float4 clipPos = TransformWorldToHClip(WorldPos);
#if SHADOWS_SCREEN
 
    float4 shadowCoord = ComputeScreenPos(clipPos);
#else
    float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
#endif
    Light mainLight = GetMainLight(shadowCoord);

    Direction = mainLight.direction;
    Color = mainLight.color;
    DistanceAtten = mainLight.distanceAttenuation;
    ShadowAtten = mainLight.shadowAttenuation;
#endif
}

void MainLight_half(half3 WorldPos, out half3 Direction, out half3 Color, 
	out half DistanceAtten, out half ShadowAtten)
{
#ifdef SHADERGRAPH_PREVIEW
    Direction = normalize(half3(0.5f, 0.5f, 0.25f));
    Color = half3(1.0f, 1.0f, 1.0f);
    DistanceAtten = 1.0f;
    ShadowAtten = 1.0f;   
    
#else
    #if SHADOWS_SCREEN
        half4 clipPos = TransfromWorldToHClip(WorldPos);
        half4 shadowCoord = ComputeScreenPos(clipPos);
    #else
        half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
#endif

    Light mainLight = GetMainLight(shadowCoord);
    
    Direction = mainLight.direction;
    Color = mainLight.color;
    DistanceAtten = mainLight.distanceAttenuation;
    ShadowAtten = mainLight.shadowAttenuation;
#endif
}

#ifndef SHADERGRAPH_PREVIEW

Light GetAdditionalLightForToon(int PixelLightIndex, float3 WorldPosition) {
    int perObjectLightIndex = GetPerObjectLightIndex(PixelLightIndex);

    
    Light light = GetAdditionalPerObjectLight(perObjectLightIndex, WorldPosition);
        
    light.shadowAttenuation = AdditionalLightRealtimeShadow(perObjectLightIndex, WorldPosition);
    return light;
}

#endif

void AdditionalLight_float(float3 WorldPosition, float3 WorldNormal, float3 WorldView, float Smoothness, float3 SpecColor, out float3 Diffuse, out float3 Specular) {

    #ifdef SHADERGRAPH_PREVIEW

        Diffuse = (0, 0, 0);
        Specular = (0, 0, 0);
    #else
       

    float3 diffuseColor = 0;
    float3 specularColor = 0;

#ifdef _ADDITIONAL_LIGHTS
    Smoothness = exp2(10 * Smoothness + 1);
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);


    uint numAdditionalLights = GetAdditionalLightsCount();
    for (uint lightI = 0; lightI < numAdditionalLights; lightI++) {
        Light light = GetAdditionalLight(lightI, WorldPosition, half4(1, 1, 1, 1));
        float3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
        diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
        specularColor += LightingSpecular(attenuatedLightColor, light.direction, WorldNormal, WorldView, float4(SpecColor, 0), Smoothness);

    }

    Diffuse = diffuseColor;
    Specular = specularColor;

#endif

#endif
}

void AdditionalLight_half(half3 WorldPos, int Index, out half3 Direction,
	out half3 Color, out half DistanceAtten, out half ShadowAtten)
{
	Direction = normalize(half3(0.5f, 0.5f, 0.25f));
	Color = half3(0.0f, 0.0f, 0.0f);
	DistanceAtten = 0.0f;
	ShadowAtten = 0.0f;

#ifndef SHADERGRAPH_PREVIEW
	int pixelLightCount = GetAdditionalLightsCount();
	if (Index < pixelLightCount)
	{
		Light light = GetAdditionalLight(Index, WorldPos);

		Direction = light.direction;
		Color = light.color;
		DistanceAtten = light.distanceAttenuation;
		ShadowAtten = light.shadowAttenuation;
	}
#endif
}

#endif
