using UnityEngine;
using System.Collections.Generic;

namespace WildFire
{
    /// <summary>
    /// Data for custom fuel items that are already burnable in the base game
    /// These items have their own unique fire colors and spark effects
    /// </summary>
    internal static class CustomFuelData
    {
        // Hardcoded constants
        public const float TRANSITION_SPEED = 1.0f;

        // All custom fuel types that this file handles
        public static readonly CustomFuelType[] AllCustomFuelTypes = new CustomFuelType[]
        {
            CustomFuelType.BariumCarbonate,
            CustomFuelType.MagnesiumPowder,
            CustomFuelType.IronFilings,
            CustomFuelType.Rubidium,
            CustomFuelType.CalciumChloride,
            CustomFuelType.CopperCarbonate
        };

        // Gear name patterns for custom items
        public static readonly Dictionary<string, CustomFuelType> NamePatterns = new Dictionary<string, CustomFuelType>
        {
            { "bariumcarbonate", CustomFuelType.BariumCarbonate },
            { "gear_bariumcarbonate", CustomFuelType.BariumCarbonate },
            { "magnesiumpowder", CustomFuelType.MagnesiumPowder },
            { "gear_magnesiumpowder", CustomFuelType.MagnesiumPowder },
            { "ironfilings", CustomFuelType.IronFilings },
            { "gear_ironfilings", CustomFuelType.IronFilings },
            { "rubidium", CustomFuelType.Rubidium },
            { "gear_rubidium", CustomFuelType.Rubidium },
            { "calciumchloride", CustomFuelType.CalciumChloride },
            { "gear_calciumchloride", CustomFuelType.CalciumChloride },
            { "coppercarbonate", CustomFuelType.CopperCarbonate },
            { "gear_coppercarbonate", CustomFuelType.CopperCarbonate }
        };

        // Fire colors for custom fuels (hardcoded values)
        public static readonly Dictionary<CustomFuelType, Color> FireColors = new Dictionary<CustomFuelType, Color>
        {
            [CustomFuelType.BariumCarbonate] = new Color(0f / 255f, 255f / 255f, 64f / 255f, 1f), // Green
            [CustomFuelType.MagnesiumPowder] = new Color(255f / 255f, 255f / 255f, 255f / 255f, 1f), // White
            [CustomFuelType.IronFilings] = new Color(255f / 255f, 215f / 255f, 0f / 255f, 1f), // Gold/Yellow
            [CustomFuelType.Rubidium] = new Color(235f / 255f, 40f / 255f, 255f / 255f, 1f), // Purple
            [CustomFuelType.CalciumChloride] = new Color(255f / 255f, 80f / 255f, 0f / 255f, 1f), // Red/Orange
            [CustomFuelType.CopperCarbonate] = new Color(0f / 255f, 140f / 255f, 255f / 255f, 1f) // Light Blue
        };

        // Smoke settings for custom items - based on real chemical properties
        public static readonly Dictionary<CustomFuelType, GearSmokeSettings> SmokeSettings = new Dictionary<CustomFuelType, GearSmokeSettings>
        {
            [CustomFuelType.BariumCarbonate] = new GearSmokeSettings
            {
                densityMultiplier = 0.5f,    // Barium produces less smoke, burns clean
                lifetimeMultiplier = 0.5f,  // Smoke dissipates quickly
                sizeMultiplier = 0.5f,      // Smaller smoke particles
                speedMultiplier = 2f,     // Smoke rises faster (lighter)
                opacityMultiplier = 0.5f,   // Less opaque smoke
                useCustomSettings = true
            },
            [CustomFuelType.MagnesiumPowder] = new GearSmokeSettings
            {
                densityMultiplier = 0.5f,   // Magnesium burns very cleanly with minimal smoke
                lifetimeMultiplier = 0.6f, // Smoke dissipates very quickly
                sizeMultiplier = 0.3f,     // Very small smoke particles (bright white flame dominates)
                speedMultiplier = 5f,    // Smoke rises very fast (intense heat)
                opacityMultiplier = 0.5f,  // Very light smoke
                useCustomSettings = true
            },
            [CustomFuelType.IronFilings] = new GearSmokeSettings
            {
                densityMultiplier = 3f,   // Iron filings produce heavy smoke/ash
                lifetimeMultiplier = 3f, // Smoke lingers longer
                sizeMultiplier = 3f,     // Large smoke/ash particles
                speedMultiplier = 1f,   // Normal rise speed
                opacityMultiplier = 3f,  // Moderately dense smoke
                useCustomSettings = true
            },
            [CustomFuelType.Rubidium] = new GearSmokeSettings
            {
                densityMultiplier = 3f,   // Rubidium produces moderate smoke
                lifetimeMultiplier = 4f, // Smoke lasts a reasonable time
                sizeMultiplier = 1.0f,     // Standard smoke particle size
                speedMultiplier = 1.0f,    // Normal rise speed
                opacityMultiplier = 2f,  // Slightly more opaque
                useCustomSettings = true
            },
            [CustomFuelType.CalciumChloride] = new GearSmokeSettings
            {
                densityMultiplier = 2f,   // Calcium chloride produces good smoke
                lifetimeMultiplier = 2f, // Smoke lingers
                sizeMultiplier = 2f,     // Larger smoke particles
                speedMultiplier = 0.6f,    // Smoke rises slightly slower
                opacityMultiplier = 5f, // Dense smoke
                useCustomSettings = true
            },
            [CustomFuelType.CopperCarbonate] = new GearSmokeSettings
            {
                densityMultiplier = 3f,   // Copper produces good smoke
                lifetimeMultiplier = 3f, // Smoke persists well
                sizeMultiplier = 3f,     // Larger smoke particles
                speedMultiplier = 0.6f,   // Smoke rises slower (heavier particles)
                opacityMultiplier = 3f,  // Fairly dense smoke
                useCustomSettings = true
            }
        };

