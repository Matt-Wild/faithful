using UnityEngine.UI;

namespace Faithful
{
    internal class DebugStageControls : DebugPanel
    {
        // Store reference to skip stage
        protected Button skipButton;

        // Store reference to kill enemies stage
        protected Button killEnemiesButton;

        public override void Awake()
        {
            // Call base class Awake
            base.Awake();

            // Find skip stage button
            skipButton = transform.Find("SkipButton").gameObject.GetComponent<Button>();

            // Find kill enemies button
            killEnemiesButton = transform.Find("KillEnemiesButton").gameObject.GetComponent<Button>();

            // Add on skip stage behaviour
            skipButton.onClick.AddListener(OnSkipStage);

            // Add on kill enemies behaviour
            killEnemiesButton.onClick.AddListener(OnKillEnemies);
        }

        protected void OnSkipStage()
        {
            // Teleport to next stage
            Utils.TeleportToNextStage();
        }

        protected void OnKillEnemies()
        {
            // Kill all enemies
            Utils.KillAllEnemies();
        }
    }
}
