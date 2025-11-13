using UnityEngine;
using MelonLoader;

namespace WildFire
{
    internal static class FireUtils
    {
        /// <summary>
        /// Safely gets a component from a GameObject with null checking
        /// </summary>
        public static T SafeGetComponent<T>(GameObject obj) where T : Component
        {
            if (obj == null) return null;

            try
            {
                return obj.GetComponent<T>();
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error getting component {typeof(T).Name}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Safely gets a component from a GameObject or its parents
        /// </summary>
        public static T SafeGetComponentInParent<T>(GameObject obj) where T : Component
        {
            if (obj == null) return null;

            try
            {
                return obj.GetComponentInParent<T>();
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error getting component {typeof(T).Name} in parent: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Check if a fire object is the 6-burner stove (INTERACTIVE_StoveMetalA)
        /// Returns true if this is the 6-burner stove that should skip smoke modifications
        /// </summary>
        public static bool IsSixBurnerStove(GameObject fireObject)
        {
            if (fireObject == null) return false;

            try
            {
                // Check the fire object itself
                if (fireObject.name.Contains("INTERACTIVE_StoveMetalA"))
                {
                    //MelonLogger.Msg($"Detected 6-burner stove (self): {fireObject.name}");
                    return true;
                }

                // Check all parent objects up the hierarchy
                Transform current = fireObject.transform.parent;
                while (current != null)
                {
                    if (current.gameObject.name.Contains("INTERACTIVE_StoveMetalA"))
                    {
                        //MelonLogger.Msg($"Detected 6-burner stove (parent): {current.gameObject.name}");
                        return true;
                    }
                    current = current.parent;
                }

                return false;
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error checking for 6-burner stove: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if a fire object should skip spark modifications
        /// Returns true for: Wood Stove C, Pot Belly Stove, Ammo Workbench, 6-Burner Stove
        /// </summary>
        public static bool ShouldSkipSparkModifications(GameObject fireObject)
        {
            if (fireObject == null) return false;

            try
            {
                // List of fire types that should skip spark modifications
                string[] skipSparkTypes = new string[]
                {
                    "INTERACTIVE_StoveWoodC",
                    "INTERACTIVE_PotBellyStove",
                    "INTERACTIVE_AmmoWorkBench",
                    "INTERACTIVE_StoveMetalA",
                    "INTERACTIVE_Forge"
                };

                // Check the fire object itself
                foreach (string skipType in skipSparkTypes)
                {
                    if (fireObject.name.Contains(skipType))
                    {
                        //MelonLogger.Msg($"Detected fire type that skips spark modifications (self): {fireObject.name}");
                        return true;
                    }
                }

                // Check all parent objects up the hierarchy
                Transform current = fireObject.transform.parent;
                while (current != null)
                {
                    foreach (string skipType in skipSparkTypes)
                    {
                        if (current.gameObject.name.Contains(skipType))
                        {
                            //MelonLogger.Msg($"Detected fire type that skips spark modifications (parent): {current.gameObject.name}");
                            return true;
                        }
                    }
                    current = current.parent;
                }

                return false;
            }
            catch (System.Exception e)
            {
                //MelonLogger.Error($"Error checking if should skip spark modifications: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clamps a color value to valid ranges
        /// </summary>
        public static Color ClampColor(Color color)
        {
            return new Color(
                Mathf.Clamp01(color.r),
                Mathf.Clamp01(color.g),
                Mathf.Clamp01(color.b),
                Mathf.Clamp01(color.a)
            );
        }

        /// <summary>
        /// Converts RGB values (0-255) to Unity Color (0-1)
        /// </summary>
        public static Color RGBToColor(int r, int g, int b, float alpha = 1f)
        {
            return new Color(
                Mathf.Clamp01(r / 255f),
                Mathf.Clamp01(g / 255f),
                Mathf.Clamp01(b / 255f),
                Mathf.Clamp01(alpha)
            );
        }

        /// <summary>
        /// Blends two colors with a specified blend factor
        /// </summary>
        public static Color BlendColors(Color original, Color target, float blendFactor)
        {
            blendFactor = Mathf.Clamp01(blendFactor);
            return Color.Lerp(original, target, blendFactor);
        }

        /// <summary>
        /// Logs fire system information for debugging
        /// </summary>
        public static void LogFireInfo(GameObject fireObject, string context = "")
        {
            if (fireObject == null) return;

            try
            {
                var fireComponent = fireObject.GetComponent("Fire");
                var effectsController = SafeGetComponent<EffectsControllerFire>(fireObject);
                var fireType = FireTypeDetector.GetFireType(fireObject);

                //MelonLogger.Msg($"[Fire Info{(string.IsNullOrEmpty(context) ? "" : $" - {context}")}]");
                //MelonLogger.Msg($"  Object: {fireObject.name}");
                //MelonLogger.Msg($"  Type: {FireTypeDetector.GetFireTypeName(fireType)}");
                //MelonLogger.Msg($"  Has Fire Component: {fireComponent != null}");
                //MelonLogger.Msg($"  Has Effects Controller: {effectsController != null}");
                //MelonLogger.Msg($"  Is 6-Burner Stove: {IsSixBurnerStove(fireObject)}");
                //MelonLogger.Msg($"  Should Skip Spark Modifications: {ShouldSkipSparkModifications(fireObject)}");

                if (fireComponent != null)
                {
                    // Use reflection to access methods since we got the component as object
                    var fireStateMethod = fireComponent.GetType().GetMethod("GetFireState");
                    var isLitMethod = fireComponent.GetType().GetMethod("IsLit");

                    if (fireStateMethod != null)
                        MelonLogger.Msg($"  Fire State: {fireStateMethod.Invoke(fireComponent, null)}");

                    if (isLitMethod != null)
                        MelonLogger.Msg($"  Is Lit: {isLitMethod.Invoke(fireComponent, null)}");
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error logging fire info: {e.Message}");
            }
        }
    }
}