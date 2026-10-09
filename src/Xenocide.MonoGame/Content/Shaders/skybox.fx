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
 * @file skybox.fx
 *
 * Equirectangular skybox.
 *
 * ----------------------------------------------------------------------------
 * WHY THIS SHADER EXISTS
 * ----------------------------------------------------------------------------
 * The sky panorama (skybox.png) is a single 2:1 "equirectangular" image: the
 * horizontal axis is longitude (0..360 degrees, wrapping at the edges) and the
 * vertical axis is latitude (north pole at the top, south pole at the bottom).
 * It is NOT a cube-map atlas, so it must not be sampled with the fixed UV
 * rectangles a cube cross would use.
 *
 * Instead we draw a unit sphere centred on the camera and, per pixel, convert
 * the pixel's view direction into a longitude/latitude pair, then sample the
 * panorama there. Because the mapping is done from the *direction* rather than
 * from mesh UVs, the result is seamless no matter how the sphere is tessellated
 * (even a low-poly sphere looks right).
 *
 * ----------------------------------------------------------------------------
 * THE MATHEMATICS
 * ----------------------------------------------------------------------------
 * For a normalised direction d = (x, y, z):
 *
 *   longitude (yaw)   = atan2(z, x)          -> -PI .. +PI
 *   latitude  (pitch) = acos(y)              -> 0 (north) .. PI (south)
 *
 * Normalise to texture coordinates:
 *
 *   u = atan2(z, x) / (2*PI) + 0.5           -> 0 .. 1 (longitude wraps)
 *   v = acos(y) / PI                         -> 0 (top) .. 1 (bottom)
 *
 * Direction vectors used for sampling are taken from the sphere's *object
 * space* position (the sphere has radius 1 and no world transform), which is
 * exactly the ray from the camera to that point on the sky.
 *
 * ----------------------------------------------------------------------------
 * STATE NOTES
 * ----------------------------------------------------------------------------
 *  - The vertex shader pushes each vertex to the far plane (z = w) so the sky
 *    is always behind scene geometry, and translation is stripped from the view
 *    matrix on the C# side (a sky has no parallax).
 *  - The sampler wraps in U so the left/right edges of the panorama join, and
 *    clamps in V so the poles do not bleed past the image.
 */

// Rotation-only view matrix * projection (translation removed on the C# side).
float4x4 RotationProjection;

texture SkyMap;

sampler SkyMapSampler = sampler_state
{
    Texture   = < SkyMap >;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    // Longitude (U) repeats around the sphere; latitude (V) clamps at the poles.
    AddressU  = WRAP;
    AddressV  = CLAMP;
};

struct VS_INPUT
{
    float3 Position : POSITION0;
};

struct VS_OUTPUT
{
    float4 Position  : POSITION0;
    float3 Direction : TEXCOORD0;
};

VS_OUTPUT SkyVS(VS_INPUT input)
{
    VS_OUTPUT output;

    // Place the vertex; no world matrix needed because the sphere is already
    // centred on the camera (in the stripped view space) at the origin.
    output.Position = mul(float4(input.Position, 1.0f), RotationProjection);

    // Force the depth to the far plane. After the perspective divide,
    // z/w == 1.0, the maximum depth value, so everything else draws in front.
    output.Position.z = output.Position.w;

    // The object-space position IS the view direction (radius-1 sphere,
    // centred on the camera). Interpolating it and normalising per pixel gives
    // the exact ray for each fragment.
    output.Direction = input.Position;

    return output;
}

float4 SkyPS(VS_OUTPUT input) : COLOR0
{
    const float PI = 3.14159265358979323846;

    float3 dir = normalize(input.Direction);

    // Longitude -> U (see the header comment). atan2 returns -PI..PI.
    float u = atan2(dir.z, dir.x) / (2.0 * PI) + 0.5;

    // Latitude -> V. acos(1) = 0 (north pole, top of the image),
    // acos(-1) = PI (south pole, bottom). clamp guards against FP error > 1.
    float v = acos(clamp(dir.y, -1.0, 1.0)) / PI;

    return float4(tex2D(SkyMapSampler, float2(u, v)).rgb, 1.0);
}

technique Skybox
{
    pass P0
    {
        // We are inside the sphere, so draw both faces (winding independent).
        CULLMODE = NONE;

        VertexShader = compile vs_3_0 SkyVS();
        PixelShader  = compile ps_3_0 SkyPS();
    }
}
