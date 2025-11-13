using UnityEngine;
using System.Collections.Generic;
using MelonLoader;
using System.Linq;

namespace WildFire
{
    internal class FuelColorData
    {
        public CustomFuelType fuelType;
        public Color color;
        public float timeAdded;
        public float duration;

        public bool IsExpired => Time.time - timeAdded > duration;
    }

    public struct SparkSettings
    {
        public float emissionMultiplier;
        public float lifetimeMultiplier;
        public float sizeMultiplier;
        public float speedMultiplier;
        public Color sparkColor;
        public float durationMultiplier;
    }

    /// <summary>
    /// Tracks which fuel currently dominates the smoke appearance for a fire
    /// </summary>
    internal struct SmokeProfile
    {
        public CustomFuelType dominantFuelType;
        public GearSmokeSettings smokeSettings;
        public float expiryTime;
        public bool isValid;
    }

    /// <summary>
    /// Tracks fuel colors and manages color blending for fires
    /// All fuel colors use hardcoded settings - vanilla and custom
    /// Color duration is based on item burnDurationHours with a configurable multiplier
    /// UPDATED: Now tracks smoke dominance separately - smoke follows the thickest active fuel
    /// FIXED: Light color now updates BEFORE refreshing effects to prevent desync
    /// </summary>
    internal static class FuelColorTracker
    {
        // ===== EASY-TO-ADJUST CONFIGURATION =====
        private static float COLOR_DURATION_MULTIPLIER => Settings.options.fireDuration;
        private const float GAME_TIME_TO_REAL_SECONDS = 300f;
        // =========================================

        private const float FUEL_COLOR_TRANSITION_SPEED = .25f;

        private static Dictionary<GameObject, List<FuelColorData>> fireColorData = new Dictionary<GameObject, List<FuelColorData>>();
        private static Dictionary<GameObject, EffectsControllerFire> trackedFireControllers = new Dictionary<GameObject, EffectsControllerFire>();

        // NEW: Track smoke dominance per fire
        private static Dictionary<GameObject, SmokeProfile> fireSmokeProfiles = new Dictionary<GameObject, SmokeProfile>();

        private static float lastRefreshTime = 0f;
        private static float refreshInterval = 1f;

        public static CustomFuelType GetFuelTypeFromName(string itemName)
        {
            var fuelType = GearItemData.GetFuelTypeFromName(itemName);
            return fuelType;
        }

        public static Color GetColorForFuelType(CustomFuelType fuelType)
        {
            bool isVanillaFuel = fuelType == CustomFuelType.FlareA ||
                                fuelType == CustomFuelType.BlueFlare ||
                                fuelType == CustomFuelType.DustingSulfur ||
                                fuelType == CustomFuelType.StumpRemover ||
                                fuelType == CustomFuelType.GunpowderCan ||
                                fuelType == CustomFuelType.FlareGunAmmoSingle;

            if (isVanillaFuel)
            {
                var color = VanillaFuelData.FireColors[fuelType];
                return color;
            }

            bool isCustomFuel = fuelType == CustomFuelType.BariumCarbonate ||
                               fuelType == CustomFuelType.MagnesiumPowder ||
                               fuelType == CustomFuelType.IronFilings ||
                               fuelType == CustomFuelType.Rubidium ||
                               fuelType == CustomFuelType.CalciumChloride ||
                               fuelType == CustomFuelType.CopperCarbonate;

            if (isCustomFuel)
            {
                var color = GearItemData.GetFireColor(fuelType);
                return color;
            }

            return Color.white;
        }

        public static SparkSettings GetSparkSettingsForFuel(CustomFuelType fuelType)
        {
            if (GearItemData.HasCustomSparkSettings(fuelType))
            {
                var gearSpark = GearItemData.GetSparkSettings(fuelType);
                var settings = new SparkSettings
                {
                    emissionMultiplier = gearSpark.emissionMultiplier,
                    lifetimeMultiplier = gearSpark.lifetimeMultiplier,
                    sizeMultiplier = gearSpark.sizeMultiplier,
                    speedMultiplier = gearSpark.speedMultiplier,
                    sparkColor = gearSpark.sparkColor,
                    durationMultiplier = gearSpark.durationMultiplier
                };

                return settings;
            }

            var globalSettings = new SparkSettings
            {
                emissionMultiplier = Settings.options.sparkEmissionMultiplier,
                lifetimeMultiplier = Settings.options.sparkLifetimeMultiplier,
                sizeMultiplier = Settings.options.sparkSizeMultiplier,
                speedMultiplier = Settings.options.sparkSpeedMultiplier,
                sparkColor = FireUtils.RGBToColor(
                    Settings.options.sparkColorR,
                    Settings.options.sparkColorG,
                    Settings.options.sparkColorB),
                durationMultiplier = Settings.options.sparkDurationMultiplier
            };

            return globalSettings;
        }

