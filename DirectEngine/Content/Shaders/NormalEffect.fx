struct VS_IN
{
	float4 pos : POSITION;
	float4 normal : NORMAL;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float3 normal : NORMAL;
	float clip : SV_ClipDistance0;
	float3 depth : TEXCOORD;
};

cbuffer PerFrame : register(b0) {
	float4 clipPlane;
};

cbuffer PerFrame : register(b1) {
	float4x4 viewProj;
	float4x4 world;
};

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	input.pos.w = 1;

	output.pos = mul(input.pos, world);
	output.pos = mul(output.pos, viewProj);
	output.depth = output.pos.xyz / 100;

	output.normal = normalize(mul((float3)input.normal, (float3x3)world));

	output.clip = dot(mul(input.pos, world), clipPlane);

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	//return float4(input.depth.z, input.depth.z, input.depth.z, 1);
	float4 color = float4(0.5f * normalize(input.normal) + float3(0.5f, 0.5f, 0.5f), input.depth.z);
	return color;
}

technique10 Render
{
	pass P0
	{
		SetGeometryShader(0);
		SetVertexShader(CompileShader(vs_4_0, VS()));
		SetPixelShader(CompileShader(ps_4_0, PS()));
	}
}

