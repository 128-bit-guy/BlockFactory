#version 460 core

in vec3 vertColor;
in vec3 vertUv;

uniform sampler2DArray tex;

out vec4 fragColor;

void main() {
	vec4 col = vec4(vertColor, 1.0f) *  texture(tex, vertUv);
	if(col.a <= 0.5f) {
		discard;
	}
	fragColor = col;
}