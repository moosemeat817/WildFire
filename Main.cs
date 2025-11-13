using MelonLoader;
using UnityEngine;

namespace WildFire
{
    public class Main : MelonMod
    {
        private static float updateInterval = 2f;
        private static float lastUpdateTime = 0f;
        private static int frameSkipCounter = 0;
        private static readonly int framesToSkip = 120;

        public override void OnInitializeMelon()
        {
            Settings.OnLoad();
            FireLightOverrideManager.Initialize();
            LoggerInstance.Msg("WildFire Loaded.  Burninate in color!");
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (!Settings.options.fireEnabled)
            {
                LoggerInstance.Msg("WildFire Mod is disabled in settings");
                return;
            }

            //LoggerInstance.Msg("Fire Customization Mod with Fuel-Based Colors Initialized");
            //LoggerInstance.Msg("Features: Fire Colors, Fuel Colors, Spark Effects, Smoke Modifications, Light Glow");

            FuelColorTracker.CleanupAll();
            SmokeEffectsModifier.ClearModificationTracking();
            SparkEffectsModifier.CleanupOriginalValues();
            FireLightColorModifier.CleanupAll();
            FireLightOverrideManager.ClearAllOverrides();

            lastUpdateTime = 0f;
            frameSkipCounter = 0;

            // Initialize all active fires in the scene with default color from settings
            InitializeSceneFireColors();

            // Initialize all active fires with default smoke settings
            InitializeSceneSmokeSettings();
        }


        /// <summary>
        /// Initialize all active fires in the scene with the default color from settings
        /// This ensures fires start with the configured color even before fuel is added
        /// </summary>
        private void InitializeSceneFireColors()
        {
            try
            {
                // Find all EffectsControllerFire components in the scene
                var fireControllers = UnityEngine.Object.FindObjectsOfType<EffectsControllerFire>();

                if (fireControllers == null || fireControllers.Length == 0)
                {
                    //LoggerInstance.Msg("No fires found in scene to initialize");
                    return;
                }

                //LoggerInstance.Msg($"Found {fireControllers.Length} fire(s) in scene, initializing with default color");

                // Get the default fire color from settings
                Color defaultFireColor = FireUtils.RGBToColor(
                    Settings.options.fireColorR,
                    Settings.options.fireColorG,
                    Settings.options.fireColorB);

                //LoggerInstance.Msg($"Default fire color from settings: R={defaultFireColor.r:F3}, G={defaultFireColor.g:F3}, B={defaultFireColor.b:F3}");

                // Apply default color to all fires in the scene
                foreach (var fireController in fireControllers)
                {
                    if (fireController == null || fireController.gameObject == null)
                        continue;

                    try
                    {
                        GameObject fireObject = fireController.gameObject;
                        FireType fireType = FireTypeDetector.GetFireType(fireObject);

                        //LoggerInstance.Msg($"Initializing fire: {fireObject.name} (Type: {FireTypeDetector.GetFireTypeName(fireType)})");

                        // Register the default fire color override
                        FireLightOverrideManager.RegisterFireLightOverride(fireObject, defaultFireColor);

                        // Apply color modifications to all particle systems
                        var allParticleSystems = fireController.GetComponentsInChildren<ParticleSystem>();

                        if (allParticleSystems != null && allParticleSystems.Length > 0)
                        {
                            foreach (var ps in allParticleSystems)
                            {
                                if (ps != null && ps.gameObject.activeInHierarchy && ps.emission.enabled && ps.isPlaying)
                                {
                                    FireStage stage = FireTypeDetector.GetStageFromName(ps.gameObject.name);
                                    FireColorIntensityModifier.ApplyColorModifications(ps, fireType, stage, fireObject);
                                }
                            }
                        }

                        //LoggerInstance.Msg($"  Initialized fire with default color");
                    }
                    catch (System.Exception e)
                    {
                        //LoggerInstance.Error($"Error initializing fire {fireController?.gameObject?.name}: {e.Message}");
                    }
                }

                //LoggerInstance.Msg($"Scene fire initialization complete");
            }
            catch (System.Exception e)
            {
                LoggerInstance.Error($"Error initializing scene fire colors: {e.Message}");
            }
        }

