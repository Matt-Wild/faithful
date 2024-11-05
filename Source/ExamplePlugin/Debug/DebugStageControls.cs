using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

namespace Faithful
{
    internal class DebugStageControls : DebugPanel
    {
        // Store reference to skip stage
        protected Button skipButton;

        public override void Awake()
        {
            // Call base class Awake
            base.Awake();

            // Find skip stage button
            skipButton = transform.Find("SkipButton").gameObject.GetComponent<Button>();

            // Add on skip stage behaviour
            skipButton.onClick.AddListener(OnSkipStage);
        }

        protected void OnSkipStage()
        {
            // Teleport to next stage
            Utils.TeleportToNextStage();
        }
    }
}
