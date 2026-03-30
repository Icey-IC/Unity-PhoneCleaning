using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PhotoItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public bool canDelete = true;
    public GameObject deleteSelect;
    public GameObject waitBox;

    private bool isSelected = false;
    private bool isPointerDown = false;
    private float pressTime = 0f;
    private float longPressTime = 0.5f;
    private bool longPressTriggered = false;

    void Start()
    {
        deleteSelect.SetActive(false);
        waitBox.SetActive(false);
    }

    void Update()
    {
        if (isPointerDown)
        {
            pressTime += Time.deltaTime;
            if (pressTime >= longPressTime && !longPressTriggered)
            {
                longPressTriggered = true;
                isPointerDown = false;
                GalleryManager.Instance.EnterMultiSelect();
                SelectThisPhoto();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown´¥·¢ÁË");
        isPointerDown = true;
        pressTime = 0f;
        longPressTriggered = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (longPressTriggered)
        {
            longPressTriggered = false;
            return;
        }

        if (GalleryManager.Instance.multiMode)
        {
            ToggleSelect();
        }
    }

    private void SelectThisPhoto()
    {
        if (!canDelete)
        {
            GalleryManager.Instance.ShowWarning();
            return;
        }
        isSelected = true;
        UpdateVisual();
        GalleryManager.Instance.AddSelect(this);
    }

    public void ToggleSelect()
    {
        if (!canDelete)
        {
            GalleryManager.Instance.ShowWarning();
            return;
        }

        isSelected = !isSelected;
        UpdateVisual();

        if (isSelected)
            GalleryManager.Instance.AddSelect(this);
        else
            GalleryManager.Instance.RemoveSelect(this);
    }

    public void SetSelected(bool value)
    {
        isSelected = value;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        bool inMulti = GalleryManager.Instance.multiMode;

        if (!inMulti)
        {
            deleteSelect.SetActive(false);
            waitBox.SetActive(false);
        }
        else if (isSelected)
        {
            deleteSelect.SetActive(true);
            waitBox.SetActive(false);
        }
        else
        {
            deleteSelect.SetActive(false);
            waitBox.SetActive(true);
        }
    }

    public bool IsSelected() => isSelected;
}