// GalleryManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GalleryManager : MonoBehaviour
{
    public static GalleryManager Instance;
    public bool multiMode = false;
    public Button deleteButton;
    public TextMeshProUGUI warningText;

    private List<PhotoItem> selectedPhotos = new List<PhotoItem>();

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
        if (multiMode) return;
        multiMode = true;
        selectedPhotos.Clear();
        UpdateAllPhotoState(); // 让所有图片刷新到"待选"状态
    }

    public void ExitMultiSelect()
    {
        multiMode = false;
        selectedPhotos.Clear();
        UpdateAllPhotoState(); // 所有图片回到正常状态
        deleteButton.interactable = false;
    }

    public void AddSelect(PhotoItem photo)
    {
        if (!selectedPhotos.Contains(photo))
            selectedPhotos.Add(photo);
        UpdateDeleteButton();
    }

    public void RemoveSelect(PhotoItem photo)
    {
        selectedPhotos.Remove(photo);
        UpdateDeleteButton();

        // 如果全部取消选择，退出multiMode
        if (selectedPhotos.Count == 0)
            ExitMultiSelect();
    }

    private void UpdateDeleteButton()
    {
        deleteButton.interactable = selectedPhotos.Count > 0;
    }

    public void UpdateAllPhotoState()
    {
        PhotoItem[] photos = FindObjectsOfType<PhotoItem>();
        foreach (var p in photos)
            p.UpdateVisual();
    }

    public void DeleteSelected()
    {
        // 检查是否有不该删的
        foreach (var photo in selectedPhotos)
        {
            if (!photo.canDelete)
            {
                ShowWarning();
                return; // 有不该删的，拒绝删除
            }
        }

        // 全部合法，执行删除
        foreach (var photo in selectedPhotos)
            Destroy(photo.gameObject);

        selectedPhotos.Clear();
        deleteButton.interactable = false;
        multiMode = false;
    }

    public void ShowWarning()
    {
        StopAllCoroutines(); // 防止多次触发叠加
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
