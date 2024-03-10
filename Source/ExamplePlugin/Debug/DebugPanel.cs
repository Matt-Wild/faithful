using UnityEngine;
using UnityEngine.EventSystems;

namespace Faithful
{
    internal class DebugPanel : MonoBehaviour, IDragHandler
    {
        // Store reference to Debug Controller
        protected DebugController debugController;

        // Store reference to canvas and rect transform
        protected Canvas canvas;
        protected RectTransform rectTransform;

        public void Init(DebugController _debugController)
        {
            // Assign debug controller
            debugController = _debugController;
        }

        public virtual void Awake()
        {
            // Add Debug Tab Buttons behaviour
            gameObject.AddComponent<DebugPanelButtons>();

            // Get canvas
            canvas = transform.parent.gameObject.GetComponent<Canvas>();

            // Get rect transform
            rectTransform = GetComponent<RectTransform>();
        }

        public void OnDrag(PointerEventData _eventData)
        {
            // Move panel
            rectTransform.anchoredPosition += _eventData.delta / canvas.scaleFactor;
        }
    }
}