        public static bool HasCustomSparkSettings(CustomFuelType fuelType)
        {
            return GearItemData.HasCustomSparkSettings(fuelType);
        }

        public static CustomFuelType GetMostRecentFuelType(GameObject fireObject)
        {
            if (fireObject == null || !fireColorData.ContainsKey(fireObject))
            {
                return CustomFuelType.None;
            }

            CleanExpiredEntries(fireObject);

            var activeFuels = fireColorData[fireObject].Where(f => !f.IsExpired).ToList();
            if (activeFuels.Count == 0)
            {
                return CustomFuelType.None;
            }

            var mostRecentFuel = activeFuels.OrderByDescending(f => f.timeAdded).First();
            return mostRecentFuel.fuelType;
        }

        private static float CalculateFuelColorDuration(CustomFuelType fuelType)
        {
            float burnDurationHours = GearItemData.GetBurnDurationHours(fuelType);
            float colorDurationInGameHours = burnDurationHours * COLOR_DURATION_MULTIPLIER;
            float colorDurationInRealSeconds = colorDurationInGameHours * GAME_TIME_TO_REAL_SECONDS;

            return colorDurationInRealSeconds;
        }

        /// <summary>
        /// NEW: Compare smoke dominance between two fuels
        /// Returns true if newSmoke has thicker/longer smoke than currentSmoke
        /// </summary>
        private static bool IsMoreDominantSmoke(GearSmokeSettings newSmoke, GearSmokeSettings currentSmoke)
        {
            // Simple comparison: density is the primary factor for "thickness"
            // If densities are equal, compare lifetime as tiebreaker
            if (newSmoke.densityMultiplier > currentSmoke.densityMultiplier)
                return true;

            if (newSmoke.densityMultiplier == currentSmoke.densityMultiplier &&
                newSmoke.lifetimeMultiplier > currentSmoke.lifetimeMultiplier)
                return true;

            return false;
        }

        /// <summary>
        /// NEW: Find the fuel with the most dominant smoke from active fuels
        /// Returns the fuel type and its smoke settings, or None if no active fuels
        /// FIXED: Always compares all fuels to find the one with thickest smoke
        /// </summary>
        private static (CustomFuelType fuelType, GearSmokeSettings smokeSettings, float expiryTime)
            FindDominantSmokeFuel(GameObject fireObject)
        {
            if (!fireColorData.ContainsKey(fireObject))
                return (CustomFuelType.None, default, 0f);

            CleanExpiredEntries(fireObject);
            var activeFuels = fireColorData[fireObject].Where(f => !f.IsExpired).ToList();

            if (activeFuels.Count == 0)
                return (CustomFuelType.None, default, 0f);

            // Start with the first fuel as the initial dominant
            CustomFuelType dominantType = activeFuels[0].fuelType;
            GearSmokeSettings dominantSmoke = GearItemData.GetSmokeSettings(dominantType);
            float dominantExpiry = activeFuels[0].timeAdded + activeFuels[0].duration;

            // Compare ALL fuels to find the most dominant (thickest) smoke
            foreach (var fuel in activeFuels.Skip(1))
            {
                var fuelSmoke = GearItemData.GetSmokeSettings(fuel.fuelType);
                float fuelExpiry = fuel.timeAdded + fuel.duration;

                // Only switch to this fuel if it has MORE dominant smoke
                // This ensures thick smoke stays dominant even when lighter smoke is added
                if (IsMoreDominantSmoke(fuelSmoke, dominantSmoke))
                {
                    dominantType = fuel.fuelType;
                    dominantSmoke = fuelSmoke;
                    dominantExpiry = fuelExpiry;

                    var fuelName = GearItemData.GetFuelTypeName(fuel.fuelType);
                    //MelonLogger.Msg($"Found more dominant smoke: {fuelName} (density: {fuelSmoke.densityMultiplier}x)");
                }
            }

            var dominantName = GearItemData.GetFuelTypeName(dominantType);
            //MelonLogger.Msg($"Dominant smoke fuel determined: {dominantName} (density: {dominantSmoke.densityMultiplier}x)");

            return (dominantType, dominantSmoke, dominantExpiry);
        }

