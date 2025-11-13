using UnityEngine;
using System.Collections.Generic;

namespace WildFire
{
    /// <summary>
    /// Data for vanilla game items 
    /// IMPORTANT: This file handles TWO types of vanilla items:
    /// 1. Items that need to be made burnable (defined in BurnableSettings)
    /// 2. Items that are already burnable but need custom colors/effects (only in NamePatterns, DisplayNames, SparkSettings)
    /// 
    /// When adding new vanilla items that already have burnable behavior:
    /// - Add them to NamePatterns and DisplayNames 
    /// - Add spark/color settings if desired
    /// - DO NOT add them to BurnableSettings (they already burn)
    /// </summary>
    internal static class VanillaFuelData
    {
        // All vanilla fuel types that this file handles  
        public static readonly CustomFuelType[] AllVanillaFuelTypes = new CustomFuelType[]
        {
            CustomFuelType.FlareA,
            CustomFuelType.BlueFlare,
            CustomFuelType.DustingSulfur,
            CustomFuelType.StumpRemover,
            CustomFuelType.GunpowderCan,
            CustomFuelType.FlareGunAmmoSingle
        };

        // Gear name patterns for vanilla items
        // NOTE: This includes ALL vanilla items we want to track, regardless of whether they need burnable behavior added
        public static readonly Dictionary<string, CustomFuelType> NamePatterns = new Dictionary<string, CustomFuelType>
        {
            { "flarea", CustomFuelType.FlareA },
            { "gear_flarea", CustomFuelType.FlareA },
            { "blueflare", CustomFuelType.BlueFlare },
            { "gear_blueflare", CustomFuelType.BlueFlare },
            { "dustingsulfur", CustomFuelType.DustingSulfur },
            { "gear_dustingsulfur", CustomFuelType.DustingSulfur },
            { "stumpremover", CustomFuelType.StumpRemover },
            { "gear_stumpremover", CustomFuelType.StumpRemover },
            { "gunpowdercan", CustomFuelType.GunpowderCan },
            { "gear_gunpowdercan", CustomFuelType.GunpowderCan },
            { "flaregunammosingle", CustomFuelType.FlareGunAmmoSingle },
            { "gear_flaregunammosingle", CustomFuelType.FlareGunAmmoSingle }
        };

        // Burnable settings for vanilla items that DON'T already have burnable behavior
        // IMPORTANT: Only add items here if they need to be made burnable
        // Items already burnable in the base game should NOT be added here
        public static readonly Dictionary<CustomFuelType, GearBurnableSettings> BurnableSettings = new Dictionary<CustomFuelType, GearBurnableSettings>
        {
            [CustomFuelType.FlareA] = new GearBurnableSettings
            {
                burnDurationHours = .25f,
                fireAgeMinutesBeforeAdding = 0f,
                fireStartSkillModifier = 0,
                heatIncrease = 5f,
                heatInnerRadius = 2.5f,
                heatOuterRadius = 6f,
                fireStartDurationModifier = 0f,
                isWet = false,
                isTinder = false,
                isBurntInFireTracked = false
            },
            [CustomFuelType.BlueFlare] = new GearBurnableSettings
            {
                burnDurationHours = .25f,
                fireAgeMinutesBeforeAdding = 0f,
                fireStartSkillModifier = 0,
                heatIncrease = 5f,
                heatInnerRadius = 2.5f,
                heatOuterRadius = 6f,
                fireStartDurationModifier = 0f,
                isWet = false,
                isTinder = false,
                isBurntInFireTracked = false
            },
            [CustomFuelType.DustingSulfur] = new GearBurnableSettings
            {
                burnDurationHours = .5f,
                fireAgeMinutesBeforeAdding = 0f,
                fireStartSkillModifier = 0,
                heatIncrease = 5f,
                heatInnerRadius = 2.5f,
                heatOuterRadius = 6f,
                fireStartDurationModifier = 0f,
                isWet = false,
                isTinder = false,
                isBurntInFireTracked = false
            },
            [CustomFuelType.StumpRemover] = new GearBurnableSettings
            {
                burnDurationHours = .5f,
                fireAgeMinutesBeforeAdding = 0f,
                fireStartSkillModifier = 0,
                heatIncrease = 5f,
                heatInnerRadius = 2.5f,
                heatOuterRadius = 6f,
                fireStartDurationModifier = 0f,
                isWet = false,
                isTinder = false,
                isBurntInFireTracked = false
            },
            [CustomFuelType.GunpowderCan] = new GearBurnableSettings
            {
                burnDurationHours = .1f,
                fireAgeMinutesBeforeAdding = 0f,
                fireStartSkillModifier = 0,
                heatIncrease = 5f,
                heatInnerRadius = 2.5f,
                heatOuterRadius = 6f,
                fireStartDurationModifier = 0f,
                isWet = false,
                isTinder = false,
                isBurntInFireTracked = false
            },
            [CustomFuelType.FlareGunAmmoSingle] = new GearBurnableSettings
            {
                burnDurationHours = .1f,
                fireAgeMinutesBeforeAdding = 0f,
                fireStartSkillModifier = 0,
                heatIncrease = 5f,
                heatInnerRadius = 2.5f,
                heatOuterRadius = 6f,
                fireStartDurationModifier = 0f,
                isWet = false,
                isTinder = false,
                isBurntInFireTracked = false
            }
        };

