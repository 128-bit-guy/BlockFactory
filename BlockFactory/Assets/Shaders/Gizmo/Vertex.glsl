#version 460 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec4 color;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec4 vertColor;

void main()
{
    vec4 pos4 = projection * view * model * vec4(position.x, position.y, position.z, 1.0);
    vec3 pos = pos4.xyz / pos4.w;
    pos.z -= 0.0001f;
    gl_Position = vec4(pos, 1.0f);
    vertColor = color;
}