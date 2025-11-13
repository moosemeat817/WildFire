using ModSettings;
using System;
using System.Reflection;

namespace WildFire
{
    internal class WildFireSettings : JsonModSettings
    {
        // ===========================
        // General WildFire Settings
        // ===========================

        [Name("Enable WildFire")]
        //[Description("Enable WildFire")]
        public bool fireEnabled = true;


        [Section("        Fire Color")]
        [Name("Show Fire Color Settings")]
        [Description("Show/Hide menu for fire color settings.  (NOTE: These settings define the base color for all fires.)")]
        public bool showGeneral = false;

        [Name("Fire Color Red")]
        [Description("Red component of fire color [0-255]. (Default 255)")]
        [Slider(0, 255)]
        public int fireColorR = 255;

        [Name("Fire Color Green")]
        [Description("Green component of fire color [0-255]. (Default 127)")]
        [Slider(0, 255)]
        public int fireColorG = 127;

        [Name("Fire Color Blue")]
        [Description("Blue component of fire color [0-255]. (Default 0)")]
        [Slider(0, 255)]
        public int fireColorB = 0;


        [Name("Fire Color Duration")]
        [Description("Fire color duration multiplier [1-4]. (Default 2) (NOTE: This determines how long the color of fires are changed after adding an item.  This value is multiplied by the burn time for each item.)")]
        [Slider(1, 4)]
        public int fireDuration = 2;




        // ===========================
        // Global Sparks Settings
        // ===========================
        [Section("        Global Sparks Override Settings")]
        [Name("Show Spark Settings")]
        [Description("Show/Hide menu for sparks.")]
        public bool showSparks = false;

        [Name("Enable Spark Modifications")]
        [Description("Modify the sparks that fly when adding fuel to fires. (NOTE: These setting override all individual spark values.)")]
        public bool enableSparkModifications = false;

        [Name("Spark Color Red")]
        [Description("Red component for fuel addition sparks (0-255)")]
        [Slider(0, 255)]
        public int sparkColorR = 255;

        [Name("Spark Color Green")]
        [Description("Green component for fuel addition sparks (0-255)")]
        [Slider(0, 255)]
        public int sparkColorG = 165;

        [Name("Spark Color Blue")]
        [Description("Blue component for fuel addition sparks (0-255)")]
        [Slider(0, 255)]
        public int sparkColorB = 0;

        [Name("Spark Emission Multiplier")]
        [Description("How many more sparks to emit when adding fuel (1.0 = default)")]
        [Slider(0.1f, 36.0f, NumberFormat = "{0:F1}")]
        public float sparkEmissionMultiplier = 5.0f;

        [Name("Spark Lifetime Multiplier")]
        [Description("How long sparks live and float upward (1.0 = default)")]
        [Slider(0.1f, 10.0f, NumberFormat = "{0:F1}")]
        public float sparkLifetimeMultiplier = 3.0f;

        [Name("Spark Size Multiplier")]
        [Description("Size of individual spark particles (1.0 = default)")]
        [Slider(0.1f, 10.0f, NumberFormat = "{0:F1}")]
        public float sparkSizeMultiplier = 1.5f;

        [Name("Spark Speed Multiplier")]
        [Description("How fast sparks fly upward (1.0 = default)")]
        [Slider(0.1f, 20.0f, NumberFormat = "{0:F1}")]
        public float sparkSpeedMultiplier = 1.2f;

        [Name("Spark Duration Multiplier")]
        [Description("How long the spark effect lasts when adding fuel (1.0 = default ~5 seconds)")]
        [Slider(0.1f, 36.0f, NumberFormat = "{0:F1}")]
        public float sparkDurationMultiplier = 2.0f;




        // ===========================
        // Global Smoke Settings
        // ===========================
        [Section("        Global Smoke Settings")]
        [Name("Show Smoke Customization")]
        [Description("Show/Hide menu for smoke customization. (NOTE: These settings define the baseline smoke appearance.)")]
        public bool showSmoke = false;

        [Name("Smoke Density Multiplier")]
        [Description("Amount of smoke particles (1.0 = default baseline)")]
        [Slider(0.1f, 5.0f, NumberFormat = "{0:F1}")]
        public float smokeDensityMultiplier = 1.0f;

        [Name("Smoke Lifetime Multiplier")]
        [Description("How long smoke particles last (1.0 = default baseline)")]
        [Slider(0.1f, 10.0f, NumberFormat = "{0:F1}")]
        public float smokeLifetimeMultiplier = 1.0f;

        [Name("Smoke Size Multiplier")]
        [Description("Size of smoke particles (1.0 = default baseline)")]
        [Slider(0.1f, 5.0f, NumberFormat = "{0:F1}")]
        public float smokeSizeMultiplier = 1.0f;

        [Name("Smoke Speed Multiplier")]
        [Description("Speed of smoke rising (1.0 = default baseline)")]
        [Slider(0.1f, 3.0f, NumberFormat = "{0:F1}")]
        public float smokeSpeedMultiplier = 1.0f;

        [Name("Smoke Opacity Multiplier")]
        [Description("Thickness/opacity of smoke (1.0 = default baseline)")]
        [Slider(0.1f, 3.0f, NumberFormat = "{0:F1}")]
        public float smokeOpacityMultiplier = 1.0f;




        // ===========================
        // OnChange & RefreshFields
        // ===========================
        protected override void OnChange(FieldInfo field, object oldValue, object newValue)
        {
            RefreshFields();
        }

        internal void RefreshFields()
        {
            // General
            //SetFieldVisible(nameof(fireEnabled), showGeneral);
            SetFieldVisible(nameof(fireColorR), showGeneral);
            SetFieldVisible(nameof(fireColorG), showGeneral);
            SetFieldVisible(nameof(fireColorB), showGeneral);


            // Sparks
            SetFieldVisible(nameof(enableSparkModifications), showSparks);
            SetFieldVisible(nameof(sparkEmissionMultiplier), showSparks);
            SetFieldVisible(nameof(sparkLifetimeMultiplier), showSparks);
            SetFieldVisible(nameof(sparkSizeMultiplier), showSparks);
            SetFieldVisible(nameof(sparkSpeedMultiplier), showSparks);
            SetFieldVisible(nameof(sparkColorR), showSparks);
            SetFieldVisible(nameof(sparkColorG), showSparks);
            SetFieldVisible(nameof(sparkColorB), showSparks);
            SetFieldVisible(nameof(sparkDurationMultiplier), showSparks);

            // Smoke
            SetFieldVisible(nameof(smokeDensityMultiplier), showSmoke);
            SetFieldVisible(nameof(smokeLifetimeMultiplier), showSmoke);
            SetFieldVisible(nameof(smokeSizeMultiplier), showSmoke);
            SetFieldVisible(nameof(smokeSpeedMultiplier), showSmoke);
            SetFieldVisible(nameof(smokeOpacityMultiplier), showSmoke);
        }
    }

    internal static class Settings
    {
        public static WildFireSettings options;

        public static void OnLoad()
        {
            options = new WildFireSettings();
            options.AddToModSettings("_WildFire");
            options.RefreshFields();
        }
    }
}