        // Fire colors for vanilla fuels (hardcoded values)
        // These colors are used when enableFuelColors is false
        public static readonly Dictionary<CustomFuelType, Color> FireColors = new Dictionary<CustomFuelType, Color>
        {
            [CustomFuelType.FlareA] = new Color(255f / 255f, 34f / 255f, 13f / 255f, 1f), // Red
            [CustomFuelType.BlueFlare] = new Color(10f / 255f, 11f / 255f, 255f / 255f, 1f), // Blue
            [CustomFuelType.DustingSulfur] = new Color(0f / 255f, 255f / 255f, 100f / 255f, 1f), // Green
            [CustomFuelType.StumpRemover] = new Color(237f / 255f, 236f / 255f, 237f / 255f, 1f), // Light purple/white
            [CustomFuelType.GunpowderCan] = new Color(255f / 255f, 255f / 255f, 255f / 255f, 1f), // White
            [CustomFuelType.FlareGunAmmoSingle] = new Color(255f / 255f, 34f / 255f, 13f / 255f, 1f), // Red
        };

        // Smoke settings for vanilla items - based on real chemical properties
        public static readonly Dictionary<CustomFuelType, GearSmokeSettings> SmokeSettings = new Dictionary<CustomFuelType, GearSmokeSettings>
        {
            [CustomFuelType.FlareA] = new GearSmokeSettings
            {
                densityMultiplier = 4f,    // Flares produce moderate smoke
                lifetimeMultiplier = 2f,   // Longer smoke lifetime
                sizeMultiplier = 5f,       // Slightly larger smoke particles
                speedMultiplier = 3f,      // Smoke rises at normal pace
                opacityMultiplier = 3f,    // Slightly more opaque smoke
                useCustomSettings = true
            },
            [CustomFuelType.BlueFlare] = new GearSmokeSettings
            {
                densityMultiplier = 4f,    // Blue flares produce more smoke
                lifetimeMultiplier = 2f,   // Smoke lingers longer
                sizeMultiplier = 5f,       // Larger smoke particles
                speedMultiplier = 3f,     // Smoke rises slower (heavier particles)
                opacityMultiplier = 3f,    // Denser  smoke
                useCustomSettings = true
            },
            [CustomFuelType.DustingSulfur] = new GearSmokeSettings
            {
                densityMultiplier = 3.5f,    // Sulfur produces heavy smoke
                lifetimeMultiplier = 2f,   // Smoke persists longer
                sizeMultiplier = 3f,       // Large smoke clouds
                speedMultiplier = 0.7f,      // Heavy smoke rises slowly
                opacityMultiplier = 3f,    // Very dense smoke
                useCustomSettings = true
            },
            [CustomFuelType.StumpRemover] = new GearSmokeSettings
            {
                densityMultiplier = 3.5f,    // Stump remover produces the most smoke
                lifetimeMultiplier = 2f,   // Smoke lingers for extended time
                sizeMultiplier = 3f,       // Very large smoke particles
                speedMultiplier = 2f,     // Heavy smoke rises slowly
                opacityMultiplier = 2f,    // Very thick smoke
                useCustomSettings = true
            },
            [CustomFuelType.GunpowderCan] = new GearSmokeSettings
            {
                densityMultiplier = 2f,    // Flares produce moderate smoke
                lifetimeMultiplier = 2f,   // Standard smoke lifetime
                sizeMultiplier = 2f,       // Slightly larger smoke particles
                speedMultiplier = 2f,      // Smoke rises at normal pace
                opacityMultiplier = 2f,    // Slightly more opaque smoke
                useCustomSettings = true
            },
            [CustomFuelType.FlareGunAmmoSingle] = new GearSmokeSettings
            {
                densityMultiplier = 1.6f,    // Flares produce moderate smoke
                lifetimeMultiplier = 1.6f,   // Standard smoke lifetime
                sizeMultiplier = 1.7f,       // Slightly larger smoke particles
                speedMultiplier = 2f,      // Smoke rises at normal pace
                opacityMultiplier = 1.7f,    // Slightly more opaque smoke
                useCustomSettings = true
            }
        };

