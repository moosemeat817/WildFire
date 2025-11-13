using HarmonyLib;
using UnityEngine;
using MelonLoader;
using ComplexLogger;

namespace WildFire
{
    [HarmonyPatch(typeof(GearItem), nameof(GearItem.Awake))]
    internal static class GearPatches
    {
        static void Postfix(GearItem __instance)
        {
            if (!Settings.options.fireEnabled)
                return;

            if (__instance == null || __instance.gameObject == null) return;

            try
            {
                string name = __instance.name;
                var fuelType = GearItemData.GetFuelTypeFromName(name);

                if (fuelType != CustomFuelType.None && fuelType != CustomFuelType.Other && GearItemData.ShouldMakeBurnable(name))
                {
                    var fuelSource = __instance.gameObject.GetComponent<FuelSourceItem>();
                    if (fuelSource == null)
                    {
                        fuelSource = __instance.gameObject.AddComponent<FuelSourceItem>();

                        var settings = GearItemData.GetBurnableSettings(fuelType);

                        fuelSource.m_BurnDurationHours = settings.burnDurationHours;
                        fuelSource.m_FireAgeMinutesBeforeAdding = settings.fireAgeMinutesBeforeAdding;
                        fuelSource.m_FireStartSkillModifier = settings.fireStartSkillModifier;
                        fuelSource.m_HeatIncrease = settings.heatIncrease;
                        fuelSource.m_HeatInnerRadius = settings.heatInnerRadius;
                        fuelSource.m_HeatOuterRadius = settings.heatOuterRadius;
                        fuelSource.m_FireStartDurationModifier = settings.fireStartDurationModifier;
                        fuelSource.m_IsWet = settings.isWet;
                        fuelSource.m_IsTinder = settings.isTinder;
                        fuelSource.m_IsBurntInFireTracked = settings.isBurntInFireTracked;
                        fuelSource.enabled = true;

                        __instance.m_FuelSourceItem = fuelSource;

                        var fuelName = GearItemData.GetFuelTypeName(fuelType);
                        //MelonLogger.Msg($"[WildFire] Made {name} ({fuelName}) burnable as fuel with settings: burn={settings.burnDurationHours:F2}h, heat={settings.heatIncrease:F1}");
                    }
                }
                else if (fuelType != CustomFuelType.None && fuelType != CustomFuelType.Other)
                {
                    var fuelName = GearItemData.GetFuelTypeName(fuelType);
                    bool isCustom = GearItemData.IsCustomFuelType(fuelType);
                    bool isVanilla = GearItemData.IsVanillaFuelType(fuelType);

                    //MelonLogger.Msg($"[WildFire] Detected {(isCustom ? "custom" : isVanilla ? "vanilla" : "unknown")} fuel item {name} ({fuelName}) - using existing burnable behavior");
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"[WildFire] Error in GearPatches: {e.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(Fire), "AddFuel")]
    internal static class FuelAddedDebugPatch
    {
        static void Prefix(Fire __instance, GearItem fuel)
        {
            if (!Settings.options.fireEnabled)
                return;

            if (fuel == null) return;

            try
            {
                string itemName = fuel.name;
                var fuelType = GearItemData.GetFuelTypeFromName(itemName);

                if (fuelType != CustomFuelType.None && fuelType != CustomFuelType.Other)
                {
                    var fuelName = GearItemData.GetFuelTypeName(fuelType);
                    bool isCustomFuel = GearItemData.IsCustomFuelType(fuelType);
                    bool isVanillaFuel = GearItemData.IsVanillaFuelType(fuelType);

                    //MelonLogger.Msg($"[WildFire] Fuel being added to fire: {itemName} ({fuelName}) - Type: {(isCustomFuel ? "Custom" : isVanillaFuel ? "Vanilla" : "Unknown")}");

                    GameObject fireObject = __instance.gameObject;
                    FuelColorTracker.AddFuelToFire(fireObject, fuelType);

                    if (GearItemData.HasCustomSparkSettings(fuelType))
                    {
                        var sparkSettings = GearItemData.GetSparkSettings(fuelType);
                        //MelonLogger.Msg($"[WildFire] Custom spark settings for {fuelName}: " +
                                       //$"Emission={sparkSettings.emissionMultiplier:F1}x, " +
                                       //$"Color=R{sparkSettings.sparkColor.r:F1}G{sparkSettings.sparkColor.g:F1}B{sparkSettings.sparkColor.b:F1}");
                    }

                    if (GearItemData.HasCustomSmokeSettings(fuelType))
                    {
                        var smokeSettings = GearItemData.GetSmokeSettings(fuelType);
                        //MelonLogger.Msg($"[WildFire] Custom smoke settings for {fuelName}: " +
                                       //$"Density={smokeSettings.densityMultiplier:F1}x, " +
                                       //$"Lifetime={smokeSettings.lifetimeMultiplier:F1}x, " +
                                       //$"Size={smokeSettings.sizeMultiplier:F1}x");
                    }
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"[WildFire] Error in fuel added debug patch: {e.Message}");
            }
        }

        static void Postfix(Fire __instance, GearItem fuel)
        {
            if (!Settings.options.fireEnabled)
                return;

            if (fuel == null) return;

            try
            {
                string itemName = fuel.name;
                var fuelType = GearItemData.GetFuelTypeFromName(itemName);

                if (fuelType != CustomFuelType.None && fuelType != CustomFuelType.Other)
                {
                    var fuelName = GearItemData.GetFuelTypeName(fuelType);
                    //MelonLogger.Msg($"[WildFire] Refreshing fire effects for {fuelName}");

                    GameObject fireObject = __instance.gameObject;

                    FireType fireType = FireTypeDetector.GetFireType(fireObject);

                    var effectsController = fireObject.GetComponent<EffectsControllerFire>();
                    if (effectsController != null)
                    {
                        FirePatches.RefreshAllFireEffects(effectsController, fireType);

                        FirePatches.StartDelayedSparkRefreshSync(effectsController, fireType, 0.1f);
                    }
                    else
                    {
                        //MelonLogger.Warning($"[WildFire] No EffectsControllerFire found for {fireObject.name}");
                    }
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"[WildFire] Error in fuel added postfix patch: {e.Message}");
            }
        }
    }
}
