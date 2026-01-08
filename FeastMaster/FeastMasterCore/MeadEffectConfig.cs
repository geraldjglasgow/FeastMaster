using BepInEx.Configuration;

namespace FeastMaster
{
    public class MeadEffectConfig
    {
        public ConfigEntry<float> Duration { get; set; }
        public ConfigEntry<float> HealthOverTime { get; set; }
        public ConfigEntry<float> StaminaOverTime { get; set; }
        public ConfigEntry<float> EitrOverTime { get; set; }
    }
}
