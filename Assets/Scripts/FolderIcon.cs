using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FolderIcon : MonoBehaviour
{
    public LayerMask gridLayer;

    [Header("允许的App")]
    public List<string> allowedAppIDs = new List<string>();

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
        new Vector3(-1f,    1f, 0f),
        new Vector3(0f,      1f, 0f),
        new Vector3(1f,   1f, 0f),
        new Vector3(-1f,  0f,    0f),
        new Vector3(0f,      0f,    0f),
        new Vector3(1f,   0f,    0f),
        new Vector3(-1f, -1f, 0f),
        new Vector3(0f,     -1f, 0f),
        new Vector3(1f,  -1f, 0f),
    };

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalSortingOrder = sr.sortingOrder;
        originalScale = transform.localScale;

        if (folderPanel != null)
        {
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
        if (isPanelOpen) return;

        isPanelOpen = true;
        folderPanel.Show();
    }

    public void ClosePanel()
    {
        if (!isPanelOpen) return;

        isPanelOpen = false;

        if (folderPanel != null)
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
        // ❌ 不允许的App
        if (!allowedAppIDs.Contains(icon.appID))
        {
            RejectIcon(icon);
            return false;
        }

        if (folderPanel.IsFull()) return false;

        folderPanel.AddIcon(icon);
        RefreshPreview();

        ScaleTo(originalScale);
        isHoveredByDrag = false;

        return true;
    }

    void RejectIcon(AppIcon icon)
    {
        // 1️⃣ 弹提示气泡
        ShowRejectMessage("这个App不能放进该文件夹");

        // 2️⃣ 复位 App
        if (icon.currentCell != null)
        {
            icon.currentCell.SetIcon(icon);
        }
        else
        {
            // fallback（极端情况）
            icon.transform.position = icon.transform.position;
        }

        // 3️⃣ 恢复缩放状态
        ScaleTo(originalScale);
        isHoveredByDrag = false;
    }
   

    void ShowRejectMessage(string msg)
    {
        var dialogue = new List<DialogueLine>()
    {
        new DialogueLine
        {
            type = DialogueType.Player,
            text = msg
        }
    };

        DialogueManager.Instance.StartDialogue(dialogue);
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

            // 用localPosition而不是worldPosition，z轴设为负值确保显示在文件夹图标前面
            preview.transform.localPosition = previewPositions[i];

            Vector3 pos = preview.transform.localPosition;
            pos.z = -0.2f; // 比folder靠前一点
            preview.transform.localPosition = pos;
            
            preview.transform.localScale = Vector3.one * previewScale;
            preview.transform.localRotation = Quaternion.identity;

            SpriteRenderer previewSr = preview.AddComponent<SpriteRenderer>();
            previewSr.sprite = original.sprite;
        }
    }
    void ScaleTo(Vector3 targetScale, System.Action onComplete = null)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleCoroutine(targetScale, onComplete));
    }

    public void NotifyPanelClosed()
    {
        isPanelOpen = false;
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