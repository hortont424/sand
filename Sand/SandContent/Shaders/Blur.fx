 float BlurDistance = 0.002f;
 sampler ColorMapSampler : register(s0);

 float4 PixelShaderFunction(float2 Tex: TEXCOORD0) : COLOR
 {
  float4 Color;

  // Get the texel from ColorMapSampler using a modified texture coordinate. This
  // gets the texels at the neighbour texels and adds it to Color.
  Color  = tex2D( ColorMapSampler, float2(Tex.x+BlurDistance, Tex.y+BlurDistance)) / 4;
  Color += tex2D( ColorMapSampler, float2(Tex.x-BlurDistance, Tex.y-BlurDistance)) / 4;
  Color += tex2D( ColorMapSampler, float2(Tex.x+BlurDistance, Tex.y-BlurDistance)) / 4;
  Color += tex2D( ColorMapSampler, float2(Tex.x-BlurDistance, Tex.y+BlurDistance)) / 4;

  // returned the blurred color
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