struct VS_IN
{
	float4 pos : POSITION;
	float4 texCoord : TEXCOORD;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float2 texCoord : TEXCOORD;
};

cbuffer Size : register(b0){
	float4 size;
};

Texture2D picture;
SamplerState pictureSampler;

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.pos = float4(input.pos.xy, 0, 1);
	output.texCoord = input.texCoord.xy;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float tx = input.texCoord.x;
	float ty = input.texCoord.y;
	float invW = size.z;
	float invH = size.w;

	float4 ul = picture.Sample(pictureSampler, float2(tx - invW, ty - invH));
	float4 uc = picture.Sample(pictureSampler, float2(tx, ty - invH));
	float4 ur = picture.Sample(pictureSampler, float2(tx + invW, ty - invH));
	float4 cl = picture.Sample(pictureSampler, float2(tx - invW, ty));
	float4 cc = picture.Sample(pictureSampler, float2(tx, ty));
	float4 cr = picture.Sample(pictureSampler, float2(tx + invW, ty));
	float4 dl = picture.Sample(pictureSampler, float2(tx - invW, ty + invH));
	float4 dc = picture.Sample(pictureSampler, float2(tx, ty + invH));
	float4 dr = picture.Sample(pictureSampler, float2(tx + invW, ty + invH));

	float4 average = (ul + uc + ur + cl + cr + dl + dc + dr) / 8.0f;
	float4 diff = cc - average;

	float range = 0.1f;
	float depthRange = 0.001f;

	if (diff.x >= -range && diff.x <= range &&
		diff.y >= -range && diff.y <= range &&
		diff.z >= -range && diff.z <= range &&
		diff.w >= -depthRange && diff.w <= depthRange)
	{
		return float4(1, 1, 1, 1);
	}
	else
	{
		return float4(0, 0, 0, 1);
	}
	return float4(0, 0, 0, 0);
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

