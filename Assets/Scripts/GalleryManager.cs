using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(-1)]
public class GalleryManager : MonoBehaviour
{
    public static GalleryManager Instance;
    public bool multiMode = false;
    public Button deleteButton;
    public TextMeshProUGUI warningText;

    private List<PhotoItem> selectedPhotos = new List<PhotoItem>();
    // 改为手动注册，不依赖FindObjectsOfType
    private List<PhotoItem> allPhotos = new List<PhotoItem>();

    void Awake()
    {
        Instance = this;
        allPhotos = new List<PhotoItem>();
        selectedPhotos = new List<PhotoItem>();
    }

    private void Start()
    {
        deleteButton.interactable = false;
        warningText.gameObject.SetActive(false);
    }

    // PhotoItem在自己的Start里调用这个注册自己
    public void RegisterPhoto(PhotoItem photo)
    {
        if (!allPhotos.Contains(photo))
            allPhotos.Add(photo);
    }

    // PhotoItem销毁时注销
    public void UnregisterPhoto(PhotoItem photo)
    {
        allPhotos.Remove(photo);
    }

    public void EnterMultiSelect()
    {
        if (multiMode) return;
        multiMode = true;
        selectedPhotos.Clear();
        Debug.Log("EnterMultiSelect时allPhotos数量：" + allPhotos.Count + "，场景总PhotoItem数：" + FindObjectsOfType<PhotoItem>().Length);
        StartCoroutine(UpdateNextFrame());
    }

    IEnumerator UpdateNextFrame()
    {
        yield return null; // 等一帧，让所有PhotoItem的Awake执行完
        Debug.Log("延迟后allPhotos数量：" + allPhotos.Count);
        UpdateAllPhotoState();
    }

    public void ExitMultiSelect()
    {
        multiMode = false;
        selectedPhotos.Clear();
        // 重置所有图片的选中状态
        foreach (var p in allPhotos)
            p.SetSelected(false);
        UpdateAllPhotoState();
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
        if (selectedPhotos.Count == 0)
            ExitMultiSelect();
    }

    private void UpdateDeleteButton()
    {
        deleteButton.interactable = selectedPhotos.Count > 0;
    }

    public void UpdateAllPhotoState()
    {
        foreach (var p in allPhotos)
        {
            Debug.Log("刷新图片：" + p.gameObject.name + " multiMode=" + multiMode);
            p.UpdateVisual();
        }
           
    }

    public void DeleteSelected()
    {
        foreach (var photo in selectedPhotos)
        {
            if (!photo.canDelete)
            {
                ShowWarning();
                return;
            }
        }

        foreach (var photo in selectedPhotos)
        {
            allPhotos.Remove(photo);
            Destroy(photo.gameObject);
        }

        selectedPhotos.Clear();
        deleteButton.interactable = false;
        multiMode = false;
        // 重置剩余图片状态
        foreach (var p in allPhotos)
            p.SetSelected(false);
        UpdateAllPhotoState();
    }

    public void ShowWarning()
    {
        StopAllCoroutines();
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
