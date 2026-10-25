shader_type spatial;
render_mode blend_mix, depth_draw_never, unshaded, cull_disabled;

uniform vec4 trail_color : source_color = vec4(1.0, 0.0, 0.0, 1.0);

void fragment() {
    float alpha = UV.y * UV.y;
    
    ALBEDO = trail_color.rgb;
    ALPHA = alpha * trail_color.a;
    EMISSION = trail_color.rgb * alpha;
}