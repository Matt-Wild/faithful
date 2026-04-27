using RoR2;

namespace Faithful
{
    internal class DebugItem : ItemBase
    {
        // Constructor
        public DebugItem(Toolbox _toolbox) : base(_toolbox, "DEBUG_ITEM")
        {
            // Create item
            MainItem = Items.AddItem(token, "Debug Item", [ItemTag.Any], "texTemporalCubeIcon", "Cube_TemporaryAsset", ItemTier.NoTier, _simulacrumBanned: true, _hidden: true, _debugOnly: true);
        }

        protected override void CreateSettings()
        {
        }

        public override void FetchSettings()
        {
            // Update item texts with new settings
            MainItem.UpdateItemTexts();
        }
    }
}
