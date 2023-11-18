#version 330 core
out vec4 FragColor;

uniform sampler2D tex;

uniform float loadProgress;
uniform vec4 skyColor;
  
in vec4 vertexColor; // the input variable from the vertex shader (same name and same type)  
in vec2 vertexUv;

void main()
{

    FragColor = mix(skyColor, vertexColor * texture(tex, vertexUv), loadProgress);
    if(FragColor.a < 0.1f) discard;
} 