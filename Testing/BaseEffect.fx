﻿struct VS_IN
{
	float4 pos : POSITION;
	float4 texCoord : TEXCOORD;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 texCoord : TEXCOORD;
};

float4x4 worldViewProj;
Texture2D picture;
SamplerState pictureSampler;

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.pos = mul(input.pos, worldViewProj);
	output.texCoord = input.texCoord;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float4 diffuse = picture.Sample(pictureSampler, input.texCoord);
	clip(diffuse.a - 0.25);
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

