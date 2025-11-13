using UnityEngine;
using MelonLoader;
using System.Collections.Generic;

namespace WildFire
{
    /// 
    /// Dedicated handler for smoke effect modifications
    /// COMPLETE REDESIGN: Smoke multipliers now define the baseline state (like fire color settings)
    /// Stores TRUE original values on first detection, then uses those for all calculations
    /// UPDATED: Added public ApplySmokeBaselineDirectly for smoke dominance tracking
    /// FIXED: Now uses smoke profile instead of most recent fuel to determine settings
    /// 
    internal static class SmokeEffectsModifier
    {
        // Track which particle systems have been processed to avoid double-application
        private static HashSet<ParticleSystem> processedSmokeSystems = new HashSet<ParticleSystem>();

        // Store TRUE original unmodified values captured on first encounter
        private static Dictionary<int, OriginalSmokeValues> trueOriginalSmokeValues = new Dictionary<int, OriginalSmokeValues>();

        private struct OriginalSmokeValues
        {
            public float trueOriginalEmissionRate;
            public float trueOriginalLifetime;
            public float trueOriginalSize;
            public float trueOriginalSpeed;
            public Color trueOriginalColor;
            public bool isInitialized;
        }


        /// <summary>
        /// Clear the processing tracking to allow a new refresh cycle
        /// Call this at the start of RefreshAllFireEffects
        /// </summary>
        public static void StartNewRefreshCycle()
        {
            processedSmokeSystems.Clear();
        }


        public static void ApplySmokeModifications(ParticleSystem particleSystem, FireType fireType, GameObject fireObject)
        {
            if (particleSystem == null) return;

            try
            {
                if (particleSystem.gameObject == null || !particleSystem.gameObject.activeInHierarchy)
                    return;

                // CRITICAL CHECK: Skip smoke modifications entirely for 6-burner stove
                if (FireUtils.IsSixBurnerStove(fireObject))
                {
                    //MelonLogger.Msg($"Skipping smoke modifications for 6-burner stove: {particleSystem.name}");
                    return;
                }

                int instanceId = particleSystem.GetInstanceID();

                // Capture TRUE original values ONLY on first encounter
                // This ensures we always have the unmodified game defaults
                if (!trueOriginalSmokeValues.ContainsKey(instanceId))
                {
                    CaptureAndStoreOriginalValues(particleSystem);
                }

                // Check if already processed to prevent double-application in same refresh cycle
                if (processedSmokeSystems.Contains(particleSystem))
                {
                    //MelonLogger.Msg($"Smoke system {particleSystem.name} already processed this cycle, skipping");
                    return;
                }

                //MelonLogger.Msg($"Applying smoke baseline state to: {particleSystem.name}");

                // Get the multipliers that define this smoke's baseline appearance
                float densityMult = Settings.options.smokeDensityMultiplier;
                float lifetimeMult = Settings.options.smokeLifetimeMultiplier;
                float sizeMult = Settings.options.smokeSizeMultiplier;
                float speedMult = Settings.options.smokeSpeedMultiplier;
                float opacityMult = Settings.options.smokeOpacityMultiplier;

                // FIXED: Check the smoke PROFILE first (which tracks the dominant smoke fuel)
                // This ensures we use the thickest smoke settings, not just the most recent fuel
                var (hasProfile, profileSettings) = FuelColorTracker.GetCurrentSmokeSettings(fireObject);

                if (hasProfile)
                {
                    // Use the dominant smoke profile settings
                    //MelonLogger.Msg($"Using smoke profile settings (dominant fuel)");
                    densityMult = profileSettings.densityMultiplier;
                    lifetimeMult = profileSettings.lifetimeMultiplier;
                    sizeMult = profileSettings.sizeMultiplier;
                    speedMult = profileSettings.speedMultiplier;
                    opacityMult = profileSettings.opacityMultiplier;
                }
                else
                {
                    // Fallback: Check for most recent fuel-specific settings only if no profile exists
                    CustomFuelType recentFuelType = FuelColorTracker.GetMostRecentFuelType(fireObject);
                    if (recentFuelType != CustomFuelType.None && GearItemData.HasCustomSmokeSettings(recentFuelType))
                    {
                        var smokeSettings = GearItemData.GetSmokeSettings(recentFuelType);
                        var fuelName = GearItemData.GetFuelTypeName(recentFuelType);
                        //MelonLogger.Msg($"Using HARDCODED smoke baseline settings for {fuelName}");

                        densityMult = smokeSettings.densityMultiplier;
                        lifetimeMult = smokeSettings.lifetimeMultiplier;
                        sizeMult = smokeSettings.sizeMultiplier;
                        speedMult = smokeSettings.speedMultiplier;
                        opacityMult = smokeSettings.opacityMultiplier;
                    }
                }

                //MelonLogger.Msg($"Smoke baseline settings - Density: {densityMult:F1}x, Lifetime: {lifetimeMult:F1}x, Size: {sizeMult:F1}x, Speed: {speedMult:F1}x, Opacity: {opacityMult:F1}x");

                // Apply baseline state directly (not as modifications)
                ApplySmokeBaselineState(particleSystem, densityMult, lifetimeMult, sizeMult, speedMult, opacityMult);

                // Mark as processed this cycle
                processedSmokeSystems.Add(particleSystem);

                //MelonLogger.Msg($"Successfully applied smoke baseline state to: {particleSystem.name}");
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error applying smoke baseline state to {particleSystem?.name}: {e.Message}");
            }
        }

