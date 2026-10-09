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
 * @file skyboxcube.fx
 *
 * Cubemap skybox: the alternative to skybox.fx.
 *
 * A cube map is six square faces stored as one texture. The graphics hardware
 * samples it with a *direction vector* (samplerCUBE / texCUBE): it picks the
 * face the direction points at and converts the direction to a (u,v) on that
 * face for you. That means the pixel shader does no trigonometry at all - one
 * texCUBE call - which is why cube maps are the traditional, cheap skybox.
 *
 * We keep this as an interchangeable strategy so the project can switch between
 * a single equirectangular panorama (skybox.fx) and a baked cube map without
 * touching the scene. The cube map is produced at load time from the same
 * panorama by CubeMapBaker.
 *
 * State notes are identical to skybox.fx: the sphere is camera-centred, the
 * vertex is pushed to the far plane, and translation is stripped from the view.
 */

float4x4 RotationProjection;

texture SkyCube;

samplerCUBE SkyCubeSampler = sampler_state
{
    Texture   = < SkyCube >;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    // Across face edges the direction may land between two faces; clamping
    // makes the sampler return the shared edge value rather than wrapping.
    AddressU  = CLAMP;
    AddressV  = CLAMP;
    AddressW  = CLAMP;
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

VS_OUTPUT SkyCubeVS(VS_INPUT input)
{
    VS_OUTPUT output;
    output.Position = mul(float4(input.Position, 1.0f), RotationProjection);
    output.Position.z = output.Position.w;   // push to the far plane
    output.Direction = input.Position;       // radius-1 sphere: position == direction
    return output;
}

float4 SkyCubePS(VS_OUTPUT input) : COLOR0
{
    // The hardware resolves the direction to a face + (u,v) and filters there.
    return float4(texCUBE(SkyCubeSampler, normalize(input.Direction)).rgb, 1.0);
}

technique SkyboxCube
{
    pass P0
    {
        CULLMODE = NONE;

        VertexShader = compile vs_3_0 SkyCubeVS();
        PixelShader  = compile ps_3_0 SkyCubePS();
    }
}
