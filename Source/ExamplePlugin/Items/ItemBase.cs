using System.Collections.Generic;

namespace Faithful
{
    internal class ItemBase
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store token
        protected string token;

        // Main item that behaviour revolves around
        private Item m_mainItem;
        public Item MainItem
        {
            get => m_mainItem;
            set
            {
                // Assign main item
                m_mainItem = value;
                
                // Link manual tokens callbacks
                m_mainItem.descriptionManualSettings = DescriptionManualTokens;
                m_mainItem.qualityDescriptionManualSettings = QualityDescriptionManualTokens;
            }
        }

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
                Log.Warning($"[{MainItem.name}] - Item missing 'QualityConstructor' method.");
            }
        }

        protected virtual void CreateSettings()
        {
            // Check if verbose console
            if (Utils.verboseConsole)
            {
                // Log warning
                Log.Warning($"[{MainItem.name}] - Item missing 'CreateSettings' method.");
            }
        }

        public virtual void FetchSettings()
        {
            // Check if verbose console
            if (Utils.verboseConsole)
            {
                // Log warning
                Log.Warning($"[{MainItem.name}] - Item missing 'FetchSettings' method.");
            }
        }

        public virtual Dictionary<string, string> DescriptionManualTokens()
        {
            // Assume no manual tokens
            return null;
        }

        public virtual Dictionary<string, string> QualityDescriptionManualTokens(Quality _quality)
        {
            // Assume no manual tokens
            return null;
        }

        public bool SupportsQuality
        {
            get
            {
                // Check for main item ref
                if (MainItem == null)
                {
                    Log.Warning($"[ITEM BASE] - Cannot return accurate 'SupportsQuality' value, main item is null!");
                    return false;
                }

                // Return if main item supports quality
                return MainItem.supportsQuality;
            }
        }
    }
}
