using System.Collections.Generic;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine.SceneManagement;

namespace FeastMaster
{
    [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
    [HarmonyPriority(Priority.First)]
    public static class PlayerAwakePatch
    {
        private static void Postfix()
        {
            if (SceneManager.GetActiveScene().name != "main")
                return;

            ApplyFoodConfigurations();
            ApplyMeadConfigurations();
        }

        private static void ApplyFoodConfigurations()
        {
            foreach (var kvp in FeastMasterData.FoodConfigs)
            {
                var item = PrefabManager.Cache.GetPrefab<ItemDrop>(kvp.Key);
                if (item == null)
                    continue;

                var shared = item.m_itemData.m_shared;
                var configs = kvp.Value;

                shared.m_food = configs[Constants.Health].Value * FeastMasterData.HealthModifier.Value;
                shared.m_foodStamina = configs[Constants.Stamina].Value * FeastMasterData.StaminaModifier.Value;
                shared.m_foodBurnTime = configs[Constants.Duration].Value * FeastMasterData.DurationModifier.Value;
                shared.m_foodRegen = configs[Constants.HealthRegen].Value * FeastMasterData.HealthRegenModifier.Value;
                shared.m_foodEitr = configs[Constants.Eitr].Value * FeastMasterData.EitrModifier.Value;
            }
        }

        private static void ApplyMeadConfigurations()
        {
            foreach (var meadName in Constants.MeadNames)
            {
                if (!FeastMasterData.MeadConfigs.TryGetValue(meadName, out var meadConfig))
                    continue;

                var itemDrop = PrefabManager.Cache.GetPrefab<ItemDrop>(meadName);
                if (itemDrop?.m_itemData.m_shared.m_consumeStatusEffect is SE_Stats stats)
                {
                    stats.m_ttl = meadConfig.Duration.Value;
                    stats.m_healthOverTime = meadConfig.HealthOverTime.Value;
                    stats.m_staminaOverTime = meadConfig.StaminaOverTime.Value;
                    stats.m_eitrOverTime = meadConfig.EitrOverTime.Value;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.GetTotalFoodValue))]
    public static class PlayerFoodDegradationPatch
    {
        // Use AccessTools to get private field at runtime
        private static readonly AccessTools.FieldRef<Player, List<Player.Food>> FoodsField =
            AccessTools.FieldRefAccess<Player, List<Player.Food>>("m_foods");

        [HarmonyPrefix]
        public static bool Prefix(Player __instance, out float hp, out float stamina, out float eitr)
        {
            if (!FeastMasterData.DisableFoodDegradation.Value)
            {
                hp = 0f;
                stamina = 0f;
                eitr = 0f;
                return true; // Run original method
            }

            // Calculate totals using original (non-degraded) values
            hp = 0f;
            stamina = 0f;
            eitr = 0f;

            var foods = FoodsField(__instance);
            foreach (var food in foods)
            {
                var shared = food.m_item.m_shared;
                hp += shared.m_food;
                stamina += shared.m_foodStamina;
                eitr += shared.m_foodEitr;
            }

            return false; // Skip original method
        }
    }
}
