using UnityEngine;
using MelonLoader;
using System.Collections.Generic;

namespace WildFire
{
    /// <summary>
    /// Dedicated handler for spark effect modifications
    /// FIXED: Proper logic for global vs fuel-specific spark settings with enhanced debugging
    /// UPDATED: Now properly applies durationMultiplier to particle effects
    /// UPDATED: Skips spark modifications for specific fire types (Wood Stove C, Pot Belly Stove, Ammo Workbench, 6-Burner Stove)
    /// </summary>
    internal static class SparkEffectsModifier
    {
        // Dictionary to store original particle system values to prevent cumulative modifications
        private static readonly Dictionary<int, OriginalSparkValues> originalValues = new Dictionary<int, OriginalSparkValues>();

        private struct OriginalSparkValues
        {
            public float originalEmissionRate;
            public float originalLifetime;
            public float originalSize;
            public float originalSpeed;
            public Color originalColor;
            public bool isInitialized;
        }

        public static void ApplySparkModifications(ParticleSystem ps, FireType fireType, GameObject fireObject)
        {
            if (ps == null) return;

            try
            {
                if (ps.gameObject == null || !ps.gameObject.activeInHierarchy)
                    return;

                // CRITICAL CHECK: Skip spark modifications for specific fire types
                if (FireUtils.ShouldSkipSparkModifications(fireObject))
                {
                    //MelonLogger.Msg($"Skipping spark modifications for excluded fire type: {ps.name}");
                    return;
                }

                MelonLogger.Msg($"Applying spark-specific modifications to: {ps.name}");

                // Store original values if not already stored
                StoreOriginalValues(ps);

                // FIXED: Enhanced logic with better debugging
                if (Settings.options.enableSparkModifications)
                {
                    // Global spark settings are enabled - they override everything
                    //MelonLogger.Msg("Using global spark settings (enabled and overriding fuel-specific settings)");
                    ApplyGlobalSparkSettings(ps);
                }
                else
                {
                    // Global spark settings are disabled - use fuel-specific hardcoded settings if available
                    CustomFuelType recentFuelType = FuelColorTracker.GetMostRecentFuelType(fireObject);
                    //MelonLogger.Msg($"DEBUG: Global spark settings disabled. Recent fuel type: {recentFuelType}");

                    bool hasCustomSettings = GearItemData.HasCustomSparkSettings(recentFuelType);
                    //MelonLogger.Msg($"DEBUG: Has custom spark settings for {recentFuelType}: {hasCustomSettings}");

                    if (recentFuelType != CustomFuelType.None && hasCustomSettings)
                    {
                        // Use fuel-specific hardcoded settings
                        var fuelName = GearItemData.GetFuelTypeName(recentFuelType);
                        //MelonLogger.Msg($"Global spark settings disabled, using hardcoded fuel-specific settings for: {fuelName}");
                        ApplyFuelSpecificSparkSettings(ps, recentFuelType);
                    }
                    else
                    {
                        // Debug why we're not applying modifications
                        if (recentFuelType == CustomFuelType.None)
                        {
                            //MelonLogger.Msg("DEBUG: No recent fuel type found - this might be a timing issue");

                            // Let's try to get fuel data directly and see what's available
                            int activeFuels = FuelColorTracker.GetActiveFuelCount(fireObject);
                            //MelonLogger.Msg($"DEBUG: Active fuel count: {activeFuels}");

                            if (activeFuels > 0)
                            {
                                //MelonLogger.Msg("DEBUG: Active fuels exist but GetMostRecentFuelType returned None - possible timing issue");
                                // Try again with a small delay or use a fallback method
                                // For now, let's log the fire's fuel data for debugging
                                FuelColorTracker.DebugFireColors(fireObject);
                            }
                        }
                        else if (!hasCustomSettings)
                        {
                            //MelonLogger.Msg($"DEBUG: Fuel type {recentFuelType} does not have custom spark settings");
                        }

                        //MelonLogger.Msg($"No hardcoded fuel-specific settings available and global spark settings disabled - no spark modifications applied. RecentFuelType: {recentFuelType}, HasCustom: {hasCustomSettings}");
                        // Optionally restore original values here if you want to reset to defaults
                        // RestoreOriginalValues(ps);
                    }
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error applying spark modifications: {e.Message}");
            }
        }

        public static bool IsSparkParticleSystem(ParticleSystem ps)
        {
            if (ps?.gameObject?.name == null) return false;

            string name = ps.gameObject.name.ToLower();

            // Check for common spark/ember particle system names
            bool isSparkSystem = name.Contains("spark") ||
                   name.Contains("ember") ||
                   name.Contains("otherfx") ||
                   name.Contains("other_fx") ||
                   name.Contains("fuel") ||
                   name.Contains("add") ||
                   (name.Contains("fx") && !name.Contains("flare"));

            if (isSparkSystem)
            {
                //MelonLogger.Msg($"DEBUG: Identified spark particle system: {ps.gameObject.name}");
            }

            return isSparkSystem;
        }

        /// <summary>
        /// Stores the original values of a particle system before any modifications
        /// This prevents cumulative modifications when fuel is added multiple times
        /// </summary>
        private static void StoreOriginalValues(ParticleSystem ps)
        {
            try
            {
                int instanceId = ps.GetInstanceID();

                if (!originalValues.ContainsKey(instanceId) || !originalValues[instanceId].isInitialized)
                {
                    var values = new OriginalSparkValues();

                    try
                    {
                        // Store original emission rate
                        values.originalEmissionRate = ps.emission.rateOverTime.constant;
                    }
                    catch
                    {
                        values.originalEmissionRate = 30.0f; // Default spark emission
                    }

                    try
                    {
                        // Store original lifetime
                        values.originalLifetime = ps.main.startLifetime.constant;
                    }
                    catch
                    {
                        values.originalLifetime = 3.0f; // Default spark lifetime
                    }

                    try
                    {
                        // Store original size
                        values.originalSize = ps.main.startSize.constant;
                    }
                    catch
                    {
                        values.originalSize = 0.5f; // Default spark size
                    }

                    try
                    {
                        // Store original speed
                        values.originalSpeed = ps.main.startSpeed.constant;
                    }
                    catch
                    {
                        values.originalSpeed = 8.0f; // Default spark speed
                    }

                    try
                    {
                        // Store original color
                        values.originalColor = ps.main.startColor.color;
                    }
                    catch
                    {
                        values.originalColor = Color.white; // Default spark color
                    }

                    values.isInitialized = true;
                    originalValues[instanceId] = values;

                    //MelonLogger.Msg($"Stored original values for particle system: {ps.name} (ID: {instanceId})");
                    //MelonLogger.Msg($"  Original - Emission: {values.originalEmissionRate:F1}, Lifetime: {values.originalLifetime:F1}, Size: {values.originalSize:F2}, Speed: {values.originalSpeed:F1}, Color: R{values.originalColor.r:F2}G{values.originalColor.g:F2}B{values.originalColor.b:F2}");
                }
            }
            catch (System.Exception e)
            {
                //MelonLogger.Warning($"Could not store original values for {ps.name}: {e.Message}");
            }
        }

        /// <summary>
        /// Gets the stored original values for a particle system
        /// </summary>
        private static OriginalSparkValues GetOriginalValues(ParticleSystem ps)
        {
            int instanceId = ps.GetInstanceID();
            if (originalValues.ContainsKey(instanceId))
            {
                return originalValues[instanceId];
            }

            // Return default values if not found
            return new OriginalSparkValues
            {
                originalEmissionRate = 30.0f,
                originalLifetime = 3.0f,
                originalSize = 0.5f,
                originalSpeed = 8.0f,
                originalColor = Color.white,
                isInitialized = false
            };
        }

        /// <summary>
        /// NEW: Restores a particle system to its original values
        /// Useful when you want to reset spark effects to defaults
        /// </summary>
        private static void RestoreOriginalValues(ParticleSystem ps)
        {
            try
            {
                var originalValuesData = GetOriginalValues(ps);
                if (!originalValuesData.isInitialized) return;

                var main = ps.main;
                var emission = ps.emission;

                main.startColor = originalValuesData.originalColor;
                main.startLifetime = originalValuesData.originalLifetime;
                main.startSize = originalValuesData.originalSize;
                main.startSpeed = originalValuesData.originalSpeed;
                emission.rateOverTime = originalValuesData.originalEmissionRate;

                //MelonLogger.Msg($"Restored original values for particle system: {ps.name}");
            }
            catch (System.Exception e)
            {
                //MelonLogger.Warning($"Could not restore original values for {ps.name}: {e.Message}");
            }
        }

        /// <summary>
        /// Cleans up stored values for destroyed particle systems
        /// Should be called when scenes change or fires are destroyed
        /// </summary>
        public static void CleanupOriginalValues()
        {
            // Remove entries for destroyed objects
            var keysToRemove = new List<int>();
            foreach (var kvp in originalValues)
            {
                // Check if the particle system still exists
                bool exists = false;
                try
                {
                    // Try to find any object with this instance ID
                    var objects = UnityEngine.Object.FindObjectsOfType<ParticleSystem>();
                    foreach (var obj in objects)
                    {
                        if (obj.GetInstanceID() == kvp.Key)
                        {
                            exists = true;
                            break;
                        }
                    }
                }
                catch
                {
                    exists = false;
                }

                if (!exists)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (int key in keysToRemove)
            {
                originalValues.Remove(key);
            }

            if (keysToRemove.Count > 0)
            {
                //MelonLogger.Msg($"Cleaned up {keysToRemove.Count} orphaned original value entries");
            }
        }

        private static void ApplyFuelSpecificSparkSettings(ParticleSystem ps, CustomFuelType fuelType)
        {
            var sparkSettings = FuelColorTracker.GetSparkSettingsForFuel(fuelType);
            var fuelName = GearItemData.GetFuelTypeName(fuelType);

            //MelonLogger.Msg($"Using HARDCODED spark settings for {fuelType}: Emission={sparkSettings.emissionMultiplier:F1}x, Lifetime={sparkSettings.lifetimeMultiplier:F1}x, Size={sparkSettings.sizeMultiplier:F1}x, Speed={sparkSettings.speedMultiplier:F1}x, Duration={sparkSettings.durationMultiplier:F1}x, Color=R{sparkSettings.sparkColor.r:F2}G{sparkSettings.sparkColor.g:F2}B{sparkSettings.sparkColor.b:F2}");

            //MelonLogger.Msg($"Applying fuel-specific settings for {fuelName}:");
            //MelonLogger.Msg($"  Emission: {sparkSettings.emissionMultiplier:F2}x");
            //MelonLogger.Msg($"  Lifetime: {sparkSettings.lifetimeMultiplier:F2}x");
            //MelonLogger.Msg($"  Size: {sparkSettings.sizeMultiplier:F2}x");
            //MelonLogger.Msg($"  Speed: {sparkSettings.speedMultiplier:F2}x");
            //MelonLogger.Msg($"  Color: R{sparkSettings.sparkColor.r:F2}G{sparkSettings.sparkColor.g:F2}B{sparkSettings.sparkColor.b:F2}");

            // Apply fuel-specific spark color
            TryApplySparkColor(ps, sparkSettings.sparkColor);

            // Apply fuel-specific spark emission
            TryApplySparkEmission(ps, sparkSettings.emissionMultiplier);

            // FIXED: Apply both lifetime and duration multipliers together
            TryApplySparkLifetime(ps, sparkSettings.lifetimeMultiplier, sparkSettings.durationMultiplier);

            // Apply fuel-specific spark size
            TryApplySparkSize(ps, sparkSettings.sizeMultiplier);

            // Apply fuel-specific spark speed
            TryApplySparkSpeed(ps, sparkSettings.speedMultiplier);
        }

        private static void ApplyGlobalSparkSettings(ParticleSystem ps)
        {
            //MelonLogger.Msg("Applying global spark settings from configuration");

            // Apply global spark color
            Color globalSparkColor = FireUtils.RGBToColor(
                Settings.options.sparkColorR,
                Settings.options.sparkColorG,
                Settings.options.sparkColorB);
            TryApplySparkColor(ps, globalSparkColor);

            // Apply global spark settings
            TryApplySparkEmission(ps, Settings.options.sparkEmissionMultiplier);
            TryApplySparkLifetime(ps, Settings.options.sparkLifetimeMultiplier, Settings.options.sparkDurationMultiplier);
            TryApplySparkSize(ps, Settings.options.sparkSizeMultiplier);
            TryApplySparkSpeed(ps, Settings.options.sparkSpeedMultiplier);
        }

        private static void TryApplySparkColor(ParticleSystem ps, Color sparkColor)
        {
            try
            {
                var main = ps.main;
                main.startColor = sparkColor;
                //MelonLogger.Msg($"  [SUCCESS] Applied spark color: R={sparkColor.r:F2}, G={sparkColor.g:F2}, B={sparkColor.b:F2}");
            }
            catch (System.Exception e)
            {
                //MelonLogger.Warning($"  [FAILED] Could not apply spark color: {e.Message}");
            }
        }

        private static void TryApplySparkEmission(ParticleSystem ps, float emissionMultiplier)
        {
            try
            {
                var emission = ps.emission;
                var originalValuesData = GetOriginalValues(ps);

                try
                {
                    // Use original value instead of current value to prevent cumulative modifications
                    float newRateValue = originalValuesData.originalEmissionRate * emissionMultiplier;

                    emission.rateOverTime = Mathf.Max(0f, newRateValue);
                    //MelonLogger.Msg($"  [SUCCESS] Applied spark emission rate: {newRateValue:F2} (original: {originalValuesData.originalEmissionRate:F1}, multiplier: {emissionMultiplier:F1}x)");
                }
                catch (System.Exception e)
                {
                    //MelonLogger.Warning($"  [FAILED] Spark emission rate error: {e.Message}");
                }
            }
            catch (System.Exception e)
            {
                //MelonLogger.Warning($"  [FAILED] Could not apply spark emission: {e.Message}");
            }
        }

        // FIXED: Now properly applies durationMultiplier to particle lifetime
        private static void TryApplySparkLifetime(ParticleSystem ps, float lifetimeMultiplier, float durationMultiplier)
        {
            try
            {
                var main = ps.main;
                var originalValuesData = GetOriginalValues(ps);

                try
                {
                    // FIXED: Combine both multipliers - durationMultiplier extends the individual spark lifetime
                    // This makes visual sense: duration affects how long each spark particle lives
                    float combinedMultiplier = lifetimeMultiplier * durationMultiplier;
                    float newLifetimeValue = originalValuesData.originalLifetime * combinedMultiplier;

                    main.startLifetime = Mathf.Max(0.1f, newLifetimeValue);
                    //MelonLogger.Msg($"  [SUCCESS] Applied spark lifetime: {newLifetimeValue:F2} seconds (original: {originalValuesData.originalLifetime:F1}, lifetime: {lifetimeMultiplier:F1}x, duration: {durationMultiplier:F1}x)");
                }
                catch (System.Exception e)
                {
                    //MelonLogger.Warning($"  [FAILED] Spark lifetime modification error: {e.Message}");
                }
            }
            catch (System.Exception e)
            {
                //MelonLogger.Warning($"  [FAILED] Could not apply spark lifetime: {e.Message}");
            }
        }

        private static void TryApplySparkSize(ParticleSystem ps, float sizeMultiplier)
        {
            try
            {
                var main = ps.main;
                var originalValuesData = GetOriginalValues(ps);

                try
                {
                    // Use original value instead of current value to prevent cumulative modifications
                    float newSizeValue = originalValuesData.originalSize * sizeMultiplier;

                    main.startSize = Mathf.Max(0.01f, newSizeValue);
                    //MelonLogger.Msg($"  [SUCCESS] Applied spark size: {newSizeValue:F2} (original: {originalValuesData.originalSize:F2}, multiplier: {sizeMultiplier:F1}x)");
                }
                catch (System.Exception e)
                {
                    //MelonLogger.Warning($"  [FAILED] Spark size modification error: {e.Message}");
                }
            }
            catch (System.Exception e)
            {
                //MelonLogger.Warning($"  [FAILED] Could not apply spark size: {e.Message}");
            }
        }

        private static void TryApplySparkSpeed(ParticleSystem ps, float speedMultiplier)
        {
            try
            {
                var main = ps.main;
                var originalValuesData = GetOriginalValues(ps);

                try
                {
                    // Use original value instead of current value to prevent cumulative modifications
                    float newSpeedValue = originalValuesData.originalSpeed * speedMultiplier;

                    main.startSpeed = newSpeedValue;
                    //MelonLogger.Msg($"  [SUCCESS] Applied spark speed: {newSpeedValue:F2} (original: {originalValuesData.originalSpeed:F1}, multiplier: {speedMultiplier:F1}x)");
                }
                catch (System.Exception e)
                {
                    //MelonLogger.Warning($"  [FAILED] Spark speed modification error: {e.Message}");
                }
            }
            catch (System.Exception e)
            {
                //MelonLogger.Warning($"  [FAILED] Could not apply spark speed: {e.Message}");
            }
        }
    }
}