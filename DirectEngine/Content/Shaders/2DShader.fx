struct VS_IN
{
	float4 pos : POSITION;
	float4 texCoord : TEXCOORD;
	float4 normal : NORMAL;
	float4 tangent : TANGENT;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 norm : NORMAL;
	float4 texCoord : TEXCOORD;
};

cbuffer _2dData
{
	float4x4 world;
	float4 size;
};

Texture2D picture;
SamplerState pictureSampler;

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.pos = input.pos;
	output.pos.x += 1;
	output.pos.y -= 1;
	output.pos.x *= world._m00;
	output.pos.y *= world._m11;
	output.pos.x += world._m30;
	output.pos.y -= world._m31;
	//output.pos = mul(output.pos, world);
	//output.pos.z = 0;
	output.pos.x -= size.x;
	output.pos.y += size.y;
	output.pos.w = 1;
	output.pos.x /= size.x;
	output.pos.y /= size.y;
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

