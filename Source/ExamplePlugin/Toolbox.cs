using BepInEx;
using BepInEx.Configuration;
using IL.RoR2.Items;
using System.Diagnostics;

namespace Faithful
{
    internal class Toolbox
    {
        // Store reference to plugin
        public Faithful plugin;

        // Tool references
        public DebugMode debugMode;

        // Constructor
        public Toolbox(Faithful _plugin, PluginInfo _pluginInfo, ConfigFile _configFile)
        {
            // Assign reference to plugin
            plugin = _plugin;

            // Create tools
            Config.Init(_configFile);
            Utils.Init(_pluginInfo);
            debugMode = new DebugMode();
            Behaviour.Init();
            Assets.Init();
            Items.Init();
            Buffs.Init();

            // Create expansion definition
            Utils.CreateExpansionDef();

            Log.Debug("Toolbox built");
        }

        public void Start()
        {
            // Update tools
            Utils.Start();
        }

        public void Update()
        {
            // Update tools
            debugMode.Update();
            Behaviour.Update();
        }

        public void FixedUpdate()
        {
            // Update tools
            Behaviour.FixedUpdate();
        }
    }
}
