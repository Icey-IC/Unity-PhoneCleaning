using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;

public class GalleryManager : MonoBehaviour
{
    public static GalleryManager Instance;
    public bool multiMode = false;
    public Button deleteButton;
    public TextMeshProUGUI warningText;

    private List<PhotoItem> selectedPhotos = new List<PhotoItem>();

    // Update is called once per frame
    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        deleteButton.interactable = false;
        warningText.gameObject.SetActive(false);
    }

    public void EnterMultiSelect()
    {
        if (multiMode)
            return;

        multiMode = true;
        selectedPhotos.Clear();
        UpdateAllPhotoState();

    }
    public void AddSelect(PhotoItem photo)
    {
        if(!selectedPhotos.Contains(photo))
        {
            selectedPhotos.Add(photo);
        }
        UpdateDeleteButton();
    }

    public void RemoveSelect(PhotoItem photo)
    {
        selectedPhotos.Remove(photo);
        UpdateDeleteButton();
    }

    public void SelectPhoto(PhotoItem photo)
    {
        if(!photo.canDelete)
        {
            ShowWarning();
            return;

        }
        photo.SetSelected(true);

        if(!selectedPhotos.Contains(photo))
        {
            selectedPhotos.Add(photo);
        }
        UpdateDeleteButton();
    }

    private void UpdateDeleteButton()
    {
        deleteButton.interactable = selectedPhotos.Count > 0;

    }
    public void UpdateAllPhotoState()
    {
        PhotoItem[] photos = FindObjectsOfType<PhotoItem>();
        foreach(var p in photos)
        {
            p.UpdateVisual();
        }
    }
    public void DeleteSelected()
    {
        foreach(var photo in selectedPhotos)
        {
            Destroy(photo.gameObject);
        }
        selectedPhotos.Clear();
        deleteButton.interactable = false;
        multiMode = false;
    }

    public void ShowWarning()
    {
        StartCoroutine(WarningCoroutine());
    }

    IEnumerator WarningCoroutine()
    {
        warningText.gameObject.SetActive(true);
        warningText.text = "选到了不该删除的照片";
        yield return new WaitForSeconds(2f);
        warningText.gameObject.SetActive(false);
    }
}
