using RoR2;

namespace Faithful
{
    internal class DebugItem : ItemBase
    {
        // Store item
        Item item;

        // Constructor
        public DebugItem(Toolbox _toolbox) : base(_toolbox)
        {
            // Create item
            item = Items.AddItem("DEBUG_ITEM", "Debug Item", [ItemTag.Any], "texTemporalCubeIcon", "Cube_TemporaryAsset", ItemTier.NoTier, _simulacrumBanned: true, _hidden: true, _debugOnly: true);
        }
    }
}
