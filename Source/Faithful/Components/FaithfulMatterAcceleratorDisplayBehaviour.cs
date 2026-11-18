using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class FaithfulMatterAcceleratorDisplayBehaviour : MonoBehaviour
    {
        // Cached display references
        CharacterBody body;
        HealthComponent healthComponent;
        GameObject normalMesh;
        GameObject voidMesh;

        void Start()
        {
            CharacterModel model = GetComponentInParent<CharacterModel>();
            body = model ? model.body : null;
            if (!body) return;

            healthComponent = body.healthComponent;
            normalMesh = transform.Find("MatterAcceleratorDisplay/MatterAcceleratorDisplayMesh")?.gameObject;
            voidMesh = transform.Find("MatterAcceleratorDisplayVoid/MatterAcceleratorDisplayMesh")?.gameObject;
            if (!normalMesh || !voidMesh) return;

            // Let the base game initialise visibility for both renderers
            GetComponent<ItemDisplay>()?.RefreshRenderers();

            body.onInventoryChanged += UpdateDisplay;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (!healthComponent || !normalMesh || !voidMesh) return;

            bool hasVoidShields = healthComponent.GetHealthBarValues().hasVoidShields;
            if (normalMesh.activeSelf == hasVoidShields) normalMesh.SetActive(!hasVoidShields);
            if (voidMesh.activeSelf != hasVoidShields) voidMesh.SetActive(hasVoidShields);
        }

        void OnDestroy()
        {
            if (body) body.onInventoryChanged -= UpdateDisplay;
        }
    }
}
