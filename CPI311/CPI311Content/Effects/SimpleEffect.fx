float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;
float3 RedPosition;
float3 GreenPosition;
float3 BluePosition;
float4 BallColor;

// TODO: add effect parameters here.

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal	: NORMAL0;
    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color	: COLOR0;
	float3 Normal	: TEXCOORD0;
	float3 WorldPosition: TEXCOORD1;
    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.WorldPosition = 0;
	output.Normal = 0;
	//---
	float3 N = normalize(mul(input.Normal,World));
	float3 V = normalize(CameraPosition - worldPosition);
	float3 Lr = normalize(RedPosition - worldPosition);
	float3 Lg = normalize(GreenPosition - worldPosition);
	float3 Lb = normalize(BluePosition - worldPosition);
	float3 R = -reflect(normalize(Lr+Lg+Lb), N);
	output.Color = float4(max(dot(N, Lr),0), max(dot(N, Lg),0), max(dot(N, Lb),0),1) + 
					pow(max(dot(V, R), 0), 15)*0.5;
	// ---
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // TODO: add your pixel shader code here.

    return float4(input.Color.rgb,1);
}

VertexShaderOutput PhongVertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
  
	output.WorldPosition = worldPosition;
	output.Normal = normalize(mul(input.Normal,World));
	output.Color = 0;
    return output;
}

float4 PhongPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // TODO: add your pixel shader code here.

    //return float4(input.Color.rgb,1); //float4(1, 0, 0, 1);
	float3 N = normalize(input.Normal);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 Lr = normalize(RedPosition - input.WorldPosition);
	float3 Lg = normalize(GreenPosition - input.WorldPosition);
	float3 Lb = normalize(BluePosition - input.WorldPosition);
	float3 R = -reflect(normalize(Lr+Lg+Lb), N);
	float4 color = float4(max(dot(N, Lr),0), max(dot(N, Lg),0), max(dot(N, Lb),0),1) + 
					pow(max(dot(V, R), 0), 15)*0.5;
	color.a = 1;
	return color;
}

technique GouraudShading
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

technique PhongShading
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 PhongVertexShaderFunction();
        PixelShader = compile ps_2_0 PhongPixelShaderFunction();
    }
}