        // Spark settings for custom items - based on real chemical properties
        public static readonly Dictionary<CustomFuelType, GearSparkSettings> SparkSettings = new Dictionary<CustomFuelType, GearSparkSettings>
        {
            [CustomFuelType.BariumCarbonate] = new GearSparkSettings
            {
                emissionMultiplier = 2.5f,
                lifetimeMultiplier = 1.2f,
                sizeMultiplier = 0.7f,
                speedMultiplier = 0.8f,
                sparkColor = new Color(0.0f, 1.0f, 0.0f, 1.0f), // Bright green (barium burns green)
                durationMultiplier = 1.3f,
                useCustomSettings = true
            },
            [CustomFuelType.CalciumChloride] = new GearSparkSettings
            {
                emissionMultiplier = 2f,
                lifetimeMultiplier = 1.2f,
                sizeMultiplier = 2.0f,
                speedMultiplier = 1.0f,
                sparkColor = new Color(1.0f, 0.4f, 0.0f, 1.0f), // Red-orange (calcium flame color)
                durationMultiplier = 1.5f,
                useCustomSettings = true
            },
            [CustomFuelType.CopperCarbonate] = new GearSparkSettings
            {
                emissionMultiplier = 2f,
                lifetimeMultiplier = 1.3f,
                sizeMultiplier = 1f,
                speedMultiplier = 1.0f,
                sparkColor = new Color(0.0f, 0.5f, 1.0f, 1.0f), // Blue-green/turquoise (copper flame color)
                durationMultiplier = 1.5f,
                useCustomSettings = true
            },
            [CustomFuelType.IronFilings] = new GearSparkSettings
            {
                emissionMultiplier = 5.0f, // Very high emission (iron filings create lots of sparks)
                lifetimeMultiplier = 1.7f,
                sizeMultiplier = 0.5f, // Smaller but numerous sparks
                speedMultiplier = 3.0f, // Fast, shooting sparks
                sparkColor = new Color(1.0f, 0.8f, 0.3f, 1.0f), // Golden orange (typical iron spark color)
                durationMultiplier = 2.0f,
                useCustomSettings = true
            },
            [CustomFuelType.MagnesiumPowder] = new GearSparkSettings
            {
                emissionMultiplier = 5.0f,
                lifetimeMultiplier = 0.5f, // Shorter lifetime but intense
                sizeMultiplier = 1.0f,
                speedMultiplier = 4.0f, // Fast, energetic sparks
                sparkColor = new Color(1.0f, 1.0f, 1.0f, 1.0f), // Brilliant white/blue-white
                durationMultiplier = 1f,
                useCustomSettings = true
            },
            [CustomFuelType.Rubidium] = new GearSparkSettings
            {
                emissionMultiplier = 2.5f, // High emission (alkali metal)
                lifetimeMultiplier = 1.3f,
                sizeMultiplier = 1.5f,
                speedMultiplier = 1.1f,
                sparkColor = new Color(1.0f, 0.0f, 1.0f, 1.0f), // Purple/violet (rubidium flame color)
                durationMultiplier = 1f,
                useCustomSettings = true
            }
        };

        // Display names for custom items
        public static readonly Dictionary<CustomFuelType, string> DisplayNames = new Dictionary<CustomFuelType, string>
        {
            [CustomFuelType.BariumCarbonate] = "Barium Carbonate (Green)",
            [CustomFuelType.MagnesiumPowder] = "Magnesium Powder (White)",
            [CustomFuelType.IronFilings] = "Iron Filings (Sparks)",
            [CustomFuelType.Rubidium] = "Rubidium (Purple)",
            [CustomFuelType.CalciumChloride] = "Calcium Chloride (Red/Orange)",
            [CustomFuelType.CopperCarbonate] = "Copper Carbonate (Light Blue)"
        };
    }
}