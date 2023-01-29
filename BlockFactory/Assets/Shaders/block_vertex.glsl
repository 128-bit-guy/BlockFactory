#version 460 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 color;
layout (location = 2) in vec3 uv;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 vertColor;
out vec3 vertUv;

void main()
{
    gl_Position = projection * view * model * vec4(position.x, position.y, position.z, 1.0);
    vertUv = uv;
    vertColor = color;
}