        // Spark settings for vanilla items (both items that need burnable behavior added AND items that are already burnable)
        public static readonly Dictionary<CustomFuelType, GearSparkSettings> SparkSettings = new Dictionary<CustomFuelType, GearSparkSettings>
        {
            [CustomFuelType.FlareA] = new GearSparkSettings
            {
                emissionMultiplier = 2f,
                lifetimeMultiplier = 2f,
                sizeMultiplier = 1f,
                speedMultiplier = 2f,
                sparkColor = new Color(1.0f, 0.0f, 0.0f, 1.0f), // Red sparks for red flare
                durationMultiplier = 1.0f,
                useCustomSettings = true
            },
            [CustomFuelType.BlueFlare] = new GearSparkSettings
            {
                emissionMultiplier = 2.0f,
                lifetimeMultiplier = 2.0f,
                sizeMultiplier = 1.5f,
                speedMultiplier = 2f,
                sparkColor = new Color(0.0f, 0.0f, 1.0f, 1.0f), // Blue sparks for blue flare
                durationMultiplier = 1.0f,
                useCustomSettings = true
            },
            [CustomFuelType.DustingSulfur] = new GearSparkSettings
            {
                emissionMultiplier = 2.0f,
                lifetimeMultiplier = 2.0f,
                sizeMultiplier = 1.0f,
                speedMultiplier = 1.5f,
                sparkColor = new Color(0.2f, 1.0f, 0.0f, 1.0f), // Green sparks for dusting sulfur
                durationMultiplier = 2.0f,
                useCustomSettings = true
            },
            [CustomFuelType.StumpRemover] = new GearSparkSettings
            {
                emissionMultiplier = 5.0f,
                lifetimeMultiplier = 3.0f,
                sizeMultiplier = 1.5f,
                speedMultiplier = 2.0f,
                sparkColor = new Color(1.0f, 0.647f, 0.0f, 1.0f), // Orange sparks for stump remover
                durationMultiplier = 2.0f,
                useCustomSettings = true
            },
            [CustomFuelType.GunpowderCan] = new GearSparkSettings
            {
                emissionMultiplier = 5.0f, // Very high emission (iron filings create lots of sparks)
                lifetimeMultiplier = 2.0f,
                sizeMultiplier = 0.3f, // Smaller but numerous sparks
                speedMultiplier = 3.0f, // Fast, shooting sparks
                sparkColor = new Color(1.0f, 0.8f, 0.3f, 1.0f), // Golden orange (typical iron spark color)
                durationMultiplier = 2.0f,
                useCustomSettings = true
            },
            [CustomFuelType.FlareGunAmmoSingle] = new GearSparkSettings
            {
                emissionMultiplier = 1.5f,
                lifetimeMultiplier = 1.5f,
                sizeMultiplier = 1.3f,
                speedMultiplier = 1.0f,
                sparkColor = new Color(1.0f, 0.2f, 0.0f, 1.0f), // Red sparks for red flare
                durationMultiplier = 1.2f,
                useCustomSettings = true
            }
        };

        // Display names for vanilla items (both items that need burnable behavior added AND items that are already burnable)
        public static readonly Dictionary<CustomFuelType, string> DisplayNames = new Dictionary<CustomFuelType, string>
        {
            [CustomFuelType.FlareA] = "Red Flare",
            [CustomFuelType.BlueFlare] = "Marine Flare (Blue)",
            [CustomFuelType.DustingSulfur] = "Dusting Sulfur (Green)",
            [CustomFuelType.StumpRemover] = "Stump Remover (Orange)",
            [CustomFuelType.GunpowderCan] = "Gunpowder Can (White)",
            [CustomFuelType.FlareGunAmmoSingle] = "Flare Gun Ammo (Red)"
        };
    }
}