        /// <summary>
        /// NEW: Public method to apply smoke settings directly from a GearSmokeSettings struct
        /// Called by FuelColorTracker when smoke profile changes
        /// </summary>
        public static void ApplySmokeBaselineDirectly(ParticleSystem ps, GearSmokeSettings smokeSettings)
        {
            if (ps == null) return;

            try
            {
                if (ps.gameObject == null || !ps.gameObject.activeInHierarchy)
                    return;

                // Capture TRUE original values on first encounter if not already done
                int instanceId = ps.GetInstanceID();
                if (!trueOriginalSmokeValues.ContainsKey(instanceId))
                {
                    CaptureAndStoreOriginalValues(ps);
                }

                //MelonLogger.Msg($"Applying smoke baseline state directly to: {ps.name}");

                ApplySmokeBaselineState(ps,
                    smokeSettings.densityMultiplier,
                    smokeSettings.lifetimeMultiplier,
                    smokeSettings.sizeMultiplier,
                    smokeSettings.speedMultiplier,
                    smokeSettings.opacityMultiplier);
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error applying smoke baseline directly: {e.Message}");
            }
        }

        /// 
        /// Apply baseline smoke state directly based on the multiplier settings
        /// These multipliers define what the smoke should look like, not modifications to apply
        /// Uses TRUE original values stored on first capture
        /// 
        private static void ApplySmokeBaselineState(ParticleSystem ps, float densityMult, float lifetimeMult, float sizeMult, float speedMult, float opacityMult)
        {
            try
            {
                var main = ps.main;
                var emission = ps.emission;

                // Set emission rate to baseline (multiplier defines the baseline amount)
                TrySetSmokeEmissionRate(ps, emission, densityMult);

                // Set lifetime to baseline
                TrySetSmokeLifetime(ps, main, lifetimeMult);

                // Set size to baseline
                TrySetSmokeSize(ps, main, sizeMult);

                // Set opacity to baseline
                TrySetSmokeOpacity(ps, main, opacityMult);

                // Only apply conservative speed modifications to preserve physics
                if (speedMult >= 0.5f && speedMult <= 1.5f)
                {
                    TrySetSmokeSpeedConservative(ps, main, speedMult);
                }
                else
                {
                    //MelonLogger.Msg($"Skipping speed modification to preserve natural smoke physics (multiplier: {speedMult:F2}x)");
                }

                //MelonLogger.Msg($"Smoke baseline state applied successfully");
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error applying smoke baseline state: {e.Message}");
            }
        }

        /// 
        /// Restore smoke effects to the default baseline state when fuel burns off
        /// Since multipliers now define the baseline, we just reapply them
        /// 
        public static void RestoreSmokeModifications(ParticleSystem particleSystem)
        {
            if (particleSystem == null) return;

            try
            {
                if (particleSystem.gameObject == null || !particleSystem.gameObject.activeInHierarchy)
                    return;

                int instanceId = particleSystem.GetInstanceID();

                if (!processedSmokeSystems.Contains(particleSystem))
                {
                    //MelonLogger.Msg($"Smoke system {particleSystem.name} was not processed, skipping restore");
                    return;
                }

                //MelonLogger.Msg($"Restoring smoke to default baseline state: {particleSystem.name}");

                // Reapply the baseline state (which is now the default)
                float densityMult = Settings.options.smokeDensityMultiplier;
                float lifetimeMult = Settings.options.smokeLifetimeMultiplier;
                float sizeMult = Settings.options.smokeSizeMultiplier;
                float speedMult = Settings.options.smokeSpeedMultiplier;
                float opacityMult = Settings.options.smokeOpacityMultiplier;

                ApplySmokeBaselineState(particleSystem, densityMult, lifetimeMult, sizeMult, speedMult, opacityMult);

                //MelonLogger.Msg($"Restored smoke to default baseline state: {particleSystem.name}");
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error restoring smoke baseline state: {e.Message}");
            }
        }