        /// <summary>
        /// NEW: Get the current smoke settings for a fire
        /// Returns the settings from the dominant smoke profile if one exists
        /// </summary>
        public static (bool hasProfile, GearSmokeSettings settings) GetCurrentSmokeSettings(GameObject fireObject)
        {
            if (fireObject == null || !fireSmokeProfiles.ContainsKey(fireObject))
            {
                return (false, default);
            }

            var profile = fireSmokeProfiles[fireObject];
            if (!profile.isValid)
            {
                return (false, default);
            }

            return (true, profile.smokeSettings);
        }
        private static void UpdateSmokeProfile(GameObject fireObject, EffectsControllerFire effectsController)
        {
            if (fireObject == null || effectsController == null)
                return;

            try
            {
                var (dominantFuelType, dominantSmoke, expiryTime) = FindDominantSmokeFuel(fireObject);

                if (dominantFuelType == CustomFuelType.None)
                {
                    // No active fuels - restore baseline
                    //MelonLogger.Msg($"No active fuels for {fireObject.name}, restoring smoke to baseline");
                    RestoreAllSmokeEffects(effectsController);
                    fireSmokeProfiles[fireObject] = new SmokeProfile { isValid = false };
                }
                else
                {
                    var currentProfile = fireSmokeProfiles.ContainsKey(fireObject) ? fireSmokeProfiles[fireObject] : default;

                    // Only update if this is a new dominant fuel or we need to switch
                    if (!currentProfile.isValid || currentProfile.dominantFuelType != dominantFuelType)
                    {
                        var fuelName = GearItemData.GetFuelTypeName(dominantFuelType);
                        //MelonLogger.Msg($"Smoke profile changed for {fireObject.name}: now using {fuelName} (density: {dominantSmoke.densityMultiplier}x, lifetime: {dominantSmoke.lifetimeMultiplier}x)");

                        // Apply the new smoke profile
                        ApplySmokeDominanceProfile(effectsController, dominantSmoke, fireObject);

                        fireSmokeProfiles[fireObject] = new SmokeProfile
                        {
                            dominantFuelType = dominantFuelType,
                            smokeSettings = dominantSmoke,
                            expiryTime = expiryTime,
                            isValid = true
                        };
                    }
                    else
                    {
                        var fuelName = GearItemData.GetFuelTypeName(dominantFuelType);
                        //MelonLogger.Msg($"Smoke profile unchanged for {fireObject.name}: still using {fuelName} (density: {dominantSmoke.densityMultiplier}x)");
                    }
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error updating smoke profile: {e.Message}");
            }
        }

        /// <summary>
        /// NEW: Apply smoke settings to all smoke particle systems in the fire
        /// </summary>
        private static void ApplySmokeDominanceProfile(EffectsControllerFire effectsController, GearSmokeSettings smokeSettings, GameObject fireObject)
        {
            try
            {
                if (effectsController == null || effectsController.gameObject == null)
                    return;

                // CRITICAL CHECK: Skip smoke modifications entirely for 6-burner stove
                if (FireUtils.IsSixBurnerStove(fireObject))
                {
                    //MelonLogger.Msg($"Skipping smoke modifications for 6-burner stove");
                    return;
                }

                var allParticleSystems = effectsController.GetComponentsInChildren<ParticleSystem>();

                if (allParticleSystems != null && allParticleSystems.Length > 0)
                {
                    foreach (var ps in allParticleSystems)
                    {
                        if (ps != null && ps.gameObject.activeInHierarchy)
                        {
                            if (SmokeEffectsModifier.IsSmokeParticleSystem(ps))
                            {
                                //MelonLogger.Msg($"Applying smoke dominance profile to: {ps.gameObject.name}");
                                SmokeEffectsModifier.ApplySmokeBaselineDirectly(ps, smokeSettings);
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error applying smoke dominance profile: {e.Message}");
            }
        }

        public static void AddFuelToFire(GameObject fireObject, CustomFuelType fuelType)
        {
            if (fireObject == null || fuelType == CustomFuelType.None)
            {
                return;
            }

            try
            {
                if (!fireColorData.ContainsKey(fireObject))
                {
                    fireColorData[fireObject] = new List<FuelColorData>();
                }

                var existingFuel = fireColorData[fireObject].FirstOrDefault(f => f.fuelType == fuelType && !f.IsExpired);

                if (existingFuel != null)
                {
                    existingFuel.timeAdded = Time.time;
                    var fuelName = GearItemData.GetFuelTypeName(fuelType);
                    //MelonLogger.Msg($"Refreshed existing {fuelName} in fire {fireObject.name}");
                }
                else
                {
                    var colorData = new FuelColorData
                    {
                        fuelType = fuelType,
                        color = GetColorForFuelType(fuelType),
                        timeAdded = Time.time,
                        duration = CalculateFuelColorDuration(fuelType)
                    };

                    fireColorData[fireObject].Add(colorData);

                    var fuelName = GearItemData.GetFuelTypeName(fuelType);
                    //MelonLogger.Msg($"Added new {fuelName} to fire {fireObject.name}");
                }

                var effectsController = fireObject.GetComponent<EffectsControllerFire>();
                if (effectsController != null && !trackedFireControllers.ContainsKey(fireObject))
                {
                    trackedFireControllers[fireObject] = effectsController;
                    //MelonLogger.Msg($"Added fire {fireObject.name} to automatic refresh tracking");
                }

                var color = GetColorForFuelType(fuelType);
                FireLightOverrideManager.RegisterFireLightOverride(fireObject, color);

                // NEW: Update smoke profile when fuel is added
                if (effectsController != null)
                {
                    UpdateSmokeProfile(fireObject, effectsController);
                }

                CleanExpiredEntries(fireObject);
                DebugFireColors(fireObject);
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error adding fuel to fire: {e.Message}");
            }
        }

        public static Color GetCurrentFireColor(GameObject fireObject, FireType fireType)
        {
            if (fireObject == null || !fireColorData.ContainsKey(fireObject))
            {
                var defaultColor = GetDefaultColorForFireType(fireType);
                return defaultColor;
            }

            try
            {
                CleanExpiredEntries(fireObject);
                var activeFuels = fireColorData[fireObject].Where(f => !f.IsExpired).ToList();

                if (activeFuels.Count == 0)
                {
                    var defaultColor = GetDefaultColorForFireType(fireType);
                    return defaultColor;
                }

                if (activeFuels.Count > 1)
                {
                    var blendedColor = BlendFuelColors(activeFuels);
                    return blendedColor;
                }

                var mostRecentFuel = activeFuels.First();
                return mostRecentFuel.color;
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error getting current fire color: {e.Message}");
                return GetDefaultColorForFireType(fireType);
            }
        }

        public static bool HasAnyTrackedFires()
        {
            return trackedFireControllers.Count > 0;
        }

        public static void UpdateFireColors()
        {
            if (Time.time - lastRefreshTime < refreshInterval) return;

            lastRefreshTime = Time.time;

            if (trackedFireControllers.Count == 0) return;

            var firesToRemove = new List<GameObject>();

            foreach (var kvp in trackedFireControllers.ToList())
            {
                var fireObject = kvp.Key;
                var effectsController = kvp.Value;

                try
                {
                    if (fireObject == null || effectsController == null)
                    {
                        firesToRemove.Add(fireObject);
                        continue;
                    }

                    if (!fireColorData.ContainsKey(fireObject))
                    {
                        firesToRemove.Add(fireObject);
                        continue;
                    }

                    var beforeCount = fireColorData[fireObject].Count;
                    CleanExpiredEntries(fireObject);

                    bool hadExpiredFuels = false;
                    if (fireColorData.ContainsKey(fireObject))
                    {
                        var afterCount = fireColorData[fireObject].Count;
                        hadExpiredFuels = beforeCount > afterCount;
                    }
                    else
                    {
                        hadExpiredFuels = beforeCount > 0;
                    }

                    if (hadExpiredFuels)
                    {
                        //MelonLogger.Msg($"Fuel expired for fire {fireObject.name}, re-evaluating smoke profile");
                        FireType fireType = FireTypeDetector.GetFireType(fireObject);

                        // Check if there are still active fuels remaining after cleanup
                        bool hasRemainingFuels = fireColorData.ContainsKey(fireObject) && fireColorData[fireObject].Count > 0;

                        if (hasRemainingFuels)
                        {
                            // FIXED: Calculate and update the fire light color BEFORE refreshing effects
                            // This ensures the light color matches the remaining fuel when RefreshAllFireEffects runs
                            Color currentColor = GetCurrentFireColor(fireObject, fireType);
                            FireLightOverrideManager.UpdateFireLightOverride(fireObject, currentColor);

                            // Update smoke profile to match the new dominant fuel
                            UpdateSmokeProfile(fireObject, effectsController);

                            // Now refresh all fire effects - they will see the correct light color
                            FirePatches.RefreshAllFireEffects(effectsController, fireType);
                        }
                        else
                        {
                            // No remaining fuels - restore to baseline
                            //MelonLogger.Msg($"All fuels expired for {fireObject.name}, restoring to baseline");

                            // Get baseline color and update light
                            Color baselineColor = GetDefaultColorForFireType(fireType);
                            FireLightOverrideManager.UpdateFireLightOverride(fireObject, baselineColor);

                            // Restore smoke to baseline
                            UpdateSmokeProfile(fireObject, effectsController);

                            // Refresh all effects with baseline settings
                            FirePatches.RefreshAllFireEffects(effectsController, fireType);
                        }
                    }

                    if (!fireColorData.ContainsKey(fireObject) || fireColorData[fireObject].Count == 0)
                    {
                        //MelonLogger.Msg($"No more active fuels for {fireObject.name}, removing from tracking");
                        FireLightOverrideManager.UnregisterFireLightOverride(fireObject);
                        fireSmokeProfiles.Remove(fireObject);
                        firesToRemove.Add(fireObject);
                    }
                }
                catch (System.Exception e)
                {
                    MelonLogger.Error($"Error updating fire colors for {fireObject?.name}: {e.Message}");
                    firesToRemove.Add(fireObject);
                }
            }

            foreach (var fireToRemove in firesToRemove)
            {
                if (fireToRemove != null && trackedFireControllers.ContainsKey(fireToRemove))
                {
                    trackedFireControllers.Remove(fireToRemove);
                }
            }
        }

        private static void RestoreAllSmokeEffects(EffectsControllerFire effectsController)
        {
            try
            {
                if (effectsController == null || effectsController.gameObject == null)
                    return;

                var allParticleSystems = effectsController.GetComponentsInChildren<ParticleSystem>();

                if (allParticleSystems != null && allParticleSystems.Length > 0)
                {
                    foreach (var ps in allParticleSystems)
                    {
                        if (ps != null && ps.gameObject.activeInHierarchy)
                        {
                            if (SmokeEffectsModifier.IsSmokeParticleSystem(ps))
                            {
                                //MelonLogger.Msg($"Restoring smoke to baseline state: {ps.gameObject.name}");
                                SmokeEffectsModifier.RestoreSmokeModifications(ps);
                                SmokeEffectsModifier.ClearModificationTrackingFor(ps);
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error restoring smoke effects: {e.Message}");
            }
        }

        private static Color BlendFuelColors(List<FuelColorData> activeFuels)
        {
            if (activeFuels.Count == 0) return Color.white;
            if (activeFuels.Count == 1) return activeFuels[0].color;

            Vector3 weightedColorSum = Vector3.zero;
            float totalWeight = 0f;

            float transitionSpeed = FUEL_COLOR_TRANSITION_SPEED;

            foreach (var fuel in activeFuels)
            {
                float timeSinceAdded = Time.time - fuel.timeAdded;
                float ageRatio = Mathf.Clamp01(timeSinceAdded / fuel.duration);

                float weight = Mathf.Max(0.1f, Mathf.Pow(1f - ageRatio, 2f));

                float transitionFactor = Mathf.Max(0.1f, transitionSpeed);
                weight = Mathf.Pow(weight, 1f / transitionFactor);

                Vector3 colorVec = new Vector3(fuel.color.r, fuel.color.g, fuel.color.b);
                weightedColorSum += colorVec * weight;
                totalWeight += weight;
            }

            Vector3 averageColor = weightedColorSum / Mathf.Max(totalWeight, 0.001f);

            Color finalColor = new Color(
                Mathf.Clamp01(averageColor.x),
                Mathf.Clamp01(averageColor.y),
                Mathf.Clamp01(averageColor.z),
                1.0f
            );

            return finalColor;
        }

        private static Color GetDefaultColorForFireType(FireType fireType)
        {
            return FireUtils.RGBToColor(
                Settings.options.fireColorR,
                Settings.options.fireColorG,
                Settings.options.fireColorB);
        }

        private static void CleanExpiredEntries(GameObject fireObject)
        {
            if (!fireColorData.ContainsKey(fireObject)) return;

            try
            {
                int beforeCount = fireColorData[fireObject].Count;
                fireColorData[fireObject].RemoveAll(f => f.IsExpired);
                int afterCount = fireColorData[fireObject].Count;

                if (beforeCount != afterCount)
                {
                    //MelonLogger.Msg($"Cleaned expired fuel entries for {fireObject.name}: {beforeCount} -> {afterCount}");
                }

                if (fireColorData[fireObject].Count == 0)
                {
                    fireColorData.Remove(fireObject);
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error cleaning expired entries: {e.Message}");
            }
        }

        public static void CleanupFireData(GameObject fireObject)
        {
            if (fireObject != null)
            {
                if (fireColorData.ContainsKey(fireObject))
                {
                    fireColorData.Remove(fireObject);
                }

                if (trackedFireControllers.ContainsKey(fireObject))
                {
                    trackedFireControllers.Remove(fireObject);
                }

                if (fireSmokeProfiles.ContainsKey(fireObject))
                {
                    fireSmokeProfiles.Remove(fireObject);
                }
            }
        }

        public static void CleanupAll()
        {
            //MelonLogger.Msg("Cleaning up all fire color data");
            fireColorData.Clear();
            trackedFireControllers.Clear();
            fireSmokeProfiles.Clear();

            FireLightColorModifier.CleanupAll();
        }

        public static int GetActiveFuelCount(GameObject fireObject)
        {
            if (!fireColorData.ContainsKey(fireObject)) return 0;

            CleanExpiredEntries(fireObject);
            return fireColorData.ContainsKey(fireObject) ? fireColorData[fireObject].Count : 0;
        }

        public static void DebugFireColors(GameObject fireObject)
        {
            if (fireObject == null || !fireColorData.ContainsKey(fireObject))
            {
                return;
            }

            CleanExpiredEntries(fireObject);
            var activeFuels = fireColorData[fireObject].Where(f => !f.IsExpired).ToList();

            //MelonLogger.Msg($"=== Fire Color Debug for {fireObject.name} ===");
            //MelonLogger.Msg($"Total active fuels: {activeFuels.Count}");

            if (fireSmokeProfiles.ContainsKey(fireObject) && fireSmokeProfiles[fireObject].isValid)
            {
                var profile = fireSmokeProfiles[fireObject];
                var fuelName = GearItemData.GetFuelTypeName(profile.dominantFuelType);
                //MelonLogger.Msg($"Dominant smoke fuel: {fuelName}");
            }

            foreach (var fuel in activeFuels)
            {
                float timeSinceAdded = Time.time - fuel.timeAdded;
                float ageRatio = timeSinceAdded / fuel.duration;
                float timeRemaining = fuel.duration - timeSinceAdded;
                var fuelName = GearItemData.GetFuelTypeName(fuel.fuelType);

                //MelonLogger.Msg($"  Fuel: {fuelName}, Fire Color: R={fuel.color.r:F3} G={fuel.color.g:F3} B={fuel.color.b:F3}, Age: {timeSinceAdded:F1}s ({ageRatio:P1}), Time Remaining: {timeRemaining:F1}s");
            }

            //MelonLogger.Msg("=== End Fire Color Debug ===");
        }
    }
}