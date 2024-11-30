shader_type canvas_item;
uniform sampler2D sampler;
uniform vec4 mask_color : source_color;
void fragment() {
	float a = texture(sampler, UV).x;
	COLOR = vec4(mask_color.xyz, mask_color.w * a);
}
