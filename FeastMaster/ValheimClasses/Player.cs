using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
    /// Transpiler to disable food degradation by replacing field accesses in GetTotalFoodValue.
    /// Replaces loads of degraded health/stamina/eitr with calls to helper methods that check config at runtime.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.GetTotalFoodValue))]
    public static class PlayerFoodDegradationPatch
    {
        private static readonly FieldInfo field_Food_m_health = AccessTools.Field(typeof(Player.Food), nameof(Player.Food.m_health));
        private static readonly FieldInfo field_Food_m_stamina = AccessTools.Field(typeof(Player.Food), nameof(Player.Food.m_stamina));
        private static readonly FieldInfo field_Food_m_eitr = AccessTools.Field(typeof(Player.Food), nameof(Player.Food.m_eitr));

        // Helper methods that check config at runtime
        public static float GetFoodHealth(Player.Food food)
        {
            if (FeastMasterData.DisableFoodDegradation.Value)
                return food.m_item.m_shared.m_food;
            return food.m_health;
        }

        public static float GetFoodStamina(Player.Food food)
        {
            if (FeastMasterData.DisableFoodDegradation.Value)
                return food.m_item.m_shared.m_foodStamina;
            return food.m_stamina;
        }

        public static float GetFoodEitr(Player.Food food)
        {
            if (FeastMasterData.DisableFoodDegradation.Value)
                return food.m_item.m_shared.m_foodEitr;
            return food.m_eitr;
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> il = instructions.ToList();

            for (int i = 0; i < il.Count; ++i)
            {
                bool loads_health = il[i].LoadsField(field_Food_m_health);
                bool loads_stamina = il[i].LoadsField(field_Food_m_stamina);
                bool loads_eitr = il[i].LoadsField(field_Food_m_eitr);

                if (loads_health || loads_stamina || loads_eitr)
                {
                    // Replace field load with call to helper method
                    // The stack already has the Food object on it from the previous instruction
                    MethodInfo helper = loads_health ? AccessTools.Method(typeof(PlayerFoodDegradationPatch), nameof(GetFoodHealth)) :
                                        loads_stamina ? AccessTools.Method(typeof(PlayerFoodDegradationPatch), nameof(GetFoodStamina)) :
                                        AccessTools.Method(typeof(PlayerFoodDegradationPatch), nameof(GetFoodEitr));

                    il[i].opcode = OpCodes.Call;
                    il[i].operand = helper;
                }
            }

            return il.AsEnumerable();
        }
    }

}
