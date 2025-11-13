using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace WildFire
{
    public struct GearBurnableSettings
    {
        public float burnDurationHours;
        public float fireAgeMinutesBeforeAdding;
        public int fireStartSkillModifier;
        public float heatIncrease;
        public float heatInnerRadius;
        public float heatOuterRadius;
        public float fireStartDurationModifier;
        public bool isWet;
        public bool isTinder;
        public bool isBurntInFireTracked;
    }

    public struct GearSparkSettings
    {
        public float emissionMultiplier;
        public float lifetimeMultiplier;
        public float sizeMultiplier;
        public float speedMultiplier;
        public Color sparkColor;
        public float durationMultiplier;
        public bool useCustomSettings;
    }

    public struct GearSmokeSettings
    {
        public float densityMultiplier;
        public float lifetimeMultiplier;
        public float sizeMultiplier;
        public float speedMultiplier;
        public float opacityMultiplier;
        public bool useCustomSettings;
    }

    public struct GearColorSettings
    {
        public Color fireColor;
        public bool hasCustomColor;
    }

    public enum CustomFuelType
    {
        None,
        // Vanilla items (existing)
        FlareA,
        BlueFlare,
        DustingSulfur,
        StumpRemover,
        GunpowderCan,
        FlareGunAmmoSingle,
        // Custom items (new) - already have burnable behavior
        BariumCarbonate,
        MagnesiumPowder,
        IronFilings,
        Rubidium,
        CalciumChloride,
        CopperCarbonate,
        Other
    }

    /// <summary>
    /// Centralized data access for all gear item configurations
    /// Now delegates to specialized data classes for better organization
    /// UPDATED: Custom items no longer need GearBurnableSettings since they already have burnable behavior
    /// UPDATED: Now includes hardcoded smoke settings for both vanilla and custom fuel types
    /// </summary>
    internal static class GearItemData
    {
        // Combined name patterns from both vanilla and custom fuel data
        private static readonly Dictionary<string, CustomFuelType> allNamePatterns = new Dictionary<string, CustomFuelType>();

        // Static constructor to initialize combined data
        static GearItemData()
        {
            // Combine name patterns from both vanilla and custom data
            foreach (var kvp in VanillaFuelData.NamePatterns)
            {
                allNamePatterns[kvp.Key] = kvp.Value;
            }
            foreach (var kvp in CustomFuelData.NamePatterns)
            {
                allNamePatterns[kvp.Key] = kvp.Value;
            }
        }

        // Public methods for accessing data
        public static CustomFuelType GetFuelTypeFromName(string itemName)
        {
            if (string.IsNullOrEmpty(itemName))
                return CustomFuelType.None;

            string lowerName = itemName.ToLower();

            foreach (var pattern in allNamePatterns)
            {
                if (lowerName.Contains(pattern.Key))
                {
                    return pattern.Value;
                }
            }

            return CustomFuelType.Other;
        }

        public static bool HasBurnableSettings(CustomFuelType fuelType)
        {
            // Only vanilla fuel types have burnable settings that need to be applied
            // Custom fuel types already have burnable behavior defined
            return VanillaFuelData.BurnableSettings.ContainsKey(fuelType);
        }

        public static GearBurnableSettings GetBurnableSettings(CustomFuelType fuelType)
        {
            // Only check vanilla data since custom items don't need burnable settings applied
            if (VanillaFuelData.BurnableSettings.ContainsKey(fuelType))
            {
                return VanillaFuelData.BurnableSettings[fuelType];
            }

            // Return default settings if not found (shouldn't happen for custom items)
            return new GearBurnableSettings
            {
                burnDurationHours = 0.25f,
                fireAgeMinutesBeforeAdding = 0f,
                fireStartSkillModifier = 0,
                heatIncrease = 5f,
                heatInnerRadius = 2.5f,
                heatOuterRadius = 6f,
                fireStartDurationModifier = 0f,
                isWet = false,
                isTinder = false,
                isBurntInFireTracked = false
            };
        }

        public static bool HasCustomSparkSettings(CustomFuelType fuelType)
        {
            bool hasVanilla = VanillaFuelData.SparkSettings.ContainsKey(fuelType) &&
                             VanillaFuelData.SparkSettings[fuelType].useCustomSettings;
            bool hasCustom = CustomFuelData.SparkSettings.ContainsKey(fuelType) &&
                            CustomFuelData.SparkSettings[fuelType].useCustomSettings;

            return hasVanilla || hasCustom;
        }

        public static GearSparkSettings GetSparkSettings(CustomFuelType fuelType)
        {
            // Check vanilla data first
            if (VanillaFuelData.SparkSettings.ContainsKey(fuelType))
            {
                return VanillaFuelData.SparkSettings[fuelType];
            }

            // Check custom data
            if (CustomFuelData.SparkSettings.ContainsKey(fuelType))
            {
                return CustomFuelData.SparkSettings[fuelType];
            }

            // Return default/global settings if not found
            return new GearSparkSettings
            {
                emissionMultiplier = 1.0f,
                lifetimeMultiplier = 1.0f,
                sizeMultiplier = 1.0f,
                speedMultiplier = 1.0f,
                sparkColor = Color.white,
                durationMultiplier = 1.0f,
                useCustomSettings = false
            };
        }

        public static bool HasCustomSmokeSettings(CustomFuelType fuelType)
        {
            bool hasVanilla = VanillaFuelData.SmokeSettings.ContainsKey(fuelType) &&
                             VanillaFuelData.SmokeSettings[fuelType].useCustomSettings;
            bool hasCustom = CustomFuelData.SmokeSettings.ContainsKey(fuelType) &&
                            CustomFuelData.SmokeSettings[fuelType].useCustomSettings;

            return hasVanilla || hasCustom;
        }

        public static GearSmokeSettings GetSmokeSettings(CustomFuelType fuelType)
        {
            // Check vanilla data first
            if (VanillaFuelData.SmokeSettings.ContainsKey(fuelType))
            {
                return VanillaFuelData.SmokeSettings[fuelType];
            }

            // Check custom data
            if (CustomFuelData.SmokeSettings.ContainsKey(fuelType))
            {
                return CustomFuelData.SmokeSettings[fuelType];
            }

            // Return default/global settings if not found
            return new GearSmokeSettings
            {
                densityMultiplier = 1.0f,
                lifetimeMultiplier = 1.0f,
                sizeMultiplier = 1.0f,
                speedMultiplier = 1.0f,
                opacityMultiplier = 1.0f,
                useCustomSettings = false
            };
        }

        public static string GetFuelTypeName(CustomFuelType fuelType)
        {
            // Check vanilla display names first
            if (VanillaFuelData.DisplayNames.ContainsKey(fuelType))
            {
                return VanillaFuelData.DisplayNames[fuelType];
            }

            // Check custom display names
            if (CustomFuelData.DisplayNames.ContainsKey(fuelType))
            {
                return CustomFuelData.DisplayNames[fuelType];
            }

            // Fallback for unmapped types
            return fuelType switch
            {
                CustomFuelType.Other => "Other Fuel",
                _ => "No Fuel"
            };
        }

        // Check if an item should be made burnable (only applies to items that don't already have burnable behavior)
        public static bool ShouldMakeBurnable(string itemName)
        {
            var fuelType = GetFuelTypeFromName(itemName);

            // Only items that are specifically defined in VanillaFuelData.BurnableSettings need to be made burnable
            // This allows for vanilla items that already have burnable behavior to be tracked without modification
            return VanillaFuelData.BurnableSettings.ContainsKey(fuelType);
        }

        // Get hardcoded fire color for custom fuels
        public static Color GetFireColor(CustomFuelType fuelType)
        {
            if (CustomFuelData.FireColors.ContainsKey(fuelType))
            {
                return CustomFuelData.FireColors[fuelType];
            }

            return Color.white; // Default fallback
        }

        // Get hardcoded transition speed for custom fuels
        public static float GetTransitionSpeed()
        {
            return CustomFuelData.TRANSITION_SPEED;
        }

        // Get all registered fuel types for debugging - dynamically assembled
        public static CustomFuelType[] GetAllFuelTypes()
        {
            var allTypes = new List<CustomFuelType>();
            allTypes.AddRange(VanillaFuelData.AllVanillaFuelTypes);
            allTypes.AddRange(CustomFuelData.AllCustomFuelTypes);
            return allTypes.ToArray();
        }

        // Helper methods for categorization - using the arrays from each data file
        public static bool IsVanillaFuelType(CustomFuelType fuelType)
        {
            return System.Array.IndexOf(VanillaFuelData.AllVanillaFuelTypes, fuelType) >= 0;
        }

        public static bool IsCustomFuelType(CustomFuelType fuelType)
        {
            return System.Array.IndexOf(CustomFuelData.AllCustomFuelTypes, fuelType) >= 0;
        }

        // Get fuel types - directly from the data files
        public static CustomFuelType[] GetVanillaFuelTypes()
        {
            return VanillaFuelData.AllVanillaFuelTypes;
        }

        public static CustomFuelType[] GetCustomFuelTypes()
        {
            return CustomFuelData.AllCustomFuelTypes;
        }

        // Helper method to check if a ParticleSystem is still alive
        private static bool IsParticleSystemAlive(ParticleSystem ps)
        {
            try
            {
                return ps != null && ps.gameObject != null && ps.gameObject.activeInHierarchy;
            }
            catch
            {
                return false;
            }
        }

        public static float GetBurnDurationHours(CustomFuelType fuelType)
        {
            // Check vanilla fuels first
            if (GearItemData.IsVanillaFuelType(fuelType))
            {
                if (VanillaFuelData.BurnableSettings.ContainsKey(fuelType))
                {
                    return VanillaFuelData.BurnableSettings[fuelType].burnDurationHours;
                }
            }

            // Check custom fuels - find a gear item instance and read its FuelSourceItem burn duration
            if (GearItemData.IsCustomFuelType(fuelType))
            {
                var gearItem = FindGearItemOfType(fuelType);
                if (gearItem != null)
                {
                    var fuelSource = gearItem.GetComponent<FuelSourceItem>();
                    if (fuelSource != null)
                    {
                        return fuelSource.m_BurnDurationHours;
                    }
                }
            }

            // Default fallback
            return 10.0f;
        }

        private static GearItem FindGearItemOfType(CustomFuelType fuelType)
        {
            try
            {
                // Find all GearItem instances in the scene
                var allGearItems = UnityEngine.Object.FindObjectsOfType<GearItem>();
                if (allGearItems == null || allGearItems.Length == 0)
                    return null;

                // Get the expected name pattern for this fuel type
                string fuelTypeName = GetFuelTypeName(fuelType);

                // Search for a matching gear item
                foreach (var gearItem in allGearItems)
                {
                    if (gearItem == null || gearItem.gameObject == null)
                        continue;

                    var fuelTypeFromName = GetFuelTypeFromName(gearItem.name);
                    if (fuelTypeFromName == fuelType)
                    {
                        return gearItem;
                    }
                }
            }
            catch
            {
                // Silently fail if there's an issue finding items
            }

            return null;
        }
    }
}