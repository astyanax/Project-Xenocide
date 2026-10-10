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
 *
 * Renders the Earth globe on the geoscape.
 *
 * Two techniques:
 *   RenderGlobeWithBump  (preferred) - normal-mapped day/night lighting,
 *                                      ocean specular and an atmospheric rim.
 *   RenderGlobeStandard  (fallback)  - flat texture, no lighting.
 *
 * Lighting model (classic Blinn-Phong against a directional "sun"):
 *   L = direction from the surface toward the sun (the LightDirection input is
 *       negated in the vertex shader for this reason)
 *   N = world-space surface normal, perturbed by the normal map
 *   V = direction from the surface toward the eye = -ViewDirection
 *   H = normalize(L + V)  (half-vector for the specular term)
 *
 * The day/night terminator uses smoothstep (rather than a hard clamp) so the
 * shadow line and the city lights fade in gradually instead of banding.
 */

float4x4 World;
float4x4 View;
float4x4 Projection;

Texture GeoscapeTexture;
Texture NightTexture;
Texture NormalMapTexture;

// Direction the sunlight travels (world space). Negated in the vertex shader.
float3  LightDirection;

// Tunables (set from C# so the look can be adjusted without recompiling art).
float   Ambient;              // minimum lit colour on the night side
float   SunIntensity;         // sunlight strength
float3  AtmosphereColor;      // rim/limb glow tint
float   RimPower;             // higher = thinner atmosphere glow
float   SpecularPower;        // higher = tighter ocean highlight
float   SpecularIntensity;    // ocean highlight strength
float   FresnelIntensity;     // how strongly grazing water reflects the atmosphere

// The Earth textures are equirectangular (longitude/latitude), so U repeats
// around the globe (WRAP) and V is clamped at the poles (CLAMP).
sampler GeoscapeTextureSampler = sampler_state
{
    texture   = <GeoscapeTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU  = wrap;
    AddressV  = clamp;
};

sampler NightTextureSampler = sampler_state
{
    texture   = <NightTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU  = wrap;
    AddressV  = clamp;
};

sampler NormalMapTextureSampler = sampler_state
{
    texture   = <NormalMapTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU  = wrap;
    AddressV  = clamp;
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
    float4 Position         : POSITION0;
    float2 TexCoord         : TEXCOORD0;
    float3 LightDirection   : TEXCOORD1;  // direction toward the sun (world space)
    float3 ViewDirection    : TEXCOORD2;  // eye -> surface (world space)
    float3x3 TangentToWorld : TEXCOORD3;
    float3 GeometryNormal   : TEXCOORD6;  // smooth sphere normal (no normal map)
    float4 ClipPos          : TEXCOORD7;  // for the deband dither (see pixel shader)
};

VS_OUTPUT TransformGlobe(VS_INPUT Input)
{
    float4x4 WorldViewProjection = mul(mul(World, View), Projection);

    VS_OUTPUT Output;
    Output.Position           = mul(Input.Position, WorldViewProjection);
    Output.Normal             = mul(Input.Normal, (float3x3)World);
    Output.Texcoord           = Input.TexCoord;
    Output.LightDirection.xyz = -LightDirection;   // negate: travel -> towards light
    Output.LightDirection.w   = 1;
    return Output;
}

VS_OUTPUT_WITH_BUMP TransformGlobeWithBump(VS_INPUT Input)
{
    VS_OUTPUT_WITH_BUMP Output;

    // Transform the position into projection space.
    float4 worldSpacePos = mul(Input.Position, World);
    Output.Position = mul(worldSpacePos, View);
    Output.Position = mul(Output.Position, Projection);

    // Direction toward the sun (LightDirection is where the light travels from).
    Output.LightDirection = -LightDirection;

    // Eye position derived from the view matrix, then the eye->surface ray.
    float3 eyePosition = mul(-View._m30_m31_m32, (float3x3)transpose(View));
    Output.ViewDirection = worldSpacePos.xyz - eyePosition;

    // Tangent space -> world space basis (columns are the world-space T/B/N).
    Output.TangentToWorld[0] = mul(Input.Tangent, (float3x3)World);
    Output.TangentToWorld[1] = mul(Input.Binormal, (float3x3)World);
    Output.TangentToWorld[2] = mul(Input.Normal, (float3x3)World);

    // Smooth (unperturbed) world-space normal. On this unit sphere the vertex
    // normal equals its position, so this is just the sphere's own normal.
    Output.GeometryNormal = mul(Input.Normal, (float3x3)World);

    Output.ClipPos = Output.Position;

    Output.TexCoord = Input.TexCoord;

    return Output;
}

struct PS_OUTPUT { float4 Color : COLOR0; };

PS_OUTPUT RenderGlobe(VS_OUTPUT Input)
{
    PS_OUTPUT Output = (PS_OUTPUT)0;

    // Low-spec fallback: flat texture, no lighting.
    Output.Color = tex2D(GeoscapeTextureSampler, Input.Texcoord);
    Output.Color.a = 1.0;

    return Output;
};

