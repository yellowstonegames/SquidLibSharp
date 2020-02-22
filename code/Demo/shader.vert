#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 3) in float aColor;
varying float vColor;
void main()
{
    gl_Position = vec4(aPos, 1.0);
    vColor = aColor;
}