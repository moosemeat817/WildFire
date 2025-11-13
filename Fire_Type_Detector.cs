using UnityEngine;
using MelonLoader;

namespace WildFire
{
    internal static class FireTypeDetector
    {
        public static FireType GetFireType(GameObject fireObject)
        {
            if (fireObject == null) return FireType.Unknown;

            try
            {
                // Check by component type first
                if (fireObject.GetComponent<Campfire>() != null)
                    return FireType.Campfire;

                if (fireObject.GetComponent<WoodStove>() != null)
                    return FireType.Stove;

                // Check by parent components if not found on current object
                var parent = fireObject.transform.parent;
                while (parent != null)
                {
                    if (parent.GetComponent<Campfire>() != null)
                        return FireType.Campfire;

                    if (parent.GetComponent<WoodStove>() != null)
                        return FireType.Stove;

                    parent = parent.parent;
                }

                // Check by name patterns if components not found
                string objectName = fireObject.name.ToLower();

                if (objectName.Contains("campfire") || objectName.Contains("camp_fire"))
                    return FireType.Campfire;

                if (objectName.Contains("stove") || objectName.Contains("wood_stove"))
                    return FireType.Stove;

                if (objectName.Contains("fireplace"))
                    return FireType.Fireplace;

                if (objectName.Contains("torch"))
                    return FireType.Torch;

                // Check parent names as well
                parent = fireObject.transform.parent;
                while (parent != null)
                {
                    string parentName = parent.name.ToLower();

                    if (parentName.Contains("campfire") || parentName.Contains("camp_fire"))
                        return FireType.Campfire;

                    if (parentName.Contains("stove") || parentName.Contains("wood_stove"))
                        return FireType.Stove;

                    if (parentName.Contains("fireplace"))
                        return FireType.Fireplace;

                    if (parentName.Contains("torch"))
                        return FireType.Torch;

                    parent = parent.parent;
                }

                return FireType.Other;
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error detecting fire type for {fireObject.name}: {e.Message}");
                return FireType.Unknown;
            }
        }

        public static string GetFireTypeName(FireType fireType)
        {
            return fireType switch
            {
                FireType.Campfire => "Campfire",
                FireType.Stove => "Wood Stove",
                FireType.Fireplace => "Fireplace",
                FireType.Torch => "Torch",
                FireType.Other => "Other Fire",
                _ => "Unknown Fire"
            };
        }

        // MOVED FROM Fire_Stage_Helper.cs: Fire stage detection logic
        public static FireStage GetStageFromName(string stageName)
        {
            if (string.IsNullOrEmpty(stageName))
                return FireStage.Unknown;

            try
            {
                string lowerStageName = stageName.ToLower();

                return lowerStageName switch
                {
                    // Check for spark effects first (fuel addition sparks)
                    var s when s.Contains("spark") => FireStage.Sparks,
                    var s when s.Contains("otherfx") => FireStage.Sparks,
                    var s when s.Contains("other_fx") => FireStage.Sparks,

                    // Regular fire stages
                    var s when s.Contains("ember") => FireStage.Embers,
                    var s when s.Contains("small") => FireStage.Small,
                    var s when s.Contains("medium") => FireStage.Medium,
                    var s when s.Contains("large") => FireStage.Large,
                    var s when s.Contains("full") => FireStage.FullBurn,
                    var s when s.Contains("accelerant") => FireStage.Accelerant,
                    var s when s.Contains("flareupsmall") => FireStage.FlareupSmall,
                    var s when s.Contains("flareupsmallfx") => FireStage.FlareupSmall,
                    var s when s.Contains("flareuplarge") => FireStage.FlareupLarge,
                    var s when s.Contains("flareuplargefx") => FireStage.FlareupLarge,
                    var s when s.Contains("stage00") => FireStage.Embers,
                    var s when s.Contains("stage01") => FireStage.Small,
                    var s when s.Contains("stage02") => FireStage.Medium,
                    var s when s.Contains("stage03") => FireStage.Large,
                    var s when s.Contains("stage04") => FireStage.FullBurn,
                    _ => FireStage.Other
                };
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error parsing fire stage name '{stageName}': {e.Message}");
                return FireStage.Unknown;
            }
        }

        // MOVED FROM Fire_Stage_Helper.cs: Helper method for getting friendly stage names
        public static string GetStageName(FireStage stage)
        {
            return stage switch
            {
                FireStage.Embers => "Starting Embers",
                FireStage.Small => "Small Flames",
                FireStage.Medium => "Medium Flames",
                FireStage.Large => "Large Flames",
                FireStage.FullBurn => "Full Burn",
                FireStage.Accelerant => "Accelerant Enhanced",
                FireStage.FlareupSmall => "Small Flare-up",
                FireStage.FlareupLarge => "Large Flare-up",
                FireStage.Sparks => "Fuel Addition Sparks",
                FireStage.Other => "Other Effects",
                _ => "Unknown Stage"
            };
        }

        // MOVED FROM Fire_Stage_Helper.cs: Stage classification helper methods
        public static bool IsFlareupStage(FireStage stage)
        {
            return stage == FireStage.FlareupSmall || stage == FireStage.FlareupLarge;
        }

        public static bool IsMainFlameStage(FireStage stage)
        {
            return stage == FireStage.Small ||
                   stage == FireStage.Medium ||
                   stage == FireStage.Large ||
                   stage == FireStage.FullBurn;
        }

        public static bool IsSpecialEffectStage(FireStage stage)
        {
            return stage == FireStage.Accelerant ||
                   stage == FireStage.FlareupSmall ||
                   stage == FireStage.FlareupLarge ||
                   stage == FireStage.Sparks;
        }

        public static bool IsSparkStage(FireStage stage)
        {
            return stage == FireStage.Sparks;
        }
    }
}