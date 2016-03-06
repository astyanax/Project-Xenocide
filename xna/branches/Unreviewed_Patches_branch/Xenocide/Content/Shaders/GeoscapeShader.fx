/*
--------------------------------------------------------------------------------
This source file is part of Xenocide
  by  Project Xenocide Team

For the latest info on Xenocide, see http://www.projectxenocide.com/

This work is licensed under the Creative Commons
Attribution-NonCommercial-ShareAlike 2.5 License.

To view a copy of this license, visit
http://creativecommons.org/licenses/by-nc-sa/2.5/
or send a letter to Creative Commons, 543 Howard Street, 5th Floor,
San Francisco, California, 94105, USA.
--------------------------------------------------------------------------------
*/

/*
* @file GeoscapeShader.fx
* @date Created: 2007/08/23
* @author File creator: dteviot
* @author Credits: riemer, hazymind
*/

/*
This is the set of shaders that render the Geoscape scene
*/

float4x4 World;
float4x4 View;
float4x4 Projection;

Texture GeoscapeTexture;
Texture NightTexture;
Texture NormalMapTexture;

float3  LightDirection;

sampler GeoscapeTextureSampler = sampler_state
{
    texture   = <GeoscapeTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU  = mirror;
    AddressV  = mirror;
};

sampler NightTextureSampler = sampler_state
{
    texture   = <NightTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU  = mirror;
    AddressV  = mirror;
};

sampler NormalMapTextureSampler = sampler_state
{
    texture   = <NormalMapTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU  = mirror;
    AddressV  = mirror;
};


struct VS_INPUT 
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float3 Tangent  : TANGENT0;
    float3 Binormal : BINORMAL0;
    float2 TexCoord : TEXCOORD0; 
};

struct VS_OUTPUT
{
    float4 Position       : POSITION0;
    float3 Normal         : TEXCOORD0;
    float2 Texcoord       : TEXCOORD1;
    float4 LightDirection : TEXCOORD2;
};

struct VS_OUTPUT_WITH_BUMP
{
    float4 Position			   : POSITION0;
    float2 TexCoord            : TEXCOORD0;
    float3 LightDirection	   : TEXCOORD1;
    float3 ViewDirection	   : TEXCOORD2;
    float3x3 TangentToWorld    : TEXCOORD3;
};

VS_OUTPUT TransformGlobe(VS_INPUT Input)
{
    float4x4 WorldViewProjection = mul(mul(World, View), Projection);

    VS_OUTPUT Output;
    Output.Position           = mul(Input.Position, WorldViewProjection);
    Output.Normal             = mul(Input.Normal, World);
    Output.Texcoord           = Input.TexCoord;
    Output.LightDirection.xyz = -LightDirection;
    Output.LightDirection.w   = 1;
    return Output;
}

VS_OUTPUT_WITH_BUMP TransformGlobeWithBump(VS_INPUT Input)
{
    VS_OUTPUT_WITH_BUMP Output;
		
    // Transform the position into projection space
    float4 worldSpacePos = mul(Input.Position, World);
    Output.Position = mul(worldSpacePos, View);
    Output.Position = mul(Output.Position, Projection);

    Output.LightDirection.xyz = -LightDirection;

    // Similarly, calculate the view direction, from the eye to the surface.  
    // Not normalized, in world space.
    float3 eyePosition = mul(-View._m30_m31_m32, transpose(View));    
    Output.ViewDirection = worldSpacePos - eyePosition;  

	// Calculate tangent space to world space matrix using the world space tangent,
    // binormal, and normal as basis vectors.  the pixel shader will normalize these
    // in case the world matrix has scaling.
    Output.TangentToWorld[0] = mul(Input.Tangent, World);
    Output.TangentToWorld[1] = mul(Input.Binormal, World);
    Output.TangentToWorld[2] = mul(Input.Normal, World);
    
    Output.TexCoord = Input.TexCoord; 
   
    return Output;
}

struct PS_OUTPUT { float4 Color : COLOR0; };

