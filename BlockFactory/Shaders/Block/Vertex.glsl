#version 330 core
layout (location = 0) in vec3 aPos; // the position variable has attribute position 0
layout (location = 1) in vec4 color;
layout (location = 2) in vec2 uv;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
  
out vec4 vertexColor; // specify a color output to the fragment shader
out vec2 vertexUv;

void main()
{
    gl_Position = projection * view * model * vec4(aPos, 1.0); // see how we directly give a vec3 to vec4's constructor
    vertexColor = color;
    vertexUv = uv;
}