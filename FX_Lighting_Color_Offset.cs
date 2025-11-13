using UnityEngine;
using MelonLoader;

namespace WildFire
{
    /// <summary>
    /// Handles color offset calculations for FX_Lighting objects
    /// Converts bright particle fire colors to darker, deeper light colors
    /// 
    /// UPDATED: Now works for any fire color (blue, green, orange, purple, etc.)
    /// Uses color-agnostic darkening that preserves the character of each color
    /// </summary>
    internal static class FireLightColorOffset
    {
        /// <summary>
        /// Apply offset to convert particle color to light color
        /// Darkens and deepens the color for a more natural fire glow
        /// FIXED: Now works for any fire color with adaptive channel reduction
        /// </summary>
        public static Color ApplyLightColorOffset(Color particleColor)
        {
            // Determine dominant color to apply appropriate reduction
            float maxChannel = Mathf.Max(particleColor.r, Mathf.Max(particleColor.g, particleColor.b));

            // Check if this is an orange/red fire (high red, medium green, low blue)
            bool isWarmFire = particleColor.r > 0.7f && particleColor.g > 0.3f && particleColor.b < 0.3f;

            Color lightColor;

            if (isWarmFire)
            {
                // Original formula for orange/red fires - preserves deep warm glow
                float r = particleColor.r * 255f;
                float g = particleColor.g * 255f;
                float b = particleColor.b * 255f;

                float offsetR = r * 0.49f;  // Keep more red for warm fire glow
                float offsetG = g * 0.13f;  // Drastically reduce green
                float offsetB = b * 0.008f; // Nearly eliminate blue

                lightColor = new Color(
                    Mathf.Clamp01(offsetR / 255f),
                    Mathf.Clamp01(offsetG / 255f),
                    Mathf.Clamp01(offsetB / 255f),
                    particleColor.a
                );
            }
            else
            {
                // For cool colors (blue, green, purple) - preserve dominant channel
                // Reduce brightness to 50% but keep color ratios
                float brightnessMultiplier = 0.50f;
                Color darkenedColor = new Color(
                    particleColor.r * brightnessMultiplier,
                    particleColor.g * brightnessMultiplier,
                    particleColor.b * brightnessMultiplier,
                    particleColor.a
                );

                // Increase saturation by reducing the minimum channel more
                float minChannel = Mathf.Min(darkenedColor.r, Mathf.Min(darkenedColor.g, darkenedColor.b));
                float saturationBoost = 0.4f;

                lightColor = new Color(
                    Mathf.Max(0f, darkenedColor.r - minChannel * saturationBoost),
                    Mathf.Max(0f, darkenedColor.g - minChannel * saturationBoost),
                    Mathf.Max(0f, darkenedColor.b - minChannel * saturationBoost),
                    particleColor.a
                );
            }

            return lightColor;
        }

        /// <summary>
        /// Alternative formula: HSV-based approach
        /// Use this if you want more control over hue/saturation/brightness
        /// </summary>
        public static Color ApplyLightColorOffsetHSV(Color particleColor)
        {
            // Convert to HSV for better color manipulation
            Color.RGBToHSV(particleColor, out float h, out float s, out float v);

            // Reduce brightness (value) to about 50% for darker light
            float darkV = v * 0.50f;

            // Slightly increase saturation for deeper colors (but cap it)
            float deepS = Mathf.Min(1.0f, s * 1.15f);

            // Optional: Add slight warmth by shifting hue slightly toward red/orange
            // Only do this for non-warm colors (blue, green, purple)
            float adjustedH = h;
            if (h > 0.15f && h < 0.95f) // Not already red/orange
            {
                // Shift hue slightly toward warm (reduce hue value slightly)
                // This gives all fires a subtle warm glow
                adjustedH = h * 0.98f; // Very subtle shift
            }

            // Convert back to RGB
            Color lightColor = Color.HSVToRGB(adjustedH, deepS, darkV);
            lightColor.a = particleColor.a;

            return lightColor;
        }