PS_OUTPUT RenderGlobeWithBump(VS_OUTPUT_WITH_BUMP Input)
{
    PS_OUTPUT Output;

    // Decode the normal map (stored 0..1) and bring it into world space.
    float3 mapNormal = tex2D(NormalMapTextureSampler, Input.TexCoord).xyz * 2.0 - 1.0;
    mapNormal = normalize(mul(mapNormal, Input.TangentToWorld));

    // Smooth sphere normal. Used (a) over water and (b) near the poles so that
    // the normal map's bathymetry/relief never creates glints on the ocean and
    // its degenerate tangent frame never creates a starburst at the poles.
    float3 geometryNormal = normalize(Input.GeometryNormal);

    float3 L = normalize(Input.LightDirection);   // surface -> sun
    float3 V = normalize(-Input.ViewDirection);   // surface -> eye
    float3 H = normalize(L + V);                  // Blinn-Phong half vector

    float4 diffuse = tex2D(GeoscapeTextureSampler, Input.TexCoord);
    float4 night   = tex2D(NightTextureSampler, Input.TexCoord);

    // Ocean mask: the sea is the "bluest" part of the day texture.
    float water = saturate((diffuse.b - max(diffuse.r, diffuse.g)) * 2.0);

    // Fade the normal map out near the poles and over water so only land away
    // from the poles uses the terrain relief. The southern fade is wide because
    // the normal map's Antarctic data is noisy/streaky; using it there just
    // produced a jagged dark band around the coast.
    float poleFade = smoothstep(0.0, 0.05, Input.TexCoord.y) * smoothstep(1.0, 0.82, Input.TexCoord.y);
    float relief = poleFade * (1.0 - water);
    float3 N = normalize(lerp(geometryNormal, mapNormal, relief));

    float dotL = dot(N, L);

    // Soft terminator. A narrow band (lit by ~84 degrees from the sub-solar
    // point, dark by ~91) reads as a day/night line rather than a broad murky
    // twilight zone, while smoothstep keeps the gradient gradual.
    float sunlight = smoothstep(-0.03, 0.10, dotL) * SunIntensity;

    // City lights fade in from just before the terminator (dotL = 0.02) to the
    // dark side, so they do not leak into daylight but the transition to the lit
    // night side (whose texture has a soft base glow) is continuous.
    float nightTerm = 1.0 - smoothstep(-0.10, 0.02, dotL);

    // Ocean glint follows the smooth sphere (not the terrain), giving a single
    // sun highlight that tracks the sun instead of scattered bathymetry glints.
    float specular = pow(saturate(dot(geometryNormal, H)), SpecularPower) * SpecularIntensity * water * sunlight;

    // Atmospheric limb glow: strongest where the surface faces away from the eye.
    float rim = pow(1.0 - saturate(dot(geometryNormal, V)), RimPower) * sunlight;

    float3 color = diffuse.rgb * max(sunlight, Ambient);
    color += night.rgb * nightTerm;                    // city lights on the dark side
    color += AtmosphereColor * rim * 0.6;              // blue atmosphere edge
    color += specular.xxx;                             // white ocean highlight

    // Sunlit oceans are lifted slightly by atmospheric scattering in the source
    // imagery; without it the day-side water reads almost as dark as the night.
    color += AtmosphereColor * water * sunlight * 0.10;

    // Atmospheric terminator glow: a soft band centred on the day/night boundary.
    // The night texture carries a faint base glow, so without this the boundary is
    // a dark stripe that is actually darker than the night side it borders.
    color += AtmosphereColor * exp(-(dotL * dotL) * 25.0) * 0.09;

    // Fresnel reflection (Schlick approximation, F0 ~= 0.02 for water): water is
    // far more reflective at grazing angles, so the ocean picks up the sky /
    // atmosphere colour toward the limb. Based on the smooth sphere normal and
    // the water mask so it never appears on land.
    float fresnel = 0.02 + 0.98 * pow(1.0 - saturate(dot(geometryNormal, V)), 5.0);
    color = lerp(color, AtmosphereColor, saturate(fresnel * water * sunlight * FresnelIntensity));

    // Deband: the night/terminator sits only a few levels above black, so its
    // smooth gradient quantizes into visible bands (and the source JPEG's
    // blocking shows through). A sub-LSB ordered dither breaks those steps up.
    // The screen-space position varies per pixel, so hashing it gives noise.
    float dither = frac(sin(dot(Input.ClipPos.xy, float2(12.9898, 78.233))) * 43758.5453);
    color += (dither - 0.5) * (1.0 / 255.0);

    Output.Color = float4(color, 1.0);
    return Output;
};

technique RenderGlobeStandard
{
    pass P0
    {
         VertexShader = compile vs_3_0 TransformGlobe();
         PixelShader = compile ps_3_0 RenderGlobe();
    }
}

technique RenderGlobeWithBump
{
    pass P0
    {
        // Shader model 3.0: the normal-mapped lighting + ocean/atmosphere terms
        // exceed the 64-instruction budget of ps_2_0. DesktopGL always reports
        // shader model 3 (Util.GetShaderVersion), so this is the path taken.
        VertexShader = compile vs_3_0 TransformGlobeWithBump();
        PixelShader = compile ps_3_0 RenderGlobeWithBump();
    }
}