        /// 
        /// Capture and store TRUE original values on first encounter with a particle system
        /// This is called before ANY modifications, so values are guaranteed to be unmodified
        /// 
        private static void CaptureAndStoreOriginalValues(ParticleSystem ps)
        {
            try
            {
                int instanceId = ps.GetInstanceID();

                // Only capture on first encounter - these are TRUE originals that never change
                if (trueOriginalSmokeValues.ContainsKey(instanceId))
                {
                    return;
                }

                var values = new OriginalSmokeValues();

                try
                {
                    values.trueOriginalEmissionRate = ps.emission.rateOverTime.constant;
                    //MelonLogger.Msg($"[Original] Captured emission rate: {values.trueOriginalEmissionRate:F1}");
                }
                catch
                {
                    values.trueOriginalEmissionRate = 5.0f;
                }

                try
                {
                    values.trueOriginalLifetime = ps.main.startLifetime.constant;
                    //MelonLogger.Msg($"[Original] Captured lifetime: {values.trueOriginalLifetime:F1}s");
                }
                catch
                {
                    values.trueOriginalLifetime = 4.0f;
                }

                try
                {
                    values.trueOriginalSize = ps.main.startSize.constant;
                    //MelonLogger.Msg($"[Original] Captured size: {values.trueOriginalSize:F2}");
                }
                catch
                {
                    values.trueOriginalSize = 2.0f;
                }

                try
                {
                    values.trueOriginalSpeed = ps.main.startSpeed.constant;
                    //MelonLogger.Msg($"[Original] Captured speed: {values.trueOriginalSpeed:F1}");
                }
                catch
                {
                    values.trueOriginalSpeed = 2.0f;
                }

                try
                {
                    values.trueOriginalColor = ps.main.startColor.color;
                    //MelonLogger.Msg($"[Original] Captured color: R={values.trueOriginalColor.r:F2}, G={values.trueOriginalColor.g:F2}, B={values.trueOriginalColor.b:F2}, A={values.trueOriginalColor.a:F2}");
                }
                catch
                {
                    values.trueOriginalColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
                }

                values.isInitialized = true;
                trueOriginalSmokeValues[instanceId] = values;

                //MelonLogger.Msg($"[CRITICAL] Stored TRUE original smoke values for: {ps.name}");
            }
            catch (System.Exception e)
            {
                //MelonLogger.Warning($"Could not capture original values for {ps.name}: {e.Message}");
            }
        }

        /// 
        /// Get TRUE original values that were captured on first encounter
        /// If not available, return reasonable defaults
        /// 
        private static OriginalSmokeValues GetTrueOriginalValues(ParticleSystem ps)
        {
            int instanceId = ps.GetInstanceID();
            if (trueOriginalSmokeValues.ContainsKey(instanceId))
            {
                return trueOriginalSmokeValues[instanceId];
            }

            return new OriginalSmokeValues
            {
                trueOriginalEmissionRate = 5.0f,
                trueOriginalLifetime = 4.0f,
                trueOriginalSize = 2.0f,
                trueOriginalSpeed = 2.0f,
                trueOriginalColor = new Color(0.5f, 0.5f, 0.5f, 0.8f),
                isInitialized = false
            };
        }

        public static bool IsSmokeParticleSystem(ParticleSystem ps)
        {
            if (ps?.gameObject?.name == null) return false;

            string name = ps.gameObject.name.ToLower();

            return name.Contains("smoke") ||
                   name.Contains("vapor") ||
                   name.Contains("steam") ||
                   name.Contains("fog") ||
                   name.Contains("mist") ||
                   (name.Contains("particle") && name.Contains("grey")) ||
                   (name.Contains("particle") && name.Contains("gray"));
        }

        private static void TrySetSmokeEmissionRate(ParticleSystem ps, ParticleSystem.EmissionModule emission, float densityMultiplier)
        {
            try
            {
                var originalValues = GetTrueOriginalValues(ps);
                float targetEmissionRate = originalValues.trueOriginalEmissionRate * densityMultiplier;

                emission.rateOverTime = Mathf.Max(0f, targetEmissionRate);

                //MelonLogger.Msg($"Set smoke emission rate baseline: {targetEmissionRate:F2} (true original: {originalValues.trueOriginalEmissionRate:F1}, multiplier: {densityMultiplier:F1}x)");
            }
            catch (System.Exception e)
            {
                //MelonLogger.Warning($"Could not set smoke emission rate: {e.Message}");
            }
        }

