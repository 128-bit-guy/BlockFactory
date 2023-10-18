#version 460 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 color;
layout (location = 2) in vec3 uv;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform vec3 playerPos;

const float planetRadius = 150f;

out vec3 vertColor;
out vec3 vertUv;

vec3 spherify(vec3 mp) {
    mp -= playerPos;
    float dist = length(mp.xz);
    float delta = planetRadius - sqrt(planetRadius * planetRadius - dist * dist);
    mp.y -= delta;
    mp += playerPos;
    return mp;
}

void main()
{
    vec3 mp = (model * vec4(position, 1.0f)).xyz;
    mp = spherify(mp);
    gl_Position = projection * view * vec4(mp, 1.0);
    vertUv = uv;
    vertColor = color;
}