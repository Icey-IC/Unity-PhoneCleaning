using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class PhotoItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,IPointerClickHandler
{
    public bool canDelete = true;
    public Image selectOverlay;
    public Image multiMask;

    private bool isSelected = false;
    private bool isPointerDown = false;
    private float pressTime = 0f;
    private float longPressTime = 0.5f;

   
    
    void Update()
    {
        if(isPointerDown)
        {
            pressTime += Time.deltaTime;
            if(pressTime>=longPressTime)
            {
                isPointerDown = false;
                GalleryManager.Instance.EnterMultiSelect();
                ToggleSelect();

            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        pressTime = 0;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;

    }
    public void ToggleSelect()
    {
        if (!GalleryManager.Instance.multiMode)
        {
            GalleryManager.Instance.EnterMultiSelect();
        }
           
        if(!canDelete)
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
        selectOverlay.gameObject.SetActive(isSelected);
        if(GalleryManager.Instance.multiMode)
        {
            multiMask.gameObject.SetActive(!isSelected);

        }
        else
        {
            multiMask.gameObject.SetActive(false);
        }
    }

    public bool IsSelected()
    {
        return isSelected;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if(GalleryManager.Instance.multiMode)
        {
            ToggleSelect();
        }
    }
}
