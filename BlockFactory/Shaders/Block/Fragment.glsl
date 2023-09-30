#version 330 core
out vec4 FragColor;

uniform sampler2D tex;
  
in vec4 vertexColor; // the input variable from the vertex shader (same name and same type)  
in vec2 vertexUv;

void main()
{
    FragColor = vertexColor * texture(tex, vertexUv);
} 