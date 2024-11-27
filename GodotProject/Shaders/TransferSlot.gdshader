shader_type canvas_item;
uniform sampler2D subv;

void fragment() {
	COLOR = texture(subv, UV);
}
