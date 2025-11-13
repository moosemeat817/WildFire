using UnityEngine;
using MelonLoader;
using System.Collections.Generic;

namespace WildFire
{
    /// <summary>
    /// Modifies Light.color on all Light components under FX_Lighting to match fuel colors
    /// Targets the hierarchy: Fire/FX_Lighting/[Light components]
    /// FIXED: IL2CPP compatibility - proper casting for Transform iteration
    /// </summary>
    internal static class FireLightColorModifier
    {
        // Store original light colors keyed by Light instance ID
        private static Dictionary<int, Color> originalLightColors = new Dictionary<int, Color>();

        /// <summary>
        /// Apply fuel color to all lights in the FX_Lighting hierarchy
        /// </summary>
        public static void ApplyFuelColorToFireLights(GameObject fireObject, Color fuelColor)
        {
            if (fireObject == null)
            {
                //MelonLogger.Warning("ApplyFuelColorToFireLights: fireObject is null");
                return;
            }

            try
            {
                // Find the FX_Lighting child object
                Transform fxLightingTransform = FindFXLightingTransform(fireObject.transform);

                if (fxLightingTransform == null)
                {
                    //MelonLogger.Warning($"Could not find FX_Lighting under fire object: {fireObject.name}");
                    return;
                }

                //MelonLogger.Msg($"Found FX_Lighting at: {GetFullPath(fxLightingTransform)}");

                // Get all Light components under FX_Lighting (including children)
                Light[] lights = fxLightingTransform.GetComponentsInChildren<Light>();

                if (lights == null || lights.Length == 0)
                {
                    //MelonLogger.Warning($"No Light components found under FX_Lighting for {fireObject.name}");
                    return;
                }

                //MelonLogger.Msg($"Found {lights.Length} Light components under FX_Lighting");

                // Apply color to each light
                foreach (Light light in lights)
                {
                    if (light != null)
                    {
                        ApplyColorToLight(light, fuelColor);
                    }
                }

                //MelonLogger.Msg($"Successfully applied fuel color to {lights.Length} lights");
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error applying fuel color to fire lights: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// Apply color to a single Light component
        /// </summary>
        private static void ApplyColorToLight(Light light, Color fuelColor)
        {
            try
            {
                int instanceId = light.GetInstanceID();

                // Store original color if not already stored
                if (!originalLightColors.ContainsKey(instanceId))
                {
                    originalLightColors[instanceId] = light.color;
                    //MelonLogger.Msg($"Stored original color for '{light.gameObject.name}': " +
                                   //$"R={light.color.r:F3} G={light.color.g:F3} B={light.color.b:F3} A={light.color.a:F3}");
                }

                // Apply the fuel color while preserving alpha
                Color newColor = new Color(fuelColor.r, fuelColor.g, fuelColor.b, light.color.a);
                light.color = newColor;

                //MelonLogger.Msg($"Applied color to '{light.gameObject.name}': " +
                               //$"R={newColor.r:F3} G={newColor.g:F3} B={newColor.b:F3} A={newColor.a:F3}");
            }
            catch (System.Exception e)
            {
                //MelonLogger.Error($"Error applying color to individual light: {e.Message}");
            }
        }

        /// <summary>
        /// Restore original light colors when fuel burns out
        /// </summary>
        public static void RestoreFireLightColors(GameObject fireObject)
        {
            if (fireObject == null)
            {
                return;
            }

            try
            {
                Transform fxLightingTransform = FindFXLightingTransform(fireObject.transform);

                if (fxLightingTransform == null)
                {
                    return;
                }

                Light[] lights = fxLightingTransform.GetComponentsInChildren<Light>();

                foreach (Light light in lights)
                {
                    if (light != null)
                    {
                        int instanceId = light.GetInstanceID();

                        if (originalLightColors.ContainsKey(instanceId))
                        {
                            Color originalColor = originalLightColors[instanceId];
                            light.color = originalColor;

                            //MelonLogger.Msg($"Restored original color for '{light.gameObject.name}': " +
                                           //$"R={originalColor.r:F3} G={originalColor.g:F3} B={originalColor.b:F3}");

                            originalLightColors.Remove(instanceId);
                        }
                    }
                }

                //MelonLogger.Msg($"Restored light colors for fire: {fireObject.name}");
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error restoring fire light colors: {e.Message}");
            }
        }

        /// <summary>
        /// Find the FX_Lighting transform under the fire object
        /// Searches for child named "FX_Lighting" (case-insensitive)
        /// FIXED: IL2CPP compatible iteration using GetChild() instead of foreach
        /// </summary>
        private static Transform FindFXLightingTransform(Transform parent)
        {
            if (parent == null)
            {
                return null;
            }

            try
            {
                // FIXED: IL2CPP compatible way to iterate children
                // Use GetChildCount() and GetChild(index) instead of foreach
                int childCount = parent.childCount;

                // Check direct children first
                for (int i = 0; i < childCount; i++)
                {
                    Transform child = parent.GetChild(i);
                    if (child != null && child.name.Equals("FX_Lighting", System.StringComparison.OrdinalIgnoreCase))
                    {
                        return child;
                    }
                }

                // If not found in direct children, search recursively
                for (int i = 0; i < childCount; i++)
                {
                    Transform child = parent.GetChild(i);
                    if (child != null)
                    {
                        Transform result = FindFXLightingTransform(child);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error finding FX_Lighting transform: {e.Message}");
            }

            return null;
        }

        /// <summary>
        /// Get full hierarchy path for debugging
        /// </summary>
        private static string GetFullPath(Transform transform)
        {
            if (transform == null)
            {
                return "null";
            }

            string path = transform.name;
            Transform current = transform.parent;

            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        /// <summary>
        /// Clean up all stored data
        /// </summary>
        public static void CleanupAll()
        {
            //MelonLogger.Msg($"Cleaning up {originalLightColors.Count} stored light colors");
            originalLightColors.Clear();
        }

        /// <summary>
        /// Debug: List all lights under FX_Lighting for a fire
        /// </summary>
        public static void DebugListLights(GameObject fireObject)
        {
            if (fireObject == null)
            {
                //MelonLogger.Msg("DebugListLights: fireObject is null");
                return;
            }

            try
            {
                //MelonLogger.Msg($"=== Debugging Lights for {fireObject.name} ===");

                Transform fxLightingTransform = FindFXLightingTransform(fireObject.transform);

                if (fxLightingTransform == null)
                {
                    MelonLogger.Msg("FX_Lighting not found");
                    return;
                }

                //MelonLogger.Msg($"FX_Lighting found at: {GetFullPath(fxLightingTransform)}");

                Light[] lights = fxLightingTransform.GetComponentsInChildren<Light>();

                //MelonLogger.Msg($"Found {lights.Length} Light components:");

                foreach (Light light in lights)
                {
                    if (light != null)
                    {
                        //MelonLogger.Msg($"  - {light.gameObject.name}:");
                        //MelonLogger.Msg($"    Path: {GetFullPath(light.transform)}");
                        //MelonLogger.Msg($"    Enabled: {light.enabled}");
                        //MelonLogger.Msg($"    Color: R={light.color.r:F3} G={light.color.g:F3} B={light.color.b:F3} A={light.color.a:F3}");
                        //MelonLogger.Msg($"    Intensity: {light.intensity:F2}");
                        //MelonLogger.Msg($"    Type: {light.type}");
                    }
                }

                //MelonLogger.Msg("=== End Debug ===");
            }
            catch (System.Exception e)
            {
                //MelonLogger.Error($"Error debugging lights: {e.Message}");
            }
        }
    }
}