#version 330 core
layout (location = 0) in vec3 aPos; // the position variable has attribute position 0
layout (location = 1) in vec4 color;
layout (location = 2) in vec2 uv;
layout (location = 3) in int sprite;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform vec3 playerPos;

out vec4 vertexColor; // specify a color output to the fragment shader
out vec2 vertexUv;
out vec4 vertexPosition;
flat out int vertexSprite;

vec3 spherify(vec3 mp) {
    return mp;
}

void main()
{
    vec4 pos = projection * view * model * vec4(aPos, 1.0);
    vertexPosition = pos; // see how we directly give a vec3 to vec4's constructor
    gl_Position = pos;
    vertexColor = color;
    vertexUv = uv;
    vertexSprite = sprite;
}