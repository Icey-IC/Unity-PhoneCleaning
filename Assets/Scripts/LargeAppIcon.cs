using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 2x2 large icon: clickable, right-click dialogue, not draggable; occupied cells reject other app drops.
/// Configure top-left anchor (row, col) in GridManager.initialLargeLayout.
/// </summary>
public class LargeAppIcon : MonoBehaviour
{
    [Header("App id")]
    public string appID;

    [Header("App UI")]
    public GameObject appView;

    [Header("Scale feedback")]
    public float pressScale = 0.9f;
    public float scaleAnimDuration = 0.1f;

    [Header("Dialogue")]
    public DialogueAsset dialogueData;
    public DialogueAsset onOpenDialogue;

    [HideInInspector] public GridCell anchorCell;

    private SpriteRenderer sr;
    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    private bool mouseDownOnThisIcon;
    private GridCell[] occupiedCells;

    static bool IsPointerOverBlockingUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    /// <summary>Called by GridManager after reserving a 2x2 block.</summary>
    public void OccupyCells(GridCell[] cells)
    {
        if (cells == null || cells.Length != 4)
        {
            Debug.LogWarning("LargeAppIcon requires exactly 4 cells.");
            return;
        }

        occupiedCells = cells;
        anchorCell = cells[0];

        foreach (var cell in cells)
            cell.SetLargeIcon(this);

        Vector3 center = Vector3.zero;
        foreach (var cell in cells)
            center += cell.transform.position;
        center /= cells.Length;
        center.z = -3f;
        transform.position = center;
    }

    void OnMouseDown()
    {
        if (IsPointerOverBlockingUI())
            return;

        mouseDownOnThisIcon = true;
        ScaleTo(originalScale * pressScale);
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (IsPointerOverBlockingUI())
                return;
            StartDialogue();
        }
    }

    void OnMouseUp()
    {
        if (!mouseDownOnThisIcon)
            return;
        mouseDownOnThisIcon = false;

        ScaleTo(originalScale, onComplete: OnClicked);
    }

    void OnClicked()
    {
        if (appView == null)
        {
            Debug.LogWarning("Large app view is not assigned.");
            return;
        }

        OpenApp();
    }

    void OpenApp()
    {
        appView.SetActive(true);
        appView.transform.SetAsLastSibling();

        if (onOpenDialogue != null)
            DialogueManager.Instance.StartDialogue(onOpenDialogue.lines);
    }

    public void CloseApp()
    {
        gameObject.SetActive(false);
    }

    void StartDialogue()
    {
        if (dialogueData == null)
        {
            Debug.LogWarning("Dialogue data is not assigned.");
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogueData.lines);
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
