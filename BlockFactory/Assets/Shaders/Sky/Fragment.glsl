#version 430 core

#define M_PI 3.1415926535897932384626433832795

out vec4 FragColor;

uniform vec3 sunDirection;
uniform float dayCoef;

in vec3 vertexPos;

float softsign(float x) {
    return atan(x) / (M_PI / 2);
}

void main()
{
    vec3 normVertexPos = normalize(vertexPos);
    float upSgn = softsign((normVertexPos.y + 0.1) * 10);
    vec3 dayCol = mix(vec3(0.9, 0.9, 1), vec3(0.7, 0.7, 1), (upSgn + 1) / 2);
    //float dayCoef = (softsign((sunDirection.y + 0.2) * 5) + 1) / 2;
    vec3 baseCol = dayCol * dayCoef;
    vec2 horizontalSunDir = normalize(sunDirection.xz);
    float sunRiseDirY = 0.8;
    float sunRiseDirHorizontal = sqrt(1 - sunRiseDirY * sunRiseDirY);
    vec2 scHorSunDir = horizontalSunDir * sunRiseDirHorizontal;
    vec3 upSunDir = normalize(vec3(scHorSunDir.x, sunRiseDirY, scHorSunDir.y));
    vec3 downSunDir = normalize(vec3(scHorSunDir.x, -sunRiseDirY, scHorSunDir.y));
    float upSunSgn = softsign((dot(normVertexPos, upSunDir) - 0.1) * 5);
    float downSunSgn = softsign((dot(normVertexPos, downSunDir) - 0.1) * 5);
    float sunRiseDirCoef = ((upSunSgn + 1) * (downSunSgn + 1)) / 4;
    float sunRisePresenceCoef = 1 - sqrt(abs(sunDirection.y));
    float sunRiseCoef = sunRiseDirCoef * sunRisePresenceCoef;
    vec3 col = mix(baseCol, vec3(1, 0.6, 0), sunRiseCoef);
    FragColor = vec4(col, 1);
} 