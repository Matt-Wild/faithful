using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Faithful
{
    // Identify generated rows so object searches do not find their own results
    internal class DebugGeneratedRow : MonoBehaviour { }

    internal class DebugScrollList : MonoBehaviour, IMoveHandler, ISubmitHandler, ICancelHandler
    {
        // Store references to the scroll view and row template
        UnityEngine.UI.ScrollRect scrollRect;
        RectTransform rowTemplate;
        RectTransform viewport;

        // Store the row setup and binding behaviours
        Action<GameObject> createRow;
        Action<GameObject, int> bindRow;

        // Store reusable rows and their current list indices
        List<RectTransform> rows = new List<RectTransform>();
        List<int> rowIndices = new List<int>();
        List<DebugScrollNavigation[]> rowControls = new List<DebugScrollNavigation[]>();

        // Store the full list size and fixed row height
        int itemCount;
        float rowHeight;

        // Remember keyboard focus when a selected row scrolls out of view
        int selectedIndex;
        string selectedControl;
        public Action onCancel;

        public void Init(UnityEngine.UI.ScrollRect _scrollRect, RectTransform _rowTemplate, Action<GameObject> _createRow, Action<GameObject, int> _bindRow)
        {
            // Assign references and behaviours
            scrollRect = _scrollRect;
            rowTemplate = _rowTemplate;
            viewport = scrollRect.viewport != null ? scrollRect.viewport : (RectTransform)scrollRect.transform;
            createRow = _createRow;
            bindRow = _bindRow;
            rowHeight = Mathf.Max(1.0f, rowTemplate.rect.height);

            // Position rows directly instead of laying out the entire result list
            UnityEngine.UI.LayoutGroup layout = scrollRect.content.GetComponent<UnityEngine.UI.LayoutGroup>();
            UnityEngine.UI.ContentSizeFitter fitter = scrollRect.content.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            if (layout != null) layout.enabled = false;
            if (fitter != null) fitter.enabled = false;
            scrollRect.content.anchorMin = new Vector2(0.0f, 1.0f);
            scrollRect.content.anchorMax = new Vector2(1.0f, 1.0f);
            scrollRect.content.pivot = new Vector2(0.5f, 1.0f);
            scrollRect.content.sizeDelta = Vector2.zero;

            // Refresh visible rows when scrolling
            scrollRect.onValueChanged.AddListener(OnScroll);
        }

        public void SetItemCount(int _count, bool _resetPosition = false)
        {
            // Size the content for every result, including rows outside the viewport
            itemCount = _count;
            scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemCount * rowHeight);

            // Reset scrolling when replacing the list
            if (_resetPosition)
            {
                scrollRect.StopMovement();
                scrollRect.content.anchoredPosition = Vector2.zero;
                for (int i = 0; i < rowIndices.Count; i++)
                {
                    ReleaseRow(i);
                    rowIndices[i] = -1;
                }
                selectedIndex = 0;
            }

            // Update the visible portion of the list
            RefreshRows();
        }

        public void RefreshItems(int _count, bool _resetPosition = false)
        {
            // Release controls before replacing the data represented by existing rows
            for (int i = 0; i < rowIndices.Count; i++)
            {
                if (rowIndices[i] != -1) ReleaseRow(i);
                rowIndices[i] = -1;
            }

            // Rebind visible rows while retaining the scroll position unless requested
            SetItemCount(_count, _resetPosition);
        }

        private void LateUpdate()
        {
            // Also handle changes to the viewport size
            if (scrollRect != null) RefreshRows();
        }

        private void OnScroll(Vector2 _position)
        {
            // Update rows after scrolling
            RefreshRows();
        }

        private void RefreshRows()
        {
            // Only create enough rows to cover the viewport
            int visibleCount = DebugListRange.GetVisibleCount(itemCount, viewport.rect.height, rowHeight);
            int firstIndex = DebugListRange.GetFirstIndex(itemCount, visibleCount, scrollRect.content.anchoredPosition.y, rowHeight);
            while (rows.Count < visibleCount)
            {
                // Create and initialise a reusable row
                GameObject row = Instantiate(rowTemplate.gameObject, scrollRect.content, false);
                row.SetActive(false);
                row.AddComponent<DebugGeneratedRow>();
                createRow(row);
                rows.Add((RectTransform)row.transform);
                rowIndices.Add(-1);
                rowControls.Add(row.GetComponentsInChildren<DebugScrollNavigation>(true));
            }

            // Bind only rows whose contents have changed
            for (int i = 0; i < rows.Count; i++)
            {
                RectTransform row = rows[i];
                if (i >= visibleCount)
                {
                    if (rowIndices[i] != -1) ReleaseRow(i);
                    row.gameObject.SetActive(false);
                    rowIndices[i] = -1;
                    continue;
                }

                // Reuse rows in a ring so scrolling one entry only changes one row
                int index = DebugListRange.GetRowIndex(i, firstIndex, visibleCount);
                if (rowIndices[i] != index)
                {
                    // Release input before a control represents a different entry
                    ReleaseRow(i);

                    rowIndices[i] = index;
                    row.anchorMin = new Vector2(0.0f, 1.0f);
                    row.anchorMax = new Vector2(1.0f, 1.0f);
                    row.pivot = new Vector2(0.5f, 1.0f);
                    row.sizeDelta = new Vector2(0.0f, rowHeight);
                    row.anchoredPosition = new Vector2(0.0f, -index * rowHeight);
                    bindRow(row.gameObject, index);
                }

                row.gameObject.SetActive(true);
            }
        }

        private void ReleaseRow(int _slot)
        {
            // A press that started on the old result must not act on the new one
            foreach (DebugScrollNavigation control in rowControls[_slot]) control.CancelPress();

            // Keep keyboard focus associated with its original result
            GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (selected != null && selected.transform.IsChildOf(rows[_slot]))
            {
                selectedIndex = Mathf.Max(0, rowIndices[_slot]);
                selectedControl = selected.name;
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }

        public GameObject ShowItem(int _index)
        {
            // Check the requested index
            if (_index < 0 || _index >= itemCount) return null;

            // Scroll the requested row into view
            scrollRect.StopMovement();
            Vector2 position = scrollRect.content.anchoredPosition;
            position.y = DebugListRange.GetScrollOffset(_index, itemCount, position.y, viewport.rect.height, rowHeight);
            scrollRect.content.anchoredPosition = position;
            RefreshRows();

            // Return the row now displaying this entry
            for (int i = 0; i < rows.Count; i++)
            {
                if (rowIndices[i] == _index) return rows[i].gameObject;
            }

            return null;
        }

        public void AddNavigation(GameObject _row)
        {
            // Handle navigation by list index so it also reaches rows outside the viewport
            foreach (UnityEngine.UI.Selectable control in _row.GetComponentsInChildren<UnityEngine.UI.Selectable>(true))
            {
                UnityEngine.UI.Navigation navigation = control.navigation;
                navigation.mode = UnityEngine.UI.Navigation.Mode.None;
                control.navigation = navigation;
                control.gameObject.AddComponent<DebugScrollNavigation>().Init(this, _row, control);
            }
        }

        public void MoveSelection(GameObject _row, UnityEngine.UI.Selectable _control, AxisEventData _eventData)
        {
            // Remember the selected result and control
            int slot = rows.IndexOf((RectTransform)_row.transform);
            if (slot < 0) return;
            selectedIndex = rowIndices[slot];
            selectedControl = _control.name;

            // Navigate through the complete list
            OnMove(_eventData);
        }

        public void OnMove(AxisEventData _eventData)
        {
            // Find the selected row and its available controls
            GameObject row = ShowItem(selectedIndex);
            if (row == null) return;
            UnityEngine.UI.Selectable[] controls = row.GetComponentsInChildren<UnityEngine.UI.Selectable>();

            // Move between controls within a row, or between results
            int direction = _eventData.moveDir == MoveDirection.Up || _eventData.moveDir == MoveDirection.Left ? -1 : 1;
            if (_eventData.moveDir == MoveDirection.None) return;
            if (controls.Length > 1 && (_eventData.moveDir == MoveDirection.Left || _eventData.moveDir == MoveDirection.Right))
            {
                // Follow the visual button order rather than the prefab's sibling order
                Array.Sort(controls, (left, right) => left.transform.localPosition.x.CompareTo(right.transform.localPosition.x));
                int controlIndex = Array.FindIndex(controls, control => control.name == selectedControl);
                controls[Mathf.Clamp(controlIndex + direction, 0, controls.Length - 1)].Select();
            }
            else
            {
                selectedIndex = Mathf.Clamp(selectedIndex + direction, 0, itemCount - 1);
                SelectControl();
            }

            _eventData.Use();
        }

        private UnityEngine.UI.Selectable SelectControl()
        {
            // Restore focus to the same button on the requested row
            GameObject row = ShowItem(selectedIndex);
            if (row == null) return null;
            UnityEngine.UI.Selectable[] controls = row.GetComponentsInChildren<UnityEngine.UI.Selectable>();
            UnityEngine.UI.Selectable control = Array.Find(controls, candidate => candidate.name == selectedControl);
            if (control == null && controls.Length > 0) control = controls[0];
            if (control != null) control.Select();
            else if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(gameObject);
            return control;
        }

        public void OnSubmit(BaseEventData _eventData)
        {
            // Bring an offscreen selection back into view before activating it
            UnityEngine.UI.Selectable control = SelectControl();
            if (control != null) ExecuteEvents.Execute(control.gameObject, _eventData, ExecuteEvents.submitHandler);
        }

        public void OnCancel(BaseEventData _eventData)
        {
            // Allow dropdown lists to close from any selected row
            onCancel?.Invoke();
        }

        private void OnDestroy()
        {
            // Remove the scroll listener
            if (scrollRect != null) scrollRect.onValueChanged.RemoveListener(OnScroll);
        }
    }

    internal class DebugScrollNavigation : MonoBehaviour, IMoveHandler, ICancelHandler, IPointerDownHandler, IPointerUpHandler
    {
        // Store the owning list, row and selectable control
        DebugScrollList scrollList;
        GameObject row;
        UnityEngine.UI.Selectable control;

        // Store a pending pointer press while the row remains bound to the same result
        PointerEventData pointerPress;

        public void Init(DebugScrollList _scrollList, GameObject _row, UnityEngine.UI.Selectable _control)
        {
            scrollList = _scrollList;
            row = _row;
            control = _control;
        }

        public void OnMove(AxisEventData _eventData)
        {
            scrollList.MoveSelection(row, control, _eventData);
        }

        public void OnCancel(BaseEventData _eventData)
        {
            scrollList.OnCancel(_eventData);
        }

        public void OnPointerDown(PointerEventData _eventData)
        {
            pointerPress = _eventData;
        }

        public void OnPointerUp(PointerEventData _eventData)
        {
            pointerPress = null;
        }

        public void CancelPress()
        {
            // Prevent a recycled delete, enable or selection button from receiving an old click
            if (pointerPress != null) pointerPress.eligibleForClick = false;
            pointerPress = null;
        }

        private void OnDisable()
        {
            CancelPress();
        }
    }
}
