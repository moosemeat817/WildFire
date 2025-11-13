using UnityEngine;
using MelonLoader;

namespace WildFire
{
    /// <summary>
    /// Main coordinator for particle system modifications
    /// Now delegates to specialized modifiers for better organization and maintainability
    /// UPDATED: Smoke baseline state is always applied (removed enableSmokeModifications check)
    /// </summary>
    internal static class ParticleSystemModifier
    {
        public static void ApplyModifications(ParticleSystem particleSystem, FireType fireType, FireStage stage, GameObject fireObject)
        {
            if (particleSystem == null) return;

            try
            {
                // Add additional safety checks for IL2CPP
                if (particleSystem.gameObject == null || !particleSystem.gameObject.activeInHierarchy)
                    return;

                //MelonLogger.Msg($"Applying modifications to particle system: {particleSystem.name}");

                // Determine what type of particle system this is and apply appropriate modifications
                bool isSparkEffect = SparkEffectsModifier.IsSparkParticleSystem(particleSystem);
                bool isSmokeEffect = SmokeEffectsModifier.IsSmokeParticleSystem(particleSystem);

                if (isSparkEffect)
                {
                    //MelonLogger.Msg($"Identified spark particle system: {particleSystem.name}");

                    // CRITICAL CHECK: Skip spark modifications for specific fire types
                    if (FireUtils.ShouldSkipSparkModifications(fireObject))
                    {
                        //MelonLogger.Msg($"Skipping spark modifications for excluded fire type: {particleSystem.name}");
                    }
                    else
                    {
                        SparkEffectsModifier.ApplySparkModifications(particleSystem, fireType, fireObject);
                    }
                }
                else if (isSmokeEffect)
                {
                    //MelonLogger.Msg($"Identified smoke particle system: {particleSystem.name}");

                    // CRITICAL CHECK: Skip smoke modifications entirely for 6-burner stove
                    if (FireUtils.IsSixBurnerStove(fireObject))
                    {
                        //MelonLogger.Msg($"Skipping smoke modifications for 6-burner stove (INTERACTIVE_StoveMetalA): {particleSystem.name}");
                        return;
                    }

                    // Apply smoke baseline state (always, not just when setting is enabled)
                    SmokeEffectsModifier.ApplySmokeModifications(particleSystem, fireType, fireObject);
                }
                else
                {
                    // Apply normal fire modifications - only color modifications now
                    //MelonLogger.Msg($"Applying fire color modifications to: {particleSystem.name}");
                    FireColorIntensityModifier.ApplyColorModifications(particleSystem, fireType, stage, fireObject);
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error applying particle system modifications to {particleSystem?.name}: {e.Message}");
            }
        }

        // Method to identify particle system type for debugging
        public static string GetParticleSystemType(ParticleSystem ps)
        {
            if (ps == null) return "Unknown";

            if (SparkEffectsModifier.IsSparkParticleSystem(ps))
                return "Spark";

            if (SmokeEffectsModifier.IsSmokeParticleSystem(ps))
                return "Smoke";

            return "Fire";
        }

        // Debugging method to log particle system info
        public static void LogParticleSystemInfo(ParticleSystem ps, string context = "")
        {
            if (ps == null) return;

            try
            {
                string psType = GetParticleSystemType(ps);
                //MelonLogger.Msg($"[Particle System Info{(string.IsNullOrEmpty(context) ? "" : $" - {context}")}]");
                //MelonLogger.Msg($"  Name: {ps.name}");
                //MelonLogger.Msg($"  Type: {psType}");
                //MelonLogger.Msg($"  Active: {ps.gameObject.activeInHierarchy}");
                //MelonLogger.Msg($"  Playing: {ps.isPlaying}");
                //MelonLogger.Msg($"  Emission Enabled: {ps.emission.enabled}");

                try
                {
                    //MelonLogger.Msg($"  Emission Rate: {ps.emission.rateOverTime.constant:F1}");
                    //MelonLogger.Msg($"  Max Particles: {ps.main.maxParticles}");
                    //MelonLogger.Msg($"  Start Lifetime: {ps.main.startLifetime.constant:F2}s");
                    //MelonLogger.Msg($"  Start Size: {ps.main.startSize.constant:F2}");
                    //MelonLogger.Msg($"  Start Speed: {ps.main.startSpeed.constant:F2}");
                }
                catch (System.Exception e)
                {
                    //MelonLogger.Warning($"  Could not read detailed particle system properties: {e.Message}");
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error logging particle system info: {e.Message}");
            }
        }
    }
}