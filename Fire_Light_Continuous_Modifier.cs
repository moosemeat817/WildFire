using UnityEngine;
using MelonLoader;
using System.Collections.Generic;

namespace WildFire
{
    /// <summary>
    /// Manages fire light color overrides with smooth lerping/transitions
    /// Now smoothly transitions light colors to match fuel colors over time
    /// UPDATED: Applies light color offset to darken light colors compared to particle colors
    /// </summary>
    internal static class BlendLightColorSetLightColorPatch
    {
        private const float FUEL_COLOR_TRANSITION_SPEED = .5f; // Reduce to slow down

        private struct LerpingLightState
        {
            public Color currentColor;
            public Color targetColor;
            public float lerpDuration;
            public float lerpStartTime;
            public bool isLerping;
            public bool pendingUnregister; // Flag to unregister after lerp completes
        }

        private static Dictionary<int, Color> fireLightOverrides = new Dictionary<int, Color>();
        private static Dictionary<int, Light[]> cachedFireLights = new Dictionary<int, Light[]>();
        private static Dictionary<int, LerpingLightState> lightLerpStates = new Dictionary<int, LerpingLightState>();

        public static void RegisterFireLightOverride(GameObject fireObject, Color targetColor)
        {
            if (fireObject == null) return;

            int instanceId = fireObject.GetInstanceID();

            // Check if this is the baseline fire color BEFORE applying offset
            bool isBaseline = IsDefaultFireColor(targetColor);

            // FIXED: Only apply light color offset if NOT the baseline color
            // Baseline color should stay as-is to avoid snapping
            Color offsetTargetColor = isBaseline ? targetColor : FireLightColorOffset.ApplyLightColorOffset(targetColor);

            //MelonLogger.Msg($"[BlendLightColorPatch] RegisterFireLightOverride called");
            //MelonLogger.Msg($"  Original particle color: R={targetColor.r:F3} G={targetColor.g:F3} B={targetColor.b:F3}");
            //MelonLogger.Msg($"  Offset light color: R={offsetTargetColor.r:F3} G={offsetTargetColor.g:F3} B={offsetTargetColor.b:F3}");

            // Check if we're changing to a new color (includes returning to default)
            bool isNewColor = !fireLightOverrides.ContainsKey(instanceId);
            bool isColorChange = fireLightOverrides.ContainsKey(instanceId) &&
                                 !AreColorsEqual(fireLightOverrides[instanceId], offsetTargetColor);

            //MelonLogger.Msg($"[BlendLightColorPatch] isNewColor: {isNewColor}, isColorChange: {isColorChange}");

            fireLightOverrides[instanceId] = offsetTargetColor;

            // Clear any pending unregister flag since we're registering again
            if (lightLerpStates.ContainsKey(instanceId))
            {
                var state = lightLerpStates[instanceId];
                state.pendingUnregister = false;
                lightLerpStates[instanceId] = state;
                //MelonLogger.Msg($"[BlendLightColorPatch] Cleared pending unregister flag for fire (ID: {instanceId})");
            }

            // Cache the lights for this fire if not already cached
            if (!cachedFireLights.ContainsKey(instanceId))
            {
                CacheLightsForFire(fireObject, instanceId);
            }

            // If this is a new override or a color change, start lerping
            if (isNewColor || isColorChange)
            {
                //MelonLogger.Msg($"[BlendLightColorPatch] Starting lerp because: isNewColor={isNewColor}, isColorChange={isColorChange}");
                StartLerpingLights(fireObject, instanceId, offsetTargetColor);
            }
            else
            {
                //MelonLogger.Msg($"[BlendLightColorPatch] NOT starting lerp - color unchanged");
            }

            //MelonLogger.Msg($"[BlendLightColorPatch] Registered light override for fire (ID: {instanceId}): R={offsetTargetColor.r:F3} G={offsetTargetColor.g:F3} B={offsetTargetColor.b:F3}");
        }

        private static bool AreColorsEqual(Color a, Color b, float tolerance = 0.01f)
        {
            bool equal = Mathf.Abs(a.r - b.r) < tolerance &&
                         Mathf.Abs(a.g - b.g) < tolerance &&
                         Mathf.Abs(a.b - b.b) < tolerance &&
                         Mathf.Abs(a.a - b.a) < tolerance;

            if (!equal)
            {
                //MelonLogger.Msg($"[BlendLightColorPatch] Color change detected: ({a.r:F3}, {a.g:F3}, {a.b:F3}) -> ({b.r:F3}, {b.g:F3}, {b.b:F3})");
            }

            return equal;
        }

