using BepInEx;
using IL.RoR2.Items;
using System.Diagnostics;

namespace Faithful
{
    internal class Toolbox
    {
        // Tool references
        public Config config;
        public Utils utils;
        public DebugMode debugMode;
        public Behaviour behaviour;
        public Assets assets;
        public Items items;
        public Buffs buffs;

        // Constructor
        public Toolbox(PluginInfo _pluginInfo)
        {
            // Create tools
            config = new Config(this, _pluginInfo);
            utils = new Utils(this, _pluginInfo);
            debugMode = new DebugMode(this);
            behaviour = new Behaviour(this);
            assets = new Assets(this);
            items = new Items(this);
            buffs = new Buffs(this);

            Log.Debug("Toolbox built");
        }

        public void Update()
        {
            // Update tools
            debugMode.Update();
            behaviour.Update();
        }

        public void FixedUpdate()
        {
            // Update tools
            behaviour.FixedUpdate();
        }
    }
}