PS_OUTPUT RenderGlobe(VS_OUTPUT Input) : COLOR0
{
    PS_OUTPUT Output = (PS_OUTPUT)0;

    // ToDo: I just can't get this to work with Shader 1.1.
    // ambient is hard coded to 0.25
    // and we multiply sunlight by 3 to quickly ramp to full daylight.
    // float sunlight = dot(Input.Normal, Input.LightDirection);
    // Output.Color = tex2D(GeoscapeTextureSampler, Input.Texcoord) * clamp(sunlight * 3, 0.25, 1.0);
    Output.Color = tex2D(GeoscapeTextureSampler, Input.Texcoord);

    // make sure the base texture's alpha is always 100% 
    Output.Color.a = 1.0;

    return Output;
};

PS_OUTPUT RenderGlobeWithBump(VS_OUTPUT_WITH_BUMP Input) : COLOR0
{
    PS_OUTPUT Output = (PS_OUTPUT)1;

    // Look up the normal from the normal map, and transform from tangent space
    // into world space using the matrix created above.  normalize the result
    // in case the matrix contains scaling.
    float3 normalFromMap = tex2D(NormalMapTextureSampler, Input.TexCoord);
    normalFromMap = mul(normalFromMap, Input.TangentToWorld);
    normalFromMap = normalize(normalFromMap);

    // Interpolation can mess up with our normalization, so we normalize again.
    Input.ViewDirection = normalize(Input.ViewDirection);
    Input.LightDirection = normalize(Input.LightDirection);

    // figure out if pixel is in day or night
    float sunangle = dot(Input.TangentToWorld[2], Input.LightDirection);

    float4 bumpTexture = tex2D(NormalMapTextureSampler, Input.TexCoord);
   
	// ambient light is the amount of light if the sun isn't shineing.
	const float AMBIENT_DAY = 0.7 - bumpTexture;
	// how light is night?  (how bright to make the lights at it's peak)
	const float AMBIENT_NIGHT = 1.0;
	// how much lighter than night is day?
	const float DAYLIGHT_UPLIFT = 0.7;
	// How big is the dawn / dusk transistion area?
    const float DUSK_DAWN_MARGIN = 0.2;
    
    float dayTerm  = AMBIENT_DAY;
    float nightTerm = 0;
    
    if (sunangle > DUSK_DAWN_MARGIN)  // it's DayTime
		dayTerm+= DAYLIGHT_UPLIFT; 
		
	else if (sunangle  < -DUSK_DAWN_MARGIN) // it's NightTime
	{
		 nightTerm = AMBIENT_NIGHT;
	}	 
	else  // it's Dawn / Dusk
	{
		float duskDawnModifier = (sunangle + DUSK_DAWN_MARGIN) * (1 /(DUSK_DAWN_MARGIN * 2));
		
		dayTerm += DAYLIGHT_UPLIFT * duskDawnModifier;
		nightTerm = 1 - (1 * duskDawnModifier);
		
		if( dayTerm < AMBIENT_DAY)
			dayTerm = AMBIENT_DAY;
			
		if (nightTerm > AMBIENT_NIGHT)
			nightTerm = AMBIENT_NIGHT;
		 
	}
	
	// Base Color
    float4 diffuseTexture = tex2D(GeoscapeTextureSampler, Input.TexCoord);
	float4 nightTexture = tex2D(NightTextureSampler, Input.TexCoord);
	
	

   Output.Color =  (diffuseTexture * dayTerm) + (nightTexture * nightTerm)  ;
   
   
    //Output.Color= (float4)normalFromMap;
    // Make sure the base texture's alpha is always 100% 
    Output.Color.a = 1.0;

    return Output;
};








technique RenderGlobeStandard
{
    pass P0
    {
        VertexShader = compile vs_1_1 TransformGlobe();
        PixelShader = compile ps_1_1 RenderGlobe();
    }
}

technique RenderGlobeWithBump
{
    pass P0
    {
        VertexShader = compile vs_2_0 TransformGlobeWithBump();
        PixelShader = compile ps_2_0 RenderGlobeWithBump();
    }
}
