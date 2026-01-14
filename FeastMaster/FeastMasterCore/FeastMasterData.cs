using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Jotunn.Managers;
using ServerSync;
using Object = UnityEngine.Object;

namespace FeastMaster
{
    public static class FeastMasterData
    {
        private const string ModifiersGroup = "0. Global Settings";

        public static Dictionary<string, Dictionary<string, ConfigEntry<float>>> FoodConfigs { get; }
            = new Dictionary<string, Dictionary<string, ConfigEntry<float>>>();

        public static Dictionary<string, MeadEffectConfig> MeadConfigs { get; }
            = new Dictionary<string, MeadEffectConfig>();

        public static ConfigEntry<float> HealthModifier { get; private set; }
        public static ConfigEntry<float> StaminaModifier { get; private set; }
        public static ConfigEntry<float> DurationModifier { get; private set; }
        public static ConfigEntry<float> HealthRegenModifier { get; private set; }
        public static ConfigEntry<float> EitrModifier { get; private set; }
        public static ConfigEntry<bool> DisableFoodDegradation { get; private set; }

        private static ConfigFile _configFile;
        private static ConfigSync _configSync;

        public static void Initialize(ConfigFile configFile, ConfigSync configSync)
        {
            _configFile = configFile;
            _configSync = configSync;

            HealthModifier = CreateConfigEntry(ModifiersGroup, "Health Modifier", 1f,
                "Multiplier for health value of all foods.");
            StaminaModifier = CreateConfigEntry(ModifiersGroup, "Stamina Modifier", 1f,
                "Multiplier for stamina value of all foods.");
            DurationModifier = CreateConfigEntry(ModifiersGroup, "Duration Modifier", 1f,
                "Multiplier for duration of all foods.");
            HealthRegenModifier = CreateConfigEntry(ModifiersGroup, "Health Regen Modifier", 1f,
                "Multiplier for health regeneration of all foods.");
            EitrModifier = CreateConfigEntry(ModifiersGroup, "Eitr Modifier", 1f,
                "Multiplier for eitr value of all foods.");
            DisableFoodDegradation = CreateConfigEntry(ModifiersGroup, "Disable Food Degradation", false,
                "When enabled, food stats do not decrease over time.");
        }

        public static void LoadConfigurations()
        {
            LoadFoodConfigurations();
            LoadMeadConfigurations();
        }

        private static void LoadFoodConfigurations()
        {
            if (FoodConfigs.Count > 0)
                return;

            FeastMaster.Log.LogInfo("Loading food configurations...");

            Dictionary<string, Object> itemDrops = PrefabManager.Cache.GetPrefabs(typeof(ItemDrop));

            foreach (var kvp in itemDrops)
            {
                var itemDrop = (ItemDrop)kvp.Value;
                var shared = itemDrop.m_itemData.m_shared;

                if (shared.m_itemType != ItemDrop.ItemData.ItemType.Consumable || shared.m_foodStamina <= 0)
                    continue;

                var foodName = kvp.Key;
                var configs = new Dictionary<string, ConfigEntry<float>>
                {
                    [Constants.Health] = CreateConfigEntry(foodName, Constants.Health, shared.m_food,
                        $"Health value of {foodName}."),
                    [Constants.Stamina] = CreateConfigEntry(foodName, Constants.Stamina, shared.m_foodStamina,
                        $"Stamina value of {foodName}."),
                    [Constants.Duration] = CreateConfigEntry(foodName, Constants.Duration, shared.m_foodBurnTime,
                        $"Duration of {foodName} in seconds."),
                    [Constants.HealthRegen] = CreateConfigEntry(foodName, Constants.HealthRegen, shared.m_foodRegen,
                        $"Health regeneration of {foodName}."),
                    [Constants.Eitr] = CreateConfigEntry(foodName, Constants.Eitr, shared.m_foodEitr,
                        $"Eitr value of {foodName}.")
                };

                FoodConfigs[foodName] = configs;
            }

            CreateConfigEntry("General", "Lock Configuration", true,
                "[Server Only] Locks configuration so clients cannot change values.");

            FeastMaster.Log.LogInfo($"Loaded {FoodConfigs.Count} food configurations.");
        }

        private static void LoadMeadConfigurations()
        {
            foreach (var meadName in Constants.MeadNames)
            {
                var itemDrop = PrefabManager.Cache.GetPrefab<ItemDrop>(meadName);
                if (itemDrop == null)
                    continue;

                var effect = itemDrop.m_itemData.m_shared.m_consumeStatusEffect as SE_Stats;
                float duration = effect?.m_ttl ?? 600f;
                float healthOverTime = effect?.m_healthOverTime ?? 0f;
                float staminaOverTime = effect?.m_staminaOverTime ?? 0f;
                float eitrOverTime = effect?.m_eitrOverTime ?? 0f;

                string group = $"0Meads_{meadName}";
                MeadConfigs[meadName] = new MeadEffectConfig
                {
                    Duration = CreateConfigEntry(group, "Duration", duration,
                        $"Duration of {meadName} effect in seconds."),
                    HealthOverTime = CreateConfigEntry(group, "HealthOverTime", healthOverTime,
                        $"Total health restored over time by {meadName}."),
                    StaminaOverTime = CreateConfigEntry(group, "StaminaOverTime", staminaOverTime,
                        $"Total stamina restored over time by {meadName}."),
                    EitrOverTime = CreateConfigEntry(group, "EitrOverTime", eitrOverTime,
                        $"Total eitr restored over time by {meadName}.")
                };
            }

            FeastMaster.Log.LogInfo($"Loaded {MeadConfigs.Count} mead configurations.");
        }

        private static ConfigEntry<T> CreateConfigEntry<T>(
            string group,
            string name,
            T defaultValue,
            string description,
            bool synchronizedSetting = true)
        {
            if (_configFile == null || _configSync == null)
                throw new InvalidOperationException("FeastMasterData not initialized. Call Initialize() first.");

            string syncNote = synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]";
            var configEntry = _configFile.Bind(group, name, defaultValue, description + syncNote);

            var syncedEntry = _configSync.AddConfigEntry(configEntry);
            syncedEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }
    }
}
