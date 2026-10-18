using RoR2;

namespace Faithful
{
    internal class DebugItem : ItemBase
    {
        // Constructor
        public DebugItem(Toolbox _toolbox) : base(_toolbox, "DEBUG_ITEM")
        {
            // Create item
            MainItem = Items.AddItem(new ItemOptions(
                token: token,
                safeName: "Debug Item",
                tags: [ItemTag.Any],
                iconName: "texTemporalCubeIcon",
                modelName: "Cube_TemporaryAsset")
            {
                Tier = ItemTier.NoTier,
                SimulacrumBanned = true,
                Hidden = true,
                DebugOnly = true
            });
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
