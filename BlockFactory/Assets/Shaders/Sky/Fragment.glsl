#version 430 core

out vec4 FragColor;

in vec3 vertexPos;

void main()
{
    vec3 normVertexPos = normalize(vertexPos);
    vec3 col = (normVertexPos + vec3(1)) / 2;
    FragColor = vec4(col, 1);
} 