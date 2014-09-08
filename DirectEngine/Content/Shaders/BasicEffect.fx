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
	float4 texCoord : TEXCOORD;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
	float3 viewDirection : TEXCOORD1;
	float clip : SV_ClipDistance0;
};

cbuffer PerFrame : register(b0) {
	float4 clipPlane;
};

cbuffer PerFrame : register(b1) {
	float4 lightDir;
	float4 lightColor;
};

cbuffer PerFrame : register(b2) {
	float4x4 world;
};

cbuffer PerFrame : register(b3) {
	float4x4 viewProj;
	float4 cameraPos;
}

Texture2D picture;
Texture2D normalMap;
SamplerState pictureSampler;

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	input.pos.w = 1;

	output.pos = mul(input.pos, world);
	output.pos = mul(output.pos, viewProj);
	output.texCoord = input.texCoord;

	output.normal = normalize(mul((float3)input.normal, (float3x3)world));
	output.tangent = normalize(mul((float3)input.tangent, (float3x3)world));
	output.viewDirection = normalize(cameraPos.xyz - output.pos.xyz);

	output.clip = dot(mul(input.pos, world), clipPlane);

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float4 texColor = picture.Sample(pictureSampler, input.texCoord);
	clip(texColor.a - 0.25);

	float3 lDir = normalize((float3) - lightDir);

		//if (hasNormMap == true)
		//{
		//Load normal from normal map
		float4 normalMapCol = normalMap.Sample(pictureSampler, input.texCoord);

		//Change normal map range from [0, 1] to [-1, 1]
		normalMapCol = (2.0f*normalMapCol) - 1.0f;

	//Make sure tangent is completely orthogonal to normal
	input.tangent = normalize(input.tangent - dot(input.tangent, input.normal) * input.normal);

	//Create the biTangent
	float3 biTangent = cross(input.normal, input.tangent);

		input.normal = (normalMapCol.x * input.tangent) + (normalMapCol.y * biTangent) + (normalMapCol.z * input.normal);
	input.normal = normalize(input.normal);
	//}

	float lightIntensity = saturate(dot(input.normal, lDir));
	float4 color = saturate(lightColor * lightIntensity);
		color = saturate(color + float4(0.3f, 0.3f, 0.3f, 0.0f));
	color.a = texColor.a;

	// Calculate the reflection vector based on the light intensity, normal vector, and light direction.
	float3 reflection = normalize(2 * lightIntensity * input.normal - lDir);
		// Determine the amount of specular light based on the reflection vector, viewing direction, and specular power.
		input.viewDirection = normalize(input.viewDirection);
	float4 specular = pow(saturate(dot(reflection, input.viewDirection)), 50);

		color = saturate(color * texColor);
	color = saturate(color + specular);

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

