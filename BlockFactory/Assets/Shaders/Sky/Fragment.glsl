#version 430 core

#define M_PI 3.1415926535897932384626433832795

out vec4 FragColor;

in vec3 vertexPos;

float softsign(float x) {
    return atan(x) / M_PI;
}

void main()
{
    vec3 normVertexPos = normalize(vertexPos);
    float sgn = softsign((normVertexPos.y + 0.1) * 10);
    vec3 col = mix(vec3(0.9, 0.9, 1), vec3(0.6, 0.6, 1), (sgn + 1) / 2);
    FragColor = vec4(col, 1);
} 