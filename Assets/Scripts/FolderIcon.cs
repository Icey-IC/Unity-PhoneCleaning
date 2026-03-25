using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FolderIcon : MonoBehaviour
{
    public LayerMask gridLayer;

    [Header("缩放反馈配置")]
    public float dragHoverScale = 1.1f;
    public float scaleAnimDuration = 0.1f;

    [Header("面板配置")]
    public FolderPanel folderPanel;

    [Header("预览图标配置")]
    public float previewScale = 0.3f;
    public Transform previewRoot;

    [HideInInspector] public GridCell currentCell;

    private SpriteRenderer sr;
    private int originalSortingOrder;
    private Vector3 originalScale;

    private bool isPanelOpen = false;
    private bool isHoveredByDrag = false;

    private Coroutine scaleCoroutine;

    private static readonly Vector3[] previewPositions = new Vector3[]
    {
        new Vector3(-0.18f,  0.18f, -0.1f),
        new Vector3(0f,      0.18f, -0.1f),
        new Vector3(0.18f,   0.18f, -0.1f),
        new Vector3(-0.18f,  0f,    -0.1f),
        new Vector3(0f,      0f,    -0.1f),
        new Vector3(0.18f,   0f,    -0.1f),
        new Vector3(-0.18f, -0.18f, -0.1f),
        new Vector3(0f,     -0.18f, -0.1f),
        new Vector3(0.18f,  -0.18f, -0.1f),
    };

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalSortingOrder = sr.sortingOrder;
        originalScale = transform.localScale;

        if (folderPanel != null)
        {
            // 将面板从父物体层级中脱离，使其不受FolderIcon的transform影响
            folderPanel.transform.SetParent(null);
            folderPanel.Init(this);
        }
    }

    void OnMouseUp()
    {
        if (isPanelOpen) return;
        OpenPanel();
    }

    public void OpenPanel()
    {
        isPanelOpen = true;
        folderPanel.Show();
    }

    public void ClosePanel()
    {
        isPanelOpen = false;
        folderPanel.Hide();
    }

    public void OnDragEnter()
    {
        if (isHoveredByDrag) return;
        isHoveredByDrag = true;
        ScaleTo(originalScale * dragHoverScale);
    }

    public void OnDragExit()
    {
        if (!isHoveredByDrag) return;
        isHoveredByDrag = false;
        ScaleTo(originalScale);
    }

    public bool ReceiveIcon(AppIcon icon)
    {
        if (folderPanel.IsFull()) return false;

        folderPanel.AddIcon(icon);
        RefreshPreview();
        ScaleTo(originalScale);
        isHoveredByDrag = false;
        return true;
    }

    public void RefreshPreview()
    {
        if (previewRoot == null) return;

        foreach (Transform child in previewRoot)
            Destroy(child.gameObject);

        List<AppIcon> icons = folderPanel.GetAllIcons();

        for (int i = 0; i < icons.Count && i < 9; i++)
        {
            SpriteRenderer original = icons[i].GetComponent<SpriteRenderer>();
            if (original == null) continue;

            GameObject preview = new GameObject($"Preview_{i}");
            preview.transform.SetParent(previewRoot);
            preview.transform.localPosition = previewPositions[i];
            preview.transform.localScale = Vector3.one * previewScale;

            SpriteRenderer previewSr = preview.AddComponent<SpriteRenderer>();
            previewSr.sprite = original.sprite;
            previewSr.sortingOrder = originalSortingOrder + 1;
        }
    }

    void ScaleTo(Vector3 targetScale, System.Action onComplete = null)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleCoroutine(targetScale, onComplete));
    }

    IEnumerator ScaleCoroutine(Vector3 targetScale, System.Action onComplete)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < scaleAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleAnimDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, smoothT);
            yield return null;
        }

        transform.localScale = targetScale;
        scaleCoroutine = null;
        onComplete?.Invoke();
    }
}