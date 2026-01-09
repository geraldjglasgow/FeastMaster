using System.Collections.Generic;
using HarmonyLib;

namespace FeastMaster
{
    /// <summary>
    /// Patches Player.EatFood to apply configured food values at consumption time.
    /// This ensures config changes take effect immediately without reloading.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.EatFood))]
    public static class PlayerEatFoodPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ItemDrop.ItemData item)
        {
            if (item == null)
                return;

            var shared = item.m_shared;
            var foodName = shared.m_name;

            // Try to find config by item name (prefab name)
            if (!FeastMasterData.FoodConfigs.TryGetValue(foodName, out var configs))
            {
                // Try without $ prefix if present
                foodName = item.m_dropPrefab?.name ?? foodName;
                if (!FeastMasterData.FoodConfigs.TryGetValue(foodName, out configs))
                    return;
            }

            // Apply configured values with global modifiers
            shared.m_food = configs[Constants.Health].Value * FeastMasterData.HealthModifier.Value;
            shared.m_foodStamina = configs[Constants.Stamina].Value * FeastMasterData.StaminaModifier.Value;
            shared.m_foodBurnTime = configs[Constants.Duration].Value * FeastMasterData.DurationModifier.Value;
            shared.m_foodRegen = configs[Constants.HealthRegen].Value * FeastMasterData.HealthRegenModifier.Value;
            shared.m_foodEitr = configs[Constants.Eitr].Value * FeastMasterData.EitrModifier.Value;
        }
    }

    /// <summary>
    /// Patches status effect application to apply mead configs at consumption time.
    /// </summary>
    [HarmonyPatch(typeof(SEMan), nameof(SEMan.AddStatusEffect), typeof(StatusEffect), typeof(bool), typeof(int), typeof(float))]
    public static class SEManAddStatusEffectPatch
    {
        [HarmonyPrefix]
        public static void Prefix(StatusEffect statusEffect)
        {
            if (statusEffect == null || !(statusEffect is SE_Stats stats))
                return;

            // Find matching mead config by status effect name
            foreach (var kvp in FeastMasterData.MeadConfigs)
            {
                if (statusEffect.name.Contains(kvp.Key) || statusEffect.m_name.Contains(kvp.Key))
                {
                    var config = kvp.Value;
                    stats.m_ttl = config.Duration.Value;
                    stats.m_healthOverTime = config.HealthOverTime.Value;
                    stats.m_staminaOverTime = config.StaminaOverTime.Value;
                    stats.m_eitrOverTime = config.EitrOverTime.Value;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Patches GetTotalFoodValue to optionally disable food degradation.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.GetTotalFoodValue))]
    public static class PlayerFoodDegradationPatch
    {
        private static readonly AccessTools.FieldRef<Player, List<Player.Food>> FoodsField =
            AccessTools.FieldRefAccess<Player, List<Player.Food>>("m_foods");

        [HarmonyPrefix]
        public static bool Prefix(Player __instance, out float hp, out float stamina, out float eitr)
        {
            hp = 0f;
            stamina = 0f;
            eitr = 0f;

            if (!FeastMasterData.DisableFoodDegradation.Value)
                return true;

            // Use full food values without degradation
            var foods = FoodsField(__instance);
            foreach (var food in foods)
            {
                var shared = food.m_item.m_shared;
                hp += shared.m_food;
                stamina += shared.m_foodStamina;
                eitr += shared.m_foodEitr;
            }

            return false;
        }
    }
}