        /// <summary>
        /// Alternative formula: More aggressive darkening
        /// Use this if you want even deeper, more saturated fire colors in the lights
        /// </summary>
        public static Color ApplyLightColorOffsetAggressive(Color particleColor)
        {
            // Aggressive brightness reduction
            float brightnessMultiplier = 0.35f;
            Color darkenedColor = new Color(
                particleColor.r * brightnessMultiplier,
                particleColor.g * brightnessMultiplier,
                particleColor.b * brightnessMultiplier,
                particleColor.a
            );

            // More aggressive saturation boost
            float minChannel = Mathf.Min(darkenedColor.r, Mathf.Min(darkenedColor.g, darkenedColor.b));
            float saturationBoost = 0.5f;

            Color lightColor = new Color(
                Mathf.Max(0f, darkenedColor.r - minChannel * saturationBoost),
                Mathf.Max(0f, darkenedColor.g - minChannel * saturationBoost),
                Mathf.Max(0f, darkenedColor.b - minChannel * saturationBoost),
                particleColor.a
            );

            // Stronger warm bias
            float warmBias = 0.04f;
            lightColor.r = Mathf.Min(1.0f, lightColor.r + warmBias);

            return lightColor;
        }

        /// <summary>
        /// Alternative formula: Conservative darkening (less aggressive)
        /// Use this if you want lighter fire glow effects
        /// </summary>
        public static Color ApplyLightColorOffsetConservative(Color particleColor)
        {
            // Conservative brightness reduction
            float brightnessMultiplier = 0.65f;
            Color darkenedColor = new Color(
                particleColor.r * brightnessMultiplier,
                particleColor.g * brightnessMultiplier,
                particleColor.b * brightnessMultiplier,
                particleColor.a
            );

            // Lighter saturation boost
            float minChannel = Mathf.Min(darkenedColor.r, Mathf.Min(darkenedColor.g, darkenedColor.b));
            float saturationBoost = 0.15f;

            Color lightColor = new Color(
                Mathf.Max(0f, darkenedColor.r - minChannel * saturationBoost),
                Mathf.Max(0f, darkenedColor.g - minChannel * saturationBoost),
                Mathf.Max(0f, darkenedColor.b - minChannel * saturationBoost),
                particleColor.a
            );

            // Minimal warm bias
            float warmBias = 0.01f;
            lightColor.r = Mathf.Min(1.0f, lightColor.r + warmBias);

            return lightColor;
        }

        /// <summary>
        /// Debug method to log color offset calculations
        /// Now shows before/after for any color
        /// </summary>
        public static void DebugColorOffset(Color particleColor, string context = "")
        {
            Color offsetColor = ApplyLightColorOffset(particleColor);

            //MelonLogger.Msg($"[Fire Light Color Offset{(string.IsNullOrEmpty(context) ? "" : $" - {context}")}]");
            //MelonLogger.Msg($"Particle Color (0-1): R={particleColor.r:F4} G={particleColor.g:F4} B={particleColor.b:F4}");
            //MelonLogger.Msg($"Particle Color (0-255): R={particleColor.r * 255f:F0} G={particleColor.g * 255f:F0} B={particleColor.b * 255f:F0}");
            //MelonLogger.Msg($"Light Color (0-1): R={offsetColor.r:F4} G={offsetColor.g:F4} B={offsetColor.b:F4}");
            //MelonLogger.Msg($"Light Color (0-255): R={offsetColor.r * 255f:F0} G={offsetColor.g * 255f:F0} B={offsetColor.b * 255f:F0}");

            // Show brightness reduction
            float particleBrightness = (particleColor.r + particleColor.g + particleColor.b) / 3f;
            float lightBrightness = (offsetColor.r + offsetColor.g + offsetColor.b) / 3f;
            float reductionPercent = (1f - (lightBrightness / Mathf.Max(0.001f, particleBrightness))) * 100f;
            //MelonLogger.Msg($"Brightness Reduction: {reductionPercent:F1}%");
        }
    }
}