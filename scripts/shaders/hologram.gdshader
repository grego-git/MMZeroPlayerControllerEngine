shader_type canvas_item;

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
		COLOR = COLOR * 2.0;
	}
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
