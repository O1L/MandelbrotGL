#version 330 core

uniform float max_iterations;
uniform float zoom;
uniform float offset_x;
uniform float offset_y;

out vec4 pixel_color;
in  vec2 coord;

void main()
{
    float real  = coord.x * zoom - offset_x;
    float imag  = coord.y * zoom - offset_y;
    float creal = real;
    float cimag = imag;

    float magnitude = 0.0;
    float iterations;
    vec3 color;

    for (iterations = 0.0; iterations < max_iterations; iterations++)
    {
        float temp_real = real;

        real = (temp_real * temp_real) - (imag * imag) + creal;
        imag = 2.0 * temp_real * imag + cimag;
        magnitude = (real * real) + (imag * imag);

        if (magnitude >= 4.0)
            break;
    }

    // calculate pixel color
    if (magnitude < 4.0)
    {
        color = vec3(0.0, 0.0, 0.0);
    }
    else
    {
        float interpolation = fract (iterations * 0.05);
        vec3 color1 = vec3(0.5, 0.0, 1.5);
        vec3 color2 = vec3(0.0, 1.5, 0.0);
        color = mix (color1, color2, interpolation);
    }
    
    pixel_color = vec4(color, 1.0);
}
