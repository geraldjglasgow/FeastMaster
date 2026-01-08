using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn;
using Jotunn.Managers;
using ServerSync;

namespace FeastMaster
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(Main.ModGuid)]
    public class FeastMaster : BaseUnityPlugin
    {
        private const string PluginGuid = "com.FeastMaster";
        private const string PluginName = "FeastMaster";
        private const string PluginVersion = "2.3.1";

        public static ManualLogSource Log { get; private set; }

        private readonly ConfigSync _configSync = new ConfigSync(PluginGuid)
        {
            DisplayName = PluginName,
            CurrentVersion = PluginVersion,
            MinimumRequiredVersion = PluginVersion
        };

        private void Awake()
        {
            Log = Logger;

            FeastMasterData.Initialize(Config, _configSync);
            // Use OnPrefabsRegistered to ensure all mod-added items are available
            PrefabManager.OnPrefabsRegistered += FeastMasterData.LoadConfigurations;

            new Harmony(PluginGuid).PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
