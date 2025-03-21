﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Faithful
{
    internal class DebugPanel : MonoBehaviour, IDragHandler
    {
        // Store reference to Debug Controller
        protected DebugController debugController;

        // Store reference to panel buttons
        protected DebugPanelButtons panelButtons;

        // Store reference to canvas and rect transform
        protected Canvas canvas;
        protected RectTransform rectTransform;

        // Store if panel is minimised
        public bool minimised = false;

        public virtual void Init(DebugController _debugController, bool _startOpen = false)
        {
            // Apply fonts based on GameObject name tags
            Utils.ApplyFonts(transform);

            // Assign debug controller
            debugController = _debugController;

            // Set active
            gameObject.SetActive(_startOpen);
        }

        public virtual void Awake()
        {
            // Add Debug Tab Buttons behaviour
            panelButtons = gameObject.AddComponent<DebugPanelButtons>();

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

        public void Open()
        {
            // Set active
            gameObject.SetActive(true);
        }

        public void Close()
        {
            // Set inactive
            gameObject.SetActive(false);
        }

        public void Maximise()
        {
            // Maximise panel
            panelButtons.MaximisePanel();
        }

        public void Minimise()
        {
            // Minimise panel
            panelButtons.MinimisePanel();
        }

        public virtual void OnMinimise()
        {
            // Set as minimised
            minimised = true;
        }

        public virtual void OnMaximise()
        {
            // Set as maximised
            minimised = false;
        }
    }
}
