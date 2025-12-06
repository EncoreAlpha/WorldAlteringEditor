#pragma enable_d3d11_debug_symbols

#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

// Shader for rendering terrain.
// This is a less capable, but lighter copy of PalettedColorDraw.

Texture2D MainTexture;
SamplerState MainSampler
{
    Texture = <MainTexture>;
    AddressU = clamp;
    AddressV = clamp;
    MipFilter = Point;
    MinFilter = Point;
    MagFilter = Point;
};

Texture2D PaletteTexture;
SamplerState PaletteSampler
{
    Texture = <PaletteTexture>;
    AddressU = clamp;
    AddressV = clamp;
    MipFilter = Point;
    MinFilter = Point;
    MagFilter = Point;
};

// Vertex shader input
struct VertexShaderInput
{
    float3 Position : POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

matrix WorldViewProj : register(c0);

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};


VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(float4(input.Position, 1.0), WorldViewProj);
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}


struct PixelShaderOutput
{
    float4 color : SV_Target0;
};


PixelShaderOutput MainPS(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput) 0;

    // We need to read from the main texture first,
    // otherwise the output will be black!
    float4 tex = MainTexture.Sample(MainSampler, input.TextureCoordinates);

    // Discard transparent areas
    clip(tex.a == 0.0f ? -1 : 1);

    // Abuse alpha component of color to determine whether we should render with a palette or not
    if (input.Color.a >= 1.0f)
    {
        output.color = float4(tex.r * input.Color.r, tex.g * input.Color.g, tex.b * input.Color.b, 1.0f);
    }
    else
    {
        // Get color from palette
        float4 paletteColor = PaletteTexture.Sample(PaletteSampler, float2(tex.a, 0.5));

        // Multiply the color by 2. This is done because unlike map lighting which can exceed 1.0 and go up to 2.0,
        // the color values passed in the pixel shader input are capped at 1.0.
        // So the multiplication is done to translate pixel shader input color space into in-game color space.
        // We lose a bit of precision from doing this, but we'll have to accept that.
        output.color = float4(paletteColor.r * input.Color.r * 2.0,
                    paletteColor.g * input.Color.g * 2.0,
                    paletteColor.b * input.Color.b * 2.0,
                    paletteColor.a);
    }

    return output;
}

technique SpriteDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};