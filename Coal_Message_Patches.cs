using HarmonyLib;
using UnityEngine;
using MelonLoader;

namespace WildFire
{
    /// <summary>
    /// Comprehensive patch to change coal-specific fuel messages to generic ones
    /// </summary>
    internal static class CoalMessagePatches
    {
        // Patch the HUDMessage system which displays on-screen messages
        [HarmonyPatch(typeof(HUDMessage), nameof(HUDMessage.AddMessage), new System.Type[] { typeof(string), typeof(bool), typeof(bool) })]
        internal static class HUDMessagePatch
        {
            static void Prefix(ref string message)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    // Check if this is a coal-related message
                    if (message.ToUpper().Contains("COAL") && message.ToUpper().Contains("MINUTES BEFORE"))
                    {
                        // Replace COAL with ITEM
                        string original = message;
                        message = message.Replace("COAL", "ITEM").Replace("Coal", "Item").Replace("coal", "item");

                        //MelonLogger.Msg($"[CoalMessagePatch] Modified message:");
                        //MelonLogger.Msg($"  Original: '{original}'");
                        //MelonLogger.Msg($"  Modified: '{message}'");
                    }
                }
            }
        }

        // Patch the Localization.Get method
        [HarmonyPatch(typeof(Localization), nameof(Localization.Get), new System.Type[] { typeof(string) })]
        internal static class LocalizationGetPatch
        {
            static void Postfix(string key, ref string __result)
            {
                if (!string.IsNullOrEmpty(__result))
                {
                    // Check for coal-related localization strings
                    if (__result.ToUpper().Contains("COAL") && __result.ToUpper().Contains("MINUTES"))
                    {
                        string original = __result;
                        __result = __result.Replace("COAL", "ITEM").Replace("Coal", "Item").Replace("coal", "item");

                        //MelonLogger.Msg($"[LocalizationPatch] Modified localization key '{key}':");
                        //MelonLogger.Msg($"  Original: '{original}'");
                        //MelonLogger.Msg($"  Modified: '{__result}'");
                    }
                }
            }
        }

        // Alternative: Patch Fire.AddFuel to intercept the message before display
        [HarmonyPatch(typeof(Fire), "AddFuel")]
        internal static class FireAddFuelMessagePatch
        {
            static bool Prefix(Fire __instance, GearItem fuel)
            {
                // We don't actually block the method, just log for debugging
                if (fuel != null)
                {
                    var fuelSource = fuel.GetComponent<FuelSourceItem>();
                    if (fuelSource != null && fuelSource.m_FireAgeMinutesBeforeAdding > 0)
                    {
                        //MelonLogger.Msg($"[FireAddFuelPatch] Fuel '{fuel.name}' has {fuelSource.m_FireAgeMinutesBeforeAdding} minute requirement");
                    }
                }
                return true; // Allow the original method to run
            }
        }
    }
}