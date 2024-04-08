float GetRoundedBoxDistance(float2 pos, float2 center, float radius, float inset)
{
	// distance from center
    pos = abs(pos - center);

    // distance from the inner corner
    pos = pos - (center - float2(radius + inset, radius + inset));

    // use distance to nearest edge when not in quadrant with radius
    // this handles an edge case when radius is very close to thickness
    // otherwise we're in the quadrant with the radius, 
    // just use the analytic signed distance function
    return lerp(length(pos) - radius, max(pos.x - radius, pos.y - radius), float(pos.x <= 0 || pos.y <= 0));
}

float4 GetRoundedBoxElementColor(float2 size, float2 texcoord, float4 cornerRadii, half thickness, half4 color, half4 borderColor)
{
    float2 pos = size * texcoord;
    float2 center = size / 2.0;

	// figure out which radius to use based on which quadrant we're in
    float2 quadrant = step(texcoord, float2(.5, .5));

    float left = lerp(cornerRadii.y, cornerRadii.x, quadrant.x);
    float right = lerp(cornerRadii.z, cornerRadii.w, quadrant.x);
    float radius = lerp(right, left, quadrant.y);

	// Compute the distances internal and external to the border outline
    float dext = GetRoundedBoxDistance(pos, center, radius, 0.0);
    float din = GetRoundedBoxDistance(pos, center, max(radius - thickness, 0), thickness);

	// Compute the border intensity and fill intensity with a smooth transition
    float spread = 0.5;
    float bi = smoothstep(spread, -spread, dext);
    float fi = smoothstep(spread, -spread, din);

    float4 OutColor = lerp(borderColor, color, float(thickness > radius));
    OutColor.a = 0.0;

	// blend in the border and fill colors
    OutColor = lerp(OutColor, borderColor, bi);
    OutColor = lerp(OutColor, color, fi);
    return OutColor;
}