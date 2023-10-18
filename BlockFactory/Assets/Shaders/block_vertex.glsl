#version 460 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 color;
layout (location = 2) in vec3 uv;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform vec3 playerPos;

const float planetRadius = 300f;

out vec3 vertColor;
out vec3 vertUv;

vec3 spherify(vec3 mp) {
    mp.xz -= playerPos.xz;
    float dist = length(mp.xz);
    float angle = dist / planetRadius;
    vec2 up = vec2(sin(angle), cos(angle));
    mp.xz /= dist;
    mp.xz *= up.x * (mp.y + planetRadius);
    mp.y = (up.y * (mp.y + planetRadius)) - planetRadius;
    mp.xz += playerPos.xz;
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