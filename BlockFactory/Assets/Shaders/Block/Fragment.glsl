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
uniform vec4 skyColor;
  
in vec4 vertexColor; // the input variable from the vertex shader (same name and same type)  
in vec2 vertexUv;
flat in int vertexSprite;

void main()
{
    vec2 uv = min(max(vertexUv, boxes[vertexSprite].min), boxes[vertexSprite].max);
    FragColor = mix(skyColor, vertexColor * texture(tex, uv), loadProgress);
    if(FragColor.a < 0.1f) discard;
} 