using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Faithful
{
    internal class DebugPanelButtons : MonoBehaviour
    {
        // Store reference to debug panel
        protected DebugPanel panel;

        // Store reference to menu panel tranform
        protected RectTransform panelRect;

        // Store reference to panel toggle
        protected DebugPanelToggle toggle;

        // Store reference to panel button game objects
        protected GameObject tabButton;
        protected GameObject tabButtonOpen;
        protected GameObject tabButtonClose;
        protected GameObject closeButton;

        // Store reference to child objects
        protected List<GameObject> children = new List<GameObject>();

        // Store menu panel heights
        protected float openedHeight;
        protected float closedHeight;

        void Awake()
        {
            // Get debug panel behaviour
            panel = GetComponent<DebugPanel>();

            // Get menu panel transform from parent
            panelRect = GetComponent<RectTransform>();

            // Get tab panel buttons
            tabButton = transform.Find("TabPanelButton").gameObject;
            tabButtonOpen = tabButton.transform.Find("Open").gameObject;
            tabButtonClose = tabButton.transform.Find("Close").gameObject;
            closeButton = transform.Find("ClosePanelButton") != null ? transform.Find("ClosePanelButton").gameObject : null;

            // Get menu panel opened height
            openedHeight = panelRect.sizeDelta.y;

            // Get rect transform of tab button
            RectTransform tabButtonTransform = tabButton.GetComponent<RectTransform>();

            // Calculate menu panel closed height
            closedHeight = tabButtonTransform.sizeDelta.y + tabButtonTransform.localPosition.y * -2.0f;

            // Get tab button
            Button tabButtonComponent = tabButton.GetComponent<Button>();

            // Add tab button behaviour
            tabButtonComponent.onClick.AddListener(OnTabButtonClicked);

            // Check for close button
            if (closeButton != null)
            {
                // Get close button
                Button closeButtonComponent = closeButton.GetComponent<Button>();

                // Add close button behaviour
                closeButtonComponent.onClick.AddListener(OnCloseButtonClicked);
            }

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

                // Check for close button
                if (closeButton != null)
                {
                    // Hide close button
                    closeButton.SetActive(false);
                }
            }
            else
            {
                // Show buttons
                tabButton.SetActive(true);

                // Check for close button
                if (closeButton != null)
                {
                    // Show close button
                    closeButton.SetActive(true);
                }
            }
        }

        public void AssignPanelToggle(DebugPanelToggle _toggle)
        {
            // Assign panel toggle
            toggle = _toggle;
        }

        public void OnTabButtonClicked()
        {
            // Check if menu panel is closed
            if (panelRect.sizeDelta.y <= closedHeight)
            {
                // Maximise panel
                MaximisePanel();
            }
            else
            {
                // Minimise panel
                MinimisePanel();
            }
        }

        public void OnCloseButtonClicked()
        {
            // Check for panel toggle
            if (toggle != null)
            {
                // Close panel
                toggle.SetState(false);
                return;
            }

            // Close panel
            panel.Close();
        }

        public void MaximisePanel()
        {
            // Open menu panel
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, openedHeight);

            // Enable children
            SetChildrenActive(true);

            // Set tab button sprite
            tabButtonOpen.SetActive(false);
            tabButtonClose.SetActive(true);

            // Tell panel it is maximised
            panel.OnMaximise();
        }

        public void MinimisePanel()
        {
            // Close menu panel
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, closedHeight);

            // Disable children
            SetChildrenActive(false);

            // Set tab button sprite
            tabButtonOpen.SetActive(true);
            tabButtonClose.SetActive(false);

            // Tell panel it is minimised
            panel.OnMinimise();
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
