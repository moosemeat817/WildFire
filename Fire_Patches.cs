using WildFire;
using UnityEngine;
using MelonLoader;
using System.Reflection;
using System.Threading.Tasks;
using Il2CppTLD.Gear;

namespace WildFire
{
    public static class FirePatches
    {
        private static void FindAndModifySparkEffects(EffectsControllerFire controller, FireType fireType)
        {
            if (!Settings.options.fireEnabled)
                return;

            try
            {
                CustomFuelType recentFuelType = FuelColorTracker.GetMostRecentFuelType(controller.gameObject);

                if (recentFuelType != CustomFuelType.None)
                {
                    var sparkSettings = FuelColorTracker.GetSparkSettingsForFuel(recentFuelType);
                    var fuelName = GearItemData.GetFuelTypeName(recentFuelType);
                    //MelonLogger.Msg($"Searching for spark effects with {fuelName} settings: Emit={sparkSettings.emissionMultiplier:F1}x, Life={sparkSettings.lifetimeMultiplier:F1}x");
                }

                var allParticleSystems = controller.GetComponentsInChildren<ParticleSystem>();

                if (allParticleSystems != null && allParticleSystems.Length > 0)
                {
                    foreach (var ps in allParticleSystems)
                    {
                        if (ps != null && ps.gameObject.activeInHierarchy)
                        {
                            string psType = ParticleSystemModifier.GetParticleSystemType(ps);

                            if (psType == "Spark")
                            {
                                //MelonLogger.Msg($"Found potential spark system: {ps.gameObject.name}");

                                if (ps.emission.enabled && ps.isPlaying)
                                {
                                    string effectDescription = recentFuelType != CustomFuelType.None ?
                                        $"fuel-specific ({GearItemData.GetFuelTypeName(recentFuelType)})" : "global";

                                    //MelonLogger.Msg($"Active spark system found: {ps.gameObject.name} - applying {effectDescription} settings");

                                    FireStage stage = FireTypeDetector.GetStageFromName(ps.gameObject.name);

                                    ParticleSystemModifier.ApplyModifications(ps, fireType, stage, controller.gameObject);
                                }
                            }
                            else if (psType == "Smoke")
                            {
                                //MelonLogger.Msg($"Found smoke system: {ps.gameObject.name}");

                                if (ps.emission.enabled && ps.isPlaying)
                                {
                                    FireStage stage = FireTypeDetector.GetStageFromName(ps.gameObject.name);
                                    ParticleSystemModifier.ApplyModifications(ps, fireType, stage, controller.gameObject);
                                }
                            }
                        }
                    }
                }

                TryAccessOtherFXField(controller, fireType);
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error finding spark effects: {e.Message}");
            }
        }

