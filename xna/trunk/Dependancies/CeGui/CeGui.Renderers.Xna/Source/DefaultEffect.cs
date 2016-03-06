using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CeGui.Renderers.Xna {

  /// <summary>Default effect that is used to render the GUI</summary>
  /// <remarks>
  ///   If no custom effect is set in the XNA CeGui renderer, the renderer will use
  ///   this effect as a fallback. It's pretty simplistic and does nothing beyond
  ///   drawing textured, colored and alpha-blended triangles to the screen using
  ///   pretransformed vertex coordinates.
  /// </remarks>
  public static class DefaultEffect {

    #region Effect file source code

    const string EffectSource =
      "// --------------------------------------------------------------------------------- //\n" +
      "// Vertex Shader\n" +
      "// --------------------------------------------------------------------------------- //\n" +
      "\n" +
      "// Vertex shader input values. These come directly from the vertex buffer(s).\n" +
      "struct VS_INPUT {\n" +
      "  float4 Position : POSITION0;\n" +
      "  float2 TextureCoordinates : TEXCOORD0;\n" +
      "  float4 Color : COLOR0;\n" +
      "};\n" +
      "\n" +
      "// Vertex shader output values. These are sent to the pixel shader.\n" +
      "struct VS_OUTPUT {\n" +
      "  float4 Position : POSITION0;\n" +
      "  float2 TextureCoordinates : TEXCOORD0;\n" +
      "  float4 Color : COLOR0;\n" +
      "};\n" +
      "\n" +
      "// The vertex shader function\n" +
      "VS_OUTPUT VertexShader(VS_INPUT Input) {\n" +
      "\n" +
      "  VS_OUTPUT Output;\n" +
      "  Output.Position = Input.Position;\n" +
      "  Output.TextureCoordinates = Input.TextureCoordinates;\n" +
      "  Output.Color = Input.Color;\n" +
      "\n" +
      "  return Output;\n" +
      "}\n" +
      "\n" +
      "// --------------------------------------------------------------------------------- //\n" +
      "// Pixel Shader\n" +
      "// --------------------------------------------------------------------------------- //\n" +
      "\n" +
      "sampler TextureSampler;\n" +
      "texture base_Tex;\n" +
      "sampler2D baseMap = sampler_state {\n" +
      "  Texture = (base_Tex);\n" +
      "  ADDRESSU = WRAP;\n" +
      "  ADDRESSV = WRAP;\n" +
      "  MINFILTER = POINT;\n" +
      "  MAGFILTER = POINT;\n" +
      "};\n" +
      "\n" +
      "// Pixel shader input values. Taken out of VS_OUTPUT.\n" +
      "struct PS_INPUT {\n" +
      "  float2 TextureCoordinates : TEXCOORD0;\n" +
      "  float4 Color : COLOR0;\n" +
      "};\n" +
      "\n" +
      "// The pixel shader function\n" +
      "float4 PixelShader(PS_INPUT Input) : COLOR0 {\n" +
      "  return tex2D(baseMap, Input.TextureCoordinates) * Input.Color;\n" +
      "};\n" +
      "\n" +
      "// --------------------------------------------------------------------------------- //\n" +
      "// TransformPhong technique\n" +
      "// --------------------------------------------------------------------------------- //\n" +
      "technique TransformPhong {\n" +
      "  pass P0 {\n" +
      "    VertexShader = compile vs_1_1 VertexShader();\n" +
      "    PixelShader = compile ps_1_1 PixelShader();\n" +
      "\n" +
      "    // Disable writing to the frame buffer\n" +
      "    AlphaBlendEnable = true;\n" +
      "    SrcBlend = SrcAlpha;\n" +
      "    DestBlend = InvSrcAlpha;\n" +
      "\n" +
      "    ZEnable = false;\n" +
      "    ZWriteEnable = false;\n" +
      "    StencilEnable = false;\n" +
      "    CullMode = None;\n" +
      "  }\n" +
      "}\n";

    #endregion

    /// <summary>Compiles the effect for the provided graphics device</summary>
    /// <param name="graphicsDevice">Graphics device to compile the effect for</param>
    /// <returns>The compiled effect</returns>
    public static Effect Compile(GraphicsDevice graphicsDevice) {

      // Use the effect compiler to compile the shader code in the effect.
      CompiledEffect compiledEffect = Effect.CompileEffectFromSource(
        EffectSource, null, null, CompilerOptions.None, TargetPlatform.Windows
      );
      if(!compiledEffect.Success)
        throw new Exception("Error compiling effect: " + compiledEffect.ErrorsAndWarnings);

      // Now create the actual effect which is bound to the graphics device
      return new Effect(
        graphicsDevice, compiledEffect.GetEffectCode(),
        CompilerOptions.None, null
      );

    }

  }

} // namespace CeGui.Renderers.Xna