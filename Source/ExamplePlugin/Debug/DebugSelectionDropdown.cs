using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Faithful
{
    internal class DebugSelectionDropdown : TMP_Dropdown
    {
        // Store the reusable dropdown list and click blocker
        GameObject dropdownList;
        GameObject blocker;
        DebugScrollList scrollList;
        RectTransform itemTemplate;
        Canvas rootCanvas;
        Canvas listCanvas;
        Canvas blockerCanvas;

        public static TMP_Dropdown Create(TMP_Dropdown _dropdown)
        {
            // Copy the dropdown settings before replacing only its behaviour
            GameObject owner = _dropdown.gameObject;
            RectTransform template = _dropdown.template;
            TMP_Text captionText = _dropdown.captionText;
            TMP_Text itemText = _dropdown.itemText;
            UnityEngine.UI.Image captionImage = _dropdown.captionImage;
            UnityEngine.UI.Image itemImage = _dropdown.itemImage;
            UnityEngine.UI.Graphic targetGraphic = _dropdown.targetGraphic;
            UnityEngine.UI.Navigation navigation = _dropdown.navigation;
            UnityEngine.UI.Selectable.Transition transition = _dropdown.transition;
            UnityEngine.UI.ColorBlock colors = _dropdown.colors;
            UnityEngine.UI.SpriteState spriteState = _dropdown.spriteState;
            UnityEngine.UI.AnimationTriggers animationTriggers = _dropdown.animationTriggers;
            var options = _dropdown.options;
            var onValueChanged = _dropdown.onValueChanged;
            int value = _dropdown.value;
            bool interactable = _dropdown.interactable;

            // Keep the existing template, labels and option data
            DestroyImmediate(_dropdown);
            DebugSelectionDropdown dropdown = owner.AddComponent<DebugSelectionDropdown>();
            dropdown.template = template;
            dropdown.captionText = captionText;
            dropdown.itemText = itemText;
            dropdown.captionImage = captionImage;
            dropdown.itemImage = itemImage;
            dropdown.targetGraphic = targetGraphic;
            dropdown.navigation = navigation;
            dropdown.transition = transition;
            dropdown.colors = colors;
            dropdown.spriteState = spriteState;
            dropdown.animationTriggers = animationTriggers;
            dropdown.options = options;
            dropdown.onValueChanged = onValueChanged;
            dropdown.interactable = interactable;
            dropdown.SetValueWithoutNotify(value);
            dropdown.RefreshShownValue();
            return dropdown;
        }

        public override void OnPointerClick(PointerEventData _eventData)
        {
            // Open the selection list when clicked
            if (_eventData.button == PointerEventData.InputButton.Left) ShowList();
        }

        public override void OnSubmit(BaseEventData _eventData)
        {
            // Open the selection list from keyboard or controller input
            ShowList();
        }

        public override void OnCancel(BaseEventData _eventData)
        {
            // Close the list without changing the current selection
            HideList();
        }

        private void ShowList()
        {
            // Check if the dropdown can be opened
            if (!IsActive() || !IsInteractable() || options.Count == 0) return;
            if (dropdownList != null && dropdownList.activeSelf) return;

            // Create the reusable list on first use
            if (dropdownList == null) CreateList();
            if (dropdownList == null) return;

            // Restore the template position before checking which side has room
            RectTransform listRect = (RectTransform)dropdownList.transform;
            listRect.anchorMin = template.anchorMin;
            listRect.anchorMax = template.anchorMax;
            listRect.pivot = template.pivot;
            listRect.anchoredPosition = template.anchoredPosition;
            listRect.sizeDelta = template.sizeDelta;
            listRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Min(template.rect.height, options.Count * itemTemplate.rect.height));
            dropdownList.SetActive(true);
            blocker.SetActive(true);

            // Apply sorting after activation so the popup draws and receives clicks above the blocker
            listCanvas.overrideSorting = true;
            listCanvas.sortingLayerID = rootCanvas.sortingLayerID;
            listCanvas.sortingOrder = 30000;
            blockerCanvas.overrideSorting = true;
            blockerCanvas.sortingLayerID = listCanvas.sortingLayerID;
            blockerCanvas.sortingOrder = listCanvas.sortingOrder - 1;

            // Keep the dropdown inside the root canvas
            Vector3[] corners = new Vector3[4];
            listRect.GetWorldCorners(corners);
            RectTransform canvasRect = (RectTransform)rootCanvas.transform;
            for (int axis = 0; axis < 2; axis++)
            {
                foreach (Vector3 worldCorner in corners)
                {
                    Vector3 corner = canvasRect.InverseTransformPoint(worldCorner);
                    if (corner[axis] < canvasRect.rect.min[axis] || corner[axis] > canvasRect.rect.max[axis])
                    {
                        RectTransformUtility.FlipLayoutOnAxis(listRect, axis, false, false);
                        break;
                    }
                }
            }

            // Display the complete list and scroll to the current selection
            scrollList.SetItemCount(options.Count, true);
            GameObject selectedRow = scrollList.ShowItem(value);
            if (selectedRow != null) selectedRow.GetComponent<UnityEngine.UI.Toggle>().Select();
        }

        private void CreateList()
        {
            // Find the root canvas
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null || template == null) return;
            rootCanvas = canvas.rootCanvas;

            // Reuse the existing dropdown template
            dropdownList = Instantiate(template.gameObject, template.parent, false);
            dropdownList.name = "Dropdown List";
            UnityEngine.UI.ScrollRect scrollRect = dropdownList.GetComponent<UnityEngine.UI.ScrollRect>();
            itemTemplate = (RectTransform)scrollRect.content.Find("Item");
            itemTemplate.gameObject.SetActive(false);

            // Give the popup its own canvas and raycaster
            listCanvas = dropdownList.AddComponent<Canvas>();
            dropdownList.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Reuse only the visible option rows
            scrollList = dropdownList.AddComponent<DebugScrollList>();
            scrollList.Init(scrollRect, itemTemplate, CreateRow, BindRow);
            scrollList.onCancel = HideList;

            // Block clicks outside the list until it is closed
            blocker = new GameObject("Dropdown Blocker", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.GraphicRaycaster), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
            RectTransform blockerRect = (RectTransform)blocker.transform;
            blockerRect.SetParent(rootCanvas.transform, false);
            blockerRect.anchorMin = Vector2.zero;
            blockerRect.anchorMax = Vector2.one;
            blockerRect.sizeDelta = Vector2.zero;
            blockerCanvas = blocker.GetComponent<Canvas>();
            blocker.GetComponent<UnityEngine.UI.Image>().color = Color.clear;
            UnityEngine.UI.Button blockerButton = blocker.GetComponent<UnityEngine.UI.Button>();
            UnityEngine.UI.Navigation navigation = blockerButton.navigation;
            navigation.mode = UnityEngine.UI.Navigation.Mode.None;
            blockerButton.navigation = navigation;
            blockerButton.onClick.AddListener(HideList);
        }

        private void CreateRow(GameObject _row)
        {
            // Set up option selection and navigation once for each reusable row
            _row.AddComponent<DebugSelectionDropdownEntry>().Init(this);
            scrollList.AddNavigation(_row);
        }

        private void BindRow(GameObject _row, int _index)
        {
            // Display the option associated with this row
            _row.GetComponent<DebugSelectionDropdownEntry>().Bind(_index, options[_index], value == _index);
        }

        public void SelectOption(int _index)
        {
            // Keep the usual dropdown value and change notification behaviour
            value = _index;
            HideList();
        }

        private void HideList()
        {
            // Keep rows for the next opening
            if (dropdownList != null) dropdownList.SetActive(false);
            if (blocker != null) blocker.SetActive(false);
            if (IsActive()) Select();
        }

        public override void OnDisable()
        {
            // Close the popup when changing category or closing the debug panel
            HideList();
            base.OnDisable();
        }

        public override void OnDestroy()
        {
            // The blocker belongs to the root canvas rather than this dropdown
            if (blocker != null) Destroy(blocker);
            base.OnDestroy();
        }
    }

    internal class DebugSelectionDropdownEntry : MonoBehaviour, IPointerEnterHandler
    {
        // Store the owner, option index and existing row controls
        DebugSelectionDropdown dropdown;
        int index;
        UnityEngine.UI.Toggle toggle;
        TMP_Text title;

        public void Init(DebugSelectionDropdown _dropdown)
        {
            // Cache controls and install the selection listener once
            dropdown = _dropdown;
            toggle = GetComponent<UnityEngine.UI.Toggle>();
            title = GetComponentInChildren<TMP_Text>(true);
            toggle.onValueChanged.AddListener(OnSelected);
        }

        public void Bind(int _index, TMP_Dropdown.OptionData _option, bool _selected)
        {
            // Update the option without firing a selection event
            index = _index;
            title.text = _option.text;
            toggle.SetIsOnWithoutNotify(_selected);
        }

        private void OnSelected(bool _selected)
        {
            // Clicking the already selected option also closes the dropdown
            dropdown.SelectOption(index);
        }

        public void OnPointerEnter(PointerEventData _eventData)
        {
            // Match the usual dropdown hover selection behaviour
            toggle.Select();
        }
    }
}
