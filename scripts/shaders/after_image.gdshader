shader_type canvas_item;

uniform vec4 color: source_color;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec2 scaledUVs = UV * 100.0f;
	int scaledUVYInt = int(scaledUVs.y);

	if (scaledUVYInt % 2 == 0)
	{
		COLOR = vec4(0.0, 0.0, 0.0, 0.0);
	}
	else
	{
		COLOR = COLOR.a > 0.0f ? color : COLOR;
	}
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
