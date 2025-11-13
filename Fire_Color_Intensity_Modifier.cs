using UnityEngine;
using MelonLoader;

namespace WildFire
{
    /// <summary>
    /// Handles fire color modifications for particle systems
    /// SIMPLIFIED: Just apply the target color
    /// </summary>
    internal static class FireColorIntensityModifier
    {
        public static void ApplyColorModifications(ParticleSystem ps, FireType fireType, FireStage stage, GameObject fireObject)
        {
            if (ps == null) return;

            try
            {
                var main = ps.main;
                Color targetColor;

                // Check if we have fuel tracked on this fire
                bool fireObjectExists = fireObject != null;
                bool hasFuelTracked = fireObjectExists && FuelColorTracker.GetActiveFuelCount(fireObject) > 0;

                // If fuel exists on fire, use fuel color; otherwise use default fire type color
                if (hasFuelTracked)
                {
                    targetColor = FuelColorTracker.GetCurrentFireColor(fireObject, fireType);
                }
                else
                {
                    targetColor = GetColorForFireType(fireType);
                }

                // Just apply the color directly
                main.startColor = targetColor;
            }
            catch (System.Exception e)
            {
                //MelonLogger.Warning($"Could not apply color modifications: {e.Message}");
            }
        }

        public static void UpdateAllParticleColorLerps()
        {
            // No-op - not needed
        }

        private static Color GetColorForFireType(FireType fireType)
        {
            return FireUtils.RGBToColor(
                Settings.options.fireColorR,
                Settings.options.fireColorG,
                Settings.options.fireColorB);
        }

        public static void CleanupParticleColors()
        {
            // No-op
        }
    }
}