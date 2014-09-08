struct VS_IN
{
	float4 pos : POSITION;
	float4 norm : NORMAL;
	float4 texCoord : TEXCOORD;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 norm : NORMAL;
	float4 texCoord : TEXCOORD;
	float clip : SV_ClipDistance0;
};

cbuffer PerFrame : register(b0) {
	float4 clipPlane;
};

cbuffer portalData : register(b1) {
	float4 size;
};

cbuffer PerFrame : register(b2) {
	float4x4 world;
};

cbuffer PerFrame : register(b3) {
	float4x4 viewProj;
}

Texture2D picture;
SamplerState pictureSampler;

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	input.pos.w = 1;

	output.pos = mul(input.pos, world);
	output.pos = mul(output.pos, viewProj);
	output.texCoord = input.texCoord;

	output.clip = dot(mul(input.pos, world), clipPlane);

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float4 coord = input.pos;
	coord.x /= size.x;
	coord.y /= size.y;
	float4 diffuse = picture.Sample(pictureSampler, coord);
		//clip(diffuse.a - 0.25);
	return diffuse;
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

