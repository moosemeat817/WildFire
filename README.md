# 🔥 WildFire

Bring your fires to life — literally.  
**WildFire** completely overhauls how campfires, stoves, fire barrels, rim grills, forges, and ammunition workbenches look and behave in *The Long Dark*, with **dynamic fuel-based colors**, **realistic sparks and smoke**, and **smooth lighting transitions** that react to whatever fuel you throw in.

---

## 🌈 Dynamic Fuel-Based Fire Colors

Every fuel burns differently — and now your fires do too.

- **Automatic Color Tracking:** Fire color updates in real-time based on the fuel you add.  
- **Intelligent Color Blending:** Mixes multiple fuels with weighted color blending for smooth transitions.  
- **Persistent Colors:** Fuel colors linger based on burn time, with adjustable duration multipliers.  
- **Hardcoded Fuel Colors:** Vanilla and custom fuels use **scientifically accurate** flame colors.  
- **Smooth Transitions:** No sudden pops — just a clean, cinematic fade between colors.

> Example: Toss in magnesium powder for a blinding white blaze, or rubidium for a moody violet flame.

---

## ⚡ Enhanced Spark Effects

Because every good fire deserves a little drama.

- **Fuel Addition Sparks:** Every time you add fuel, you’ll see a custom spark burst.  
- **Per-Fuel Customization:** Each fuel type has unique spark behavior — emission rate, lifetime, color, speed, and size.  
- **Global Overrides:** Optionally apply your own universal spark settings to all fires.  
- **Realistic Physics:** Sparks follow believable motion, with configurable flight speed and lifetime.  
- **Smart Detection:** Automatically disables sparks for special fire types (like forges or industrial stoves).

---

## 💨 Advanced Smoke Modifications

Every fire tells a story — and now, so does its smoke.

- **Fuel-Specific Smoke:** Different fuels produce distinct smoke patterns, colors, and lifetimes.  
- **Realistic Physics:** Enhancements that preserve natural wind and motion behavior.  
- **Customizable:** Fine-tune density, opacity, lifetime, and speed.  
- **Global Override:** Force one consistent smoke style across all fuels.  
- **Smart Exclusions:** Automatically skips incompatible fires (e.g., six-burner stoves).

---

## 💡 Dynamic Fire Lighting

Matching light to flame — all in real time.

- **Auto-Matched Colors:** Fire lights instantly adapt to the current fuel color.  
- **Smooth Transitions:** Lights gently lerp as colors shift.  
- **Intelligent Offsets:** Converts bright flame tones into realistic, natural light hues.  
- **Persistent Lighting:** Colors stay locked even through reloads or scene updates.  
- **Works with Any Fuel:** Red, blue, green, purple — if it burns, it glows.

---

## 🔬 Supported Fuels

### 🪵 Vanilla Fuels (Now Burnable)

| Fuel Type         | Fire Color    | Spark Color | Special Effects                            |
|-------------------|---------------|--------------|---------------------------------------------|
| Red Flare         | Bright Red    | Red Sparks   | Moderate smoke, standard sparks             |
| Marine Flare (Blue)| Deep Blue     | Blue Sparks  | Heavy smoke, intense sparks                 |
| Dusting Sulfur    | Vibrant Green | Green Sparks | Dense smoke, extended burn                 |
| Stump Remover     | Orange/White  | Orange Sparks| Very heavy smoke, “explosive” sparks        |

### 🧪 Custom Fuels (Already Burnable)

| Fuel Type          | Fire Color       | Spark Color   | Special Effects                           |
|--------------------|------------------|---------------|--------------------------------------------|
| Barium Carbonate   | Bright Green     | Green Sparks  | Light smoke, clean burn                   |
| Magnesium Powder   | Brilliant White  | White Sparks  | Minimal smoke, intense heat               |
| Iron Filings       | Golden Yellow    | Golden Sparks | Heavy smoke, extreme spark shower         |
| Rubidium           | Purple/Violet    | Purple Sparks | Moderate smoke, alkali-metal effect       |
| Calcium Chloride   | Red/Orange       | Orange Sparks | Dense smoke, consistent burn              |
| Copper Carbonate   | Light Blue       | Blue Sparks   | Good smoke, true copper flame color       |

---

## ⚙️ Configuration Options

WildFire includes a detailed configuration file for tuning your experience.

### 🔥 General Settings
- **Enable WildFire** – Master toggle for all mod features  
- **Fire Color (RGB)** – Base fire color when no special fuels are active (default: orange)

---

### ✨ Global Spark Override Settings
> When enabled, overrides all fuel-specific spark values.

- **Enable Spark Modifications** – Master toggle for sparks  
- **Spark Emission Multiplier (0.1x – 20.0x)** – Number of sparks  
- **Spark Lifetime Multiplier (0.1x – 10.0x)** – Duration sparks remain visible  
- **Spark Size Multiplier (0.1x – 5.0x)** – Size of each spark particle  
- **Spark Speed Multiplier (0.1x – 5.0x)** – Upward velocity of sparks  
- **Spark Color (RGB)** – Custom color for global sparks  
- **Spark Duration Multiplier (0.1x – 10.0x)** – How long the overall spark effect lasts (~5s default)

---

### 💨 Global Smoke Override Settings
> When enabled, overrides all fuel-specific smoke values.

- **Enable Smoke Modifications** – Master toggle for smoke behavior  
- **Smoke Density Multiplier (0.1x – 5.0x)** – Number of smoke particles  
- **Smoke Lifetime Multiplier (0.1x – 10.0x)** – How long smoke lingers  
- **Smoke Size Multiplier (0.1x – 5.0x)** – Size of smoke particles  
- **Smoke Speed Multiplier (0.1x – 3.0x)** – Rate of smoke rise  
- **Smoke Opacity Multiplier (0.1x – 3.0x)** – Visual thickness of the smoke

---

## 🧭 Compatibility

WildFire is designed to work seamlessly with:
- Vanilla *The Long Dark*
- Other visual/fire overhaul mods (color blending is fully dynamic)
- Custom fuels and item mods (adds colors automatically if not hardcoded)

---

## 🛠️ Technical Notes

- Built using the **latest ModComponent and Addressable APIs**  
- No scripts modify base game prefabs — all changes are additive  
- Lightweight performance footprint (no constant update loops)

---

## 💬 Feedback & Contributions

Bug reports, feature suggestions, and flame color requests are welcome.  
Post in the [issues tab](../../issues) or join the modding Discord’s **#newmods** channel to share your burns.

---

**WildFire** — because survival’s better with colors. 🔥
