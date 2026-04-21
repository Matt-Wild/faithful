namespace Faithful
{
    internal class ItemBase
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store token
        protected string token;

        // Main item that behaviour revolves around
        public Item mainItem;

        // Constructor
        public ItemBase(Toolbox _toolbox, string _token)
        {
            // Assign toolbox
            toolbox = _toolbox;

            // Assign token
            token = _token;

            // Register with utils
            Utils.RegisterItem(_token, this);
        }

        public virtual void QualityConstructor()
        {
            // If should have quality behaviour and main item supports quality, log warning (if verbose)
            if (SupportsQuality && Utils.qualityEnabled && Utils.verboseConsole)
            {
                // Log warning
                Log.Warning($"[{mainItem.name}] - Item missing 'QualityConstructor' method.");
            }
        }

        protected virtual void CreateSettings()
        {
            // Check if verbose console
            if (Utils.verboseConsole)
            {
                // Log warning
                Log.Warning($"[{mainItem.name}] - Item missing 'CreateSettings' method.");
            }
        }

        public virtual void FetchSettings()
        {
            // Check if verbose console
            if (Utils.verboseConsole)
            {
                // Log warning
                Log.Warning($"[{mainItem.name}] - Item missing 'FetchSettings' method.");
            }
        }

        public bool SupportsQuality
        {
            get
            {
                // Check for main item ref
                if (mainItem == null)
                {
                    Log.Warning($"[ITEM BASE] - Cannot return accurate 'SupportsQuality' value, main item is null!");
                    return false;
                }

                // Return if main item supports quality
                return mainItem.supportsQuality;
            }
        }
    }
}
