#version 430 core
out vec4 FragColor;

uniform sampler2D tex;

struct SpriteBox {
    vec2 min;
    vec2 max;
};

layout(binding=0, std140) readonly buffer spriteBoxes {
    SpriteBox boxes[];
};

uniform float loadProgress;
uniform sampler2D skyTex;
  
in vec4 vertexColor; // the input variable from the vertex shader (same name and same type)  
in vec2 vertexUv;
in vec4 vertexPosition;
flat in int vertexSprite;

void main()
{
    vec2 uv = min(max(vertexUv, boxes[vertexSprite].min), boxes[vertexSprite].max);
    vec4 meshColor = vertexColor * texture(tex, uv);
    vec2 skyCoords = ((vertexPosition.xy / vertexPosition.w) + vec2(1)) / 2;
    vec3 skyColor = texture(skyTex, skyCoords).rgb;
    FragColor = vec4(mix(skyColor.rgb, meshColor.rgb, loadProgress), meshColor.a);
    if(FragColor.a < 0.1f) discard;
} 