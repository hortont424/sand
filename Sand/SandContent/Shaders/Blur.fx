float BlurDistance = 0.000001f;
float2 BlurOffset;
 
sampler ColorMapSampler : register(s0);

float4 PixelShaderFunction(float2 Tex: TEXCOORD0) : COLOR
{
	float4 Color;
	float2 pos = Tex.xy + BlurOffset.xy;
	int num = 9;

	Color  = tex2D( ColorMapSampler, float2(pos.x+BlurDistance, pos.y+BlurDistance)) / num;
	Color += tex2D( ColorMapSampler, float2(pos.x-BlurDistance, pos.y-BlurDistance)) / num;
	Color += tex2D( ColorMapSampler, float2(pos.x+BlurDistance, pos.y-BlurDistance)) / num;
	Color += tex2D( ColorMapSampler, float2(pos.x-BlurDistance, pos.y+BlurDistance)) / num;

	Color += tex2D( ColorMapSampler, float2(pos.x, pos.y)) / num;
	Color += tex2D( ColorMapSampler, float2(pos.x-BlurDistance, pos.y)) / num;
	Color += tex2D( ColorMapSampler, float2(pos.x+BlurDistance, pos.y)) / num;
	Color += tex2D( ColorMapSampler, float2(pos.x, pos.y+BlurDistance)) / num;
	Color += tex2D( ColorMapSampler, float2(pos.x, pos.y-BlurDistance)) / num;

	return Color;
}

technique Blur
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}

technique None
{
	pass Pass1
	{
	}
}