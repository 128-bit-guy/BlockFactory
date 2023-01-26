#version 460 core

in vec3 vertColor;
in vec2 vertUv;

uniform sampler2D tex;

out vec4 fragColor;

void main() {
	vec4 t = texture(tex, vertUv);
	if(t.r < 0.5f) {
		discard;
	}
	fragColor = vec4(vertColor, 1.0f) *  vec4(1.0f, 1.0f, 1.0f, t.r);
}