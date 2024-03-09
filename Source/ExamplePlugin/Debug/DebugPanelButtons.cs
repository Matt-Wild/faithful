using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Faithful
{
    internal class DebugPanelButtons : MonoBehaviour
    {
        // Store reference to menu panel tranform
        protected RectTransform panel;

        // Store reference to panel button game objects
        protected GameObject tabButton;

        // Store reference to child objects
        protected List<GameObject> children = new List<GameObject>();

        // Store menu panel heights
        protected float openedHeight;
        protected float closedHeight;

        void Awake()
        {
            // Get menu panel transform from parent
            panel = GetComponent<RectTransform>();

            // Get panel buttons
            tabButton = transform.Find("TabPanelButton").gameObject;

            // Get menu panel opened height
            openedHeight = panel.sizeDelta.y;

            // Get rect transform of tab button
            RectTransform tabButtonTransform = tabButton.GetComponent<RectTransform>();

            // Calculate menu panel closed height
            closedHeight = tabButtonTransform.sizeDelta.y + tabButtonTransform.localPosition.y * -2.0f;

            // Get tab button
            Button tabButtonComponent = tabButton.GetComponent<Button>();

            // Add tab button behaviour
            tabButtonComponent.onClick.AddListener(OnTabButtonClicked);

            // Cycle through all children in menu panel
            foreach (Transform child in transform)
            {
                // Avoid menu panel title and tab buttons
                if (child.gameObject.name == "PanelTitle" || child.gameObject.name.Contains("PanelButton"))
                {
                    // Skip
                    continue;
                }

                // Add child game object to children list
                children.Add(child.gameObject);
            }
        }

        protected void FixedUpdate()
        {
            // Hide tab buttons if cursor is locked (in gameplay)
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                // Hide buttons
                tabButton.SetActive(false);
            }
            else
            {
                // Show buttons
                tabButton.SetActive(true);
            }
        }

        public void OnTabButtonClicked()
        {
            // Check if menu panel is closed
            if (panel.sizeDelta.y <= closedHeight)
            {
                // Open menu panel
                panel.sizeDelta = new Vector2(panel.sizeDelta.x, openedHeight);

                // Enable children
                SetChildrenActive(true);
            }
            else
            {
                // Close menu panel
                panel.sizeDelta = new Vector2(panel.sizeDelta.x, closedHeight);

                // Disable children
                SetChildrenActive(false);
            }
        }

        protected void SetChildrenActive(bool _active)
        {
            // Cycle through all children in menu panel
            foreach (GameObject child in children)
            {
                // Set child active
                child.SetActive(_active);
            }
        }
    }
}