        private static void TryAccessOtherFXField(EffectsControllerFire controller, FireType fireType)
        {
            try
            {
                var controllerType = controller.GetType();
                var otherFXField = controllerType.GetField("l_OtherFX", BindingFlags.NonPublic | BindingFlags.Instance);

                if (otherFXField != null)
                {
                    var otherFXValue = otherFXField.GetValue(controller);
                    if (otherFXValue is ParticleSystem otherFXParticleSystem)
                    {
                        //MelonLogger.Msg($"Found l_OtherFX particle system via reflection: {otherFXParticleSystem.gameObject.name}");

                        if (otherFXParticleSystem.gameObject.activeInHierarchy && otherFXParticleSystem.emission.enabled)
                        {
                            FireStage stage = FireTypeDetector.GetStageFromName(otherFXParticleSystem.gameObject.name);

                            ParticleSystemModifier.ApplyModifications(otherFXParticleSystem, fireType, stage, controller.gameObject);
                        }
                    }
                }
                else
                {
                    //MelonLogger.Msg("Could not find l_OtherFX field via reflection");
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Warning($"Error accessing l_OtherFX field: {e.Message}");
            }
        }

        public static async void StartDelayedSparkRefresh(EffectsControllerFire controller, FireType fireType, float delay = 0.1f)
        {
            try
            {
                if (controller != null && controller.gameObject != null)
                {
                    await Task.Delay((int)(delay * 1000));

                    if (controller != null && controller.gameObject != null)
                    {
                        FindAndModifySparkEffects(controller, fireType);
                    }
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error in delayed spark refresh: {e.Message}");
                try
                {
                    if (controller != null && controller.gameObject != null)
                    {
                        FindAndModifySparkEffects(controller, fireType);
                    }
                }
                catch (System.Exception fallbackError)
                {
                    MelonLogger.Error($"Fallback also failed: {fallbackError.Message}");
                }
            }
        }

        public static void StartDelayedSparkRefreshSync(EffectsControllerFire controller, FireType fireType, float delay = 0.1f)
        {
            try
            {
                if (controller != null && controller.gameObject != null)
                {
                    //MelonLogger.Msg($"Applying spark effects immediately (requested delay: {delay:F1}s)");
                    FindAndModifySparkEffects(controller, fireType);
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error in synchronous spark refresh: {e.Message}");
            }
        }

        public static void RefreshAllFireEffects(EffectsControllerFire controller, FireType fireType)
        {
            if (!Settings.options.fireEnabled)
                return;

            try
            {
                if (controller == null || controller.gameObject == null) return;

                //MelonLogger.Msg($"Refreshing all fire effects for {controller.gameObject.name}");

                // START NEW REFRESH CYCLE - Clear smoke processing tracking
                SmokeEffectsModifier.StartNewRefreshCycle();

                var allParticleSystems = controller.GetComponentsInChildren<ParticleSystem>();

                if (allParticleSystems != null && allParticleSystems.Length > 0)
                {
                    foreach (var ps in allParticleSystems)
                    {
                        if (ps != null && ps.gameObject.activeInHierarchy && ps.emission.enabled && ps.isPlaying)
                        {
                            string psType = ParticleSystemModifier.GetParticleSystemType(ps);
                            FireStage stage = FireTypeDetector.GetStageFromName(ps.gameObject.name);

                            //MelonLogger.Msg($"Refreshing {psType} system: {ps.gameObject.name}");
                            ParticleSystemModifier.ApplyModifications(ps, fireType, stage, controller.gameObject);
                        }
                    }
                }

                TryAccessOtherFXField(controller, fireType);
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error refreshing all fire effects: {e.Message}");
            }
        }

        public static void DebugFireParticleSystems(EffectsControllerFire controller)
        {
            try
            {
                if (controller == null || controller.gameObject == null)
                {
                    //MelonLogger.Msg("=== No controller to debug ===");
                    return;
                }

                //MelonLogger.Msg($"=== Particle Systems Debug for {controller.gameObject.name} ===");

                var allParticleSystems = controller.GetComponentsInChildren<ParticleSystem>();

                if (allParticleSystems == null || allParticleSystems.Length == 0)
                {
                    //MelonLogger.Msg("No particle systems found");
                    return;
                }

                //MelonLogger.Msg($"Found {allParticleSystems.Length} particle systems:");

                foreach (var ps in allParticleSystems)
                {
                    if (ps != null)
                    {
                        string psType = ParticleSystemModifier.GetParticleSystemType(ps);
                        //ParticleSystemModifier.LogParticleSystemInfo(ps, $"Type: {psType}");
                    }
                }

                try
                {
                    var controllerType = controller.GetType();
                    var otherFXField = controllerType.GetField("l_OtherFX", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (otherFXField != null)
                    {
                        var otherFXValue = otherFXField.GetValue(controller);
                        if (otherFXValue is ParticleSystem otherFXPs)
                        {
                            //MelonLogger.Msg("Found l_OtherFX via reflection:");
                            ParticleSystemModifier.LogParticleSystemInfo(otherFXPs, "Type: l_OtherFX");
                        }
                    }
                }
                catch (System.Exception e)
                {
                    //MelonLogger.Warning($"Could not access l_OtherFX via reflection: {e.Message}");
                }

                //MelonLogger.Msg("=== End Particle Systems Debug ===");
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error in debug particle systems: {e.Message}");
            }
        }
    }
}