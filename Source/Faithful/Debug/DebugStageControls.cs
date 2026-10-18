using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Faithful
{
    internal class DebugStageControls : DebugPanel
    {
        // Text that displays the name of the stage
        protected TextMeshProUGUI stageNameText;

        // Store reference to skip stage
        protected Button skipButton;

        // Store reference to kill enemies stage
        protected Button killEnemiesButton;

        public override void Awake()
        {
            // Call base class Awake
            base.Awake();

            // Get stage name text
            stageNameText = Utils.FindChildWithTerm(transform, "StageName").GetComponent<TextMeshProUGUI>();

            // Find skip stage button
            skipButton = transform.Find("SkipButton").gameObject.GetComponent<Button>();

            // Find kill enemies button
            killEnemiesButton = transform.Find("KillEnemiesButton").gameObject.GetComponent<Button>();

            // Add on skip stage behaviour
            skipButton.onClick.AddListener(OnSkipStage);

            // Add on kill enemies behaviour
            killEnemiesButton.onClick.AddListener(OnKillEnemies);
        }

        private void FixedUpdate()
        {
            // Update stage name text
            stageNameText.text = $"Scene Name: {SceneManager.GetActiveScene().name}";
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
