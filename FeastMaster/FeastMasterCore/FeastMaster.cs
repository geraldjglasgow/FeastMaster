using System.IO;
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
        private const string PluginVersion = "3.2.0";

        public static ManualLogSource Log { get; private set; }

        private readonly ConfigSync _configSync = new ConfigSync(PluginGuid)
        {
            DisplayName = PluginName,
            CurrentVersion = PluginVersion,
            MinimumRequiredVersion = PluginVersion
        };

        private FileSystemWatcher _configWatcher;

        private void Awake()
        {
            Log = Logger;

            FeastMasterData.Initialize(Config, _configSync);
            PrefabManager.OnPrefabsRegistered += FeastMasterData.LoadConfigurations;

            new Harmony(PluginGuid).PatchAll(Assembly.GetExecutingAssembly());

            SetupConfigWatcher();
        }

        private void SetupConfigWatcher()
        {
            var configPath = Config.ConfigFilePath;
            var configDir = Path.GetDirectoryName(configPath);
            var configFile = Path.GetFileName(configPath);

            _configWatcher = new FileSystemWatcher(configDir, configFile)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _configWatcher.Changed += OnConfigFileChanged;
        }

        private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            Log.LogInfo("Config file changed, reloading...");
            Config.Reload();
        }

        private void OnDestroy()
        {
            _configWatcher?.Dispose();
        }
    }
}
