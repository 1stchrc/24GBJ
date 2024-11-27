shader_type canvas_item;
uniform sampler2D subv;
uniform vec4 modu;
void fragment() {
	vec4 sv_color = texture(subv, UV);
	float light = max(max(sv_color.x, sv_color.y), sv_color.z);
	COLOR = vec4(light * modu.x, light * modu.y, light * modu.z, sv_color.w * modu.w);
}