        private static void StartLerpingLights(GameObject fireObject, int instanceId, Color targetColor)
        {
            try
            {
                // Get current color from first light as starting point
                Color startColor = Color.white;
                if (cachedFireLights.ContainsKey(instanceId) && cachedFireLights[instanceId].Length > 0)
                {
                    var light = cachedFireLights[instanceId][0];
                    if (light != null)
                    {
                        startColor = light.color;
                    }
                }

                // Get transition speed from hardcoded constant
                float transitionSpeed = FUEL_COLOR_TRANSITION_SPEED;

                // Check if we're reverting to default fire color (orange-ish)
                bool isRevertingToDefault = IsDefaultFireColor(targetColor);

                // Calculate lerp duration (inverse of speed - higher speed = shorter duration)
                float lerpDuration = 1f / Mathf.Max(transitionSpeed, 0.1f);

                /*
                // If reverting to default, use a longer duration to match particle fade-out
                if (isRevertingToDefault)
                {
                    lerpDuration *= 2.5f;
                   // MelonLogger.Msg($"[BlendLightColorPatch] Detected revert to default fire color - extending lerp duration to {lerpDuration:F2}s");
                }
                */

                lightLerpStates[instanceId] = new LerpingLightState
                {
                    currentColor = startColor,
                    targetColor = targetColor,
                    lerpDuration = lerpDuration,
                    lerpStartTime = Time.time,
                    isLerping = true,
                    pendingUnregister = false
                };

                //MelonLogger.Msg($"[BlendLightColorPatch] Starting light color lerp for fire (ID: {instanceId}):");
                //MelonLogger.Msg($"  From: R={startColor.r:F3} G={startColor.g:F3} B={startColor.b:F3}");
                //MelonLogger.Msg($"  To: R={targetColor.r:F3} G={targetColor.g:F3} B={targetColor.b:F3}");
                //MelonLogger.Msg($"  Duration: {lerpDuration:F2}s (Speed: {transitionSpeed:F2}x)");
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"[BlendLightColorPatch] Error starting lerp: {e.Message}");
            }
        }

        private static bool IsDefaultFireColor(Color color)
        {
            // Check if this is approximately the default fire color (orange)
            // After offset, default is roughly R=0.49, G=0.062, B=0.007
            float tolerance = 0.02f;
            return Mathf.Abs(color.r - 0.49f) < tolerance &&
                   Mathf.Abs(color.g - 0.062f) < tolerance &&
                   Mathf.Abs(color.b - 0.007f) < tolerance;
        }