        private static void TrySetSmokeLifetime(ParticleSystem ps, ParticleSystem.MainModule main, float lifetimeMultiplier)
        {
            try
            {
                var originalValues = GetTrueOriginalValues(ps);
                float targetLifetime = originalValues.trueOriginalLifetime * lifetimeMultiplier;

                main.startLifetime = Mathf.Max(0.5f, targetLifetime);

                //MelonLogger.Msg($"Set smoke lifetime baseline: {targetLifetime:F2}s (true original: {originalValues.trueOriginalLifetime:F1}s, multiplier: {lifetimeMultiplier:F1}x)");
            }
            catch (System.Exception e)
            {
                //MelonLogger.Warning($"Could not set smoke lifetime: {e.Message}");
            }
        }

        private static void TrySetSmokeSize(ParticleSystem ps, ParticleSystem.MainModule main, float sizeMultiplier)
        {
            try
            {
                var originalValues = GetTrueOriginalValues(ps);
                float targetSize = originalValues.trueOriginalSize * sizeMultiplier;

                main.startSize = Mathf.Max(0.1f, targetSize);

                //MelonLogger.Msg($"Set smoke size baseline: {targetSize:F2} (true original: {originalValues.trueOriginalSize:F2}, multiplier: {sizeMultiplier:F1}x)");
            }
            catch (System.Exception e)
            {
                //MelonLogger.Warning($"Could not set smoke size: {e.Message}");
            }
        }

        private static void TrySetSmokeOpacity(ParticleSystem ps, ParticleSystem.MainModule main, float opacityMultiplier)
        {
            try
            {
                var originalValues = GetTrueOriginalValues(ps);
                Color targetColor = originalValues.trueOriginalColor;
                targetColor.a = Mathf.Clamp01(originalValues.trueOriginalColor.a * opacityMultiplier);

                main.startColor = targetColor;

                //MelonLogger.Msg($"Set smoke opacity baseline: {targetColor.a:F2} (true original: {originalValues.trueOriginalColor.a:F2}, multiplier: {opacityMultiplier:F1}x)");
            }
            catch (System.Exception e)
            {
                //MelonLogger.Warning($"Could not set smoke opacity: {e.Message}");
            }
        }

        private static void TrySetSmokeSpeedConservative(ParticleSystem ps, ParticleSystem.MainModule main, float speedMultiplier)
        {
            try
            {
                var originalValues = GetTrueOriginalValues(ps);
                float conservativeMultiplier = Mathf.Clamp(speedMultiplier, 0.8f, 1.2f);
                float targetSpeed = originalValues.trueOriginalSpeed * conservativeMultiplier;

                targetSpeed = Mathf.Clamp(targetSpeed, 1.0f, 4.0f);

                main.startSpeed = targetSpeed;

                //MelonLogger.Msg($"Set smoke speed baseline (conservative): {targetSpeed:F2} (true original: {originalValues.trueOriginalSpeed:F1}, conservative multiplier: {conservativeMultiplier:F1}x)");

                if (Mathf.Abs(conservativeMultiplier - speedMultiplier) > 0.001f)
                {
                    //MelonLogger.Msg($"Speed multiplier clamped for physics preservation: {speedMultiplier:F1}x ? {conservativeMultiplier:F1}x");
                }
            }
            catch (System.Exception e)
            {
                //MelonLogger.Warning($"Could not set conservative smoke speed: {e.Message}");
            }
        }

        public static void ClearModificationTracking()
        {
            processedSmokeSystems.Clear();
            //MelonLogger.Msg("Cleared smoke modification tracking and TRUE original values");
        }

        public static void ClearModificationTrackingFor(ParticleSystem ps)
        {
            if (ps != null)
            {
                int instanceId = ps.GetInstanceID();

                // Remove from processed tracking so it can be reprocessed with new fuel
                if (processedSmokeSystems.Contains(ps))
                {
                    processedSmokeSystems.Remove(ps);
                    //MelonLogger.Msg($"Removed {ps.name} from processing tracking");
                }
            }
        }

        public static void CleanupOriginalValues()
        {
            var keysToRemove = new List<int>();

            foreach (var kvp in trueOriginalSmokeValues)
            {
                bool exists = false;
                try
                {
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
                trueOriginalSmokeValues.Remove(key);
            }

            if (keysToRemove.Count > 0)
            {
                //MelonLogger.Msg($"Cleaned up {keysToRemove.Count} orphaned smoke TRUE original value entries");
            }
        }
    }
}