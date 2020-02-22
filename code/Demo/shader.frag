#version 330 core
varying float vColor;
out vec4 FragColor;

void main()
{
    FragColor = vec4(vColor, vColor, vColor, 1.0);
}