        /// <summary>
        /// Initialize all active fires in the scene with the default smoke settings
        /// Ensures smoke starts with the configured multipliers even before fuel is added
        /// </summary>
        private void InitializeSceneSmokeSettings()
        {
            try
            {
                // Find all EffectsControllerFire components in the scene
                var fireControllers = UnityEngine.Object.FindObjectsOfType<EffectsControllerFire>();

                if (fireControllers == null || fireControllers.Length == 0)
                {
                    return;
                }

                // Get the default smoke settings from global settings
                float densityMult = Settings.options.smokeDensityMultiplier;
                float lifetimeMult = Settings.options.smokeLifetimeMultiplier;
                float sizeMult = Settings.options.smokeSizeMultiplier;
                float speedMult = Settings.options.smokeSpeedMultiplier;
                float opacityMult = Settings.options.smokeOpacityMultiplier;

                //LoggerInstance.Msg($"Initializing scene smoke with global settings - Density: {densityMult:F1}x, Lifetime: {lifetimeMult:F1}x, Size: {sizeMult:F1}x, Speed: {speedMult:F1}x, Opacity: {opacityMult:F1}x");

                // Apply default smoke settings to all fires in the scene
                foreach (var fireController in fireControllers)
                {
                    if (fireController == null || fireController.gameObject == null)
                        continue;

                    try
                    {
                        GameObject fireObject = fireController.gameObject;

                        // Skip 6-burner stove
                        if (FireUtils.IsSixBurnerStove(fireObject))
                        {
                            continue;
                        }

                        // Get all particle systems in this fire
                        var allParticleSystems = fireController.GetComponentsInChildren<ParticleSystem>();

                        if (allParticleSystems != null && allParticleSystems.Length > 0)
                        {
                            foreach (var ps in allParticleSystems)
                            {
                                if (ps != null && ps.gameObject.activeInHierarchy && ps.emission.enabled && ps.isPlaying)
                                {
                                    // Only apply to smoke systems
                                    if (SmokeEffectsModifier.IsSmokeParticleSystem(ps))
                                    {
                                        SmokeEffectsModifier.ApplySmokeModifications(ps, FireType.Unknown, fireObject);
                                    }
                                }
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        //LoggerInstance.Error($"Error initializing smoke for {fireController?.gameObject?.name}: {e.Message}");
                    }
                }

                //LoggerInstance.Msg($"Scene smoke initialization complete");
            }
            catch (System.Exception e)
            {
                LoggerInstance.Error($"Error initializing scene smoke settings: {e.Message}");
            }
        }


        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            //LoggerInstance.Msg($"Scene loaded: {sceneName}");
        }

        public override void OnUpdate()
        {
            if (!Settings.options.fireEnabled)
                return;

            try
            {
                // Update fuel colors at the normal interval
                frameSkipCounter++;
                if (frameSkipCounter < framesToSkip)
                    return;

                frameSkipCounter = 0;

                if (Time.time - lastUpdateTime < updateInterval)
                    return;

                lastUpdateTime = Time.time;

                if (FuelColorTracker.HasAnyTrackedFires())
                {
                    FuelColorTracker.UpdateFireColors();
                }
            }
            catch (System.Exception e)
            {
                if (e is System.NullReferenceException == false)
                {
                    LoggerInstance.Error($"Error in fire color update: {e.Message}");
                }
            }
        }

        /// <summary>
        /// NEW: LateUpdate runs after all rendering, perfect for continuously applying light overrides
        /// This ensures our light colors are set as the very last thing before rendering
        /// </summary>
        public override void OnLateUpdate()
        {
            if (!Settings.options.fireEnabled)
                return;

            try
            {
                // Apply light color overrides EVERY FRAME in LateUpdate
                // This runs after BlendLightColor.SetLightColor and ensures our colors stick
                FireLightOverrideManager.UpdateLightColors();
            }
            catch (System.Exception e)
            {
                if (e is System.NullReferenceException == false)
                {
                    LoggerInstance.Error($"Error in late update light override: {e.Message}");
                }
            }
        }

        public override void OnApplicationQuit()
        {
            FuelColorTracker.CleanupAll();
            SmokeEffectsModifier.ClearModificationTracking();
            SmokeEffectsModifier.CleanupOriginalValues();
            FireLightColorModifier.CleanupAll();
            FireLightOverrideManager.Cleanup();
        }
    }
}