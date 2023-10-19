#version 330 core
layout (location = 0) in vec3 aPos; // the position variable has attribute position 0
layout (location = 1) in vec4 color;
layout (location = 2) in vec2 uv;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform vec3 playerPos;

const float planetRadius = 300f;

out vec4 vertexColor; // specify a color output to the fragment shader
out vec2 vertexUv;

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
    vec3 mp = (model * vec4(aPos, 1.0)).xyz;
    mp = spherify(mp);
    gl_Position = projection * view * vec4(mp, 1.0); // see how we directly give a vec3 to vec4's constructor
    vertexColor = color;
    vertexUv = uv;
}