        private static void CacheLightsForFire(GameObject fireObject, int instanceId)
        {
            try
            {
                Transform fxLightingTransform = FindFXLightingTransform(fireObject.transform);
                if (fxLightingTransform != null)
                {
                    Light[] lights = fxLightingTransform.GetComponentsInChildren<Light>();
                    cachedFireLights[instanceId] = lights;

                    //MelonLogger.Msg($"[BlendLightColorPatch] Cached {lights.Length} lights for fire:");
                    for (int i = 0; i < lights.Length; i++)
                    {
                        if (lights[i] != null)
                        {
                            //MelonLogger.Msg($"  [{i}] {lights[i].gameObject.name} - Enabled: {lights[i].enabled}, Current Color: R={lights[i].color.r:F3} G={lights[i].color.g:F3} B={lights[i].color.b:F3}");
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"[BlendLightColorPatch] Error caching lights: {e.Message}");
            }
        }

        private static Transform FindFXLightingTransform(Transform parent)
        {
            if (parent == null) return null;

            try
            {
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

                // If not found, search recursively
                for (int i = 0; i < childCount; i++)
                {
                    Transform child = parent.GetChild(i);
                    if (child != null)
                    {
                        Transform result = FindFXLightingTransform(child);
                        if (result != null) return result;
                    }
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error finding FX_Lighting: {e.Message}");
            }

            return null;
        }

        public static void UpdateFireLightOverride(GameObject fireObject, Color newColor)
        {
            if (fireObject == null) return;

            int instanceId = fireObject.GetInstanceID();

            // Always update, even if it's returning to default - this ensures lerping happens
            RegisterFireLightOverride(fireObject, newColor);
        }

        public static void UnregisterFireLightOverride(GameObject fireObject)
        {
            if (fireObject == null) return;

            int instanceId = fireObject.GetInstanceID();

            // Check if there's an active lerp
            if (lightLerpStates.ContainsKey(instanceId) && lightLerpStates[instanceId].isLerping)
            {
                // Mark for unregister after lerp completes instead of blocking
                var state = lightLerpStates[instanceId];
                state.pendingUnregister = true;
                lightLerpStates[instanceId] = state;
                //MelonLogger.Msg($"[BlendLightColorPatch] Fire (ID: {instanceId}) marked for unregister after lerp completes.");
                return;
            }

            if (fireLightOverrides.ContainsKey(instanceId))
            {
                fireLightOverrides.Remove(instanceId);
                lightLerpStates.Remove(instanceId);
                cachedFireLights.Remove(instanceId);
                //MelonLogger.Msg($"[BlendLightColorPatch] Unregistered light override for fire (ID: {instanceId})");
            }
        }

        public static void ClearAllOverrides()
        {
            fireLightOverrides.Clear();
            cachedFireLights.Clear();
            lightLerpStates.Clear();
            //MelonLogger.Msg("[BlendLightColorPatch] Cleared all fire light overrides");
        }

        /// <summary>
        /// Call this from LateUpdate to apply and lerp light color overrides
        /// This ensures our colors are set and lerped every frame
        /// </summary>
        public static void ApplyAllOverrides()
        {
            try
            {
                if (fireLightOverrides.Count == 0)
                    return;

                foreach (var kvp in fireLightOverrides)
                {
                    int fireInstanceId = kvp.Key;
                    Color targetColor = kvp.Value;

                    if (cachedFireLights.ContainsKey(fireInstanceId))
                    {
                        var lights = cachedFireLights[fireInstanceId];
                        if (lights != null && lights.Length > 0)
                        {
                            // Get the current lerp state
                            Color colorToApply = targetColor;

                            if (lightLerpStates.ContainsKey(fireInstanceId))
                            {
                                var lerpState = lightLerpStates[fireInstanceId];

                                if (lerpState.isLerping)
                                {
                                    // Calculate lerp progress
                                    float elapsed = Time.time - lerpState.lerpStartTime;
                                    float progress = Mathf.Clamp01(elapsed / lerpState.lerpDuration);

                                    // Lerp between current and target
                                    colorToApply = Color.Lerp(lerpState.currentColor, lerpState.targetColor, progress);

                                    // Check if lerp is complete
                                    if (progress >= 1f)
                                    {
                                        colorToApply = lerpState.targetColor;
                                        lerpState.isLerping = false;

                                        // Check if this fire was marked for unregister
                                        if (lerpState.pendingUnregister)
                                        {
                                            //MelonLogger.Msg($"[BlendLightColorPatch] Lerp complete for fire (ID: {fireInstanceId}), performing pending unregister");
                                            // Remove from all tracking after this frame
                                            lightLerpStates.Remove(fireInstanceId);
                                            fireLightOverrides.Remove(fireInstanceId);
                                            cachedFireLights.Remove(fireInstanceId);
                                            continue;
                                        }

                                        lightLerpStates[fireInstanceId] = lerpState;
                                        //MelonLogger.Msg($"[BlendLightColorPatch] Light color lerp complete for fire (ID: {fireInstanceId})");
                                    }
                                    else
                                    {
                                        lightLerpStates[fireInstanceId] = lerpState;
                                    }
                                }
                                else
                                {
                                    colorToApply = lerpState.targetColor;
                                }
                            }

                            // Apply the color to all lights
                            int appliedCount = 0;
                            foreach (var light in lights)
                            {
                                if (light != null)
                                {
                                    try
                                    {
                                        light.color = colorToApply;
                                        appliedCount++;
                                    }
                                    catch (System.Exception e)
                                    {
                                        //MelonLogger.Warning($"[BlendLightColorPatch] Failed to set color on light: {e.Message}");
                                    }
                                }
                            }

                            // Log every 300 frames to avoid spam
                            if (Time.frameCount % 300 == 0)
                            {
                                //MelonLogger.Msg($"[BlendLightColorPatch] Applied override to {appliedCount}/{lights.Length} lights (LateUpdate): R={colorToApply.r:F3} G={colorToApply.g:F3} B={colorToApply.b:F3}");
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"[BlendLightColorPatch] Error applying overrides: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Wrapper class for backwards compatibility
    /// </summary>
    internal static class FireLightOverrideManager
    {
        public static void Initialize()
        {
            //MelonLogger.Msg("Initialized FireLightOverrideManager (v5 - With Light Color Offset)");
        }

        public static void RegisterFireLightOverride(GameObject fireObject, Color targetColor)
        {
            BlendLightColorSetLightColorPatch.RegisterFireLightOverride(fireObject, targetColor);
        }

        public static void UpdateFireLightOverride(GameObject fireObject, Color newColor)
        {
            BlendLightColorSetLightColorPatch.UpdateFireLightOverride(fireObject, newColor);
        }

        public static void UnregisterFireLightOverride(GameObject fireObject)
        {
            BlendLightColorSetLightColorPatch.UnregisterFireLightOverride(fireObject);
        }

        public static void ClearAllOverrides()
        {
            BlendLightColorSetLightColorPatch.ClearAllOverrides();
        }

        public static void Cleanup()
        {
            ClearAllOverrides();
        }

        /// <summary>
        /// Call this from Main.cs OnLateUpdate
        /// </summary>
        public static void UpdateLightColors()
        {
            BlendLightColorSetLightColorPatch.ApplyAllOverrides();
        }
    }
}