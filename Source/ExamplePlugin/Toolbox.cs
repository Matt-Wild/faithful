using BepInEx;
using IL.RoR2.Items;
using System.Diagnostics;

namespace Faithful
{
    internal class Toolbox
    {
        // Tool references
        public Utils utils;
        public Behaviour behaviour;
        public Assets assets;
        public Items items;
        public Buffs buffs;

        // Constructor
        public Toolbox(PluginInfo _pluginInfo)
        {
            // Create tools
            utils = new Utils(this, _pluginInfo);
            behaviour = new Behaviour(this);
            assets = new Assets(this);
            items = new Items(this);
            buffs = new Buffs(this);

            On.RoR2.Items.ContagiousItemManager.Init += AddItemCorruptions;

            Log.Debug("Toolbox built");
        }

        public void Update()
        {
            // Update tools
            behaviour.Update();
        }

        public void FixedUpdate()
        {
            // Update tools
            behaviour.FixedUpdate();
        }

        private void AddItemCorruptions(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            // Add item corruptions
            items.AddItemCorruptions();

            Log.Debug("Added item corruptions");

            orig(); // Run normal processes
        }
    }
}
