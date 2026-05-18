using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class AppIcon : MonoBehaviour
{
    public LayerMask gridLayer;
    public float longPressDuration = 0.5f;

    [Header("App id")]
    public string appID;

    [Header("Notifications (for tasks)")]
    [Tooltip("Allow-notifications toggle from the long-press menu; tasks can compare appID to required on/off. Default: on at start.")]
    public bool notificationsAllowed = true;

    [Header("App UI")]
    public GameObject appView; // each app's own screen

    [HideInInspector]
    public bool isInFolder = false;

    [Header("Scale feedback")]
    public float pressScale = 0.9f;
    public float dragScale = 1.1f;
    public float scaleAnimDuration = 0.1f;

    [Header("Context menu")]
    public AppContextMenu contextMenu; // assign child menu in Inspector

    [HideInInspector] public GridCell currentCell;
    private GridCell hoveredCell;
    private Vector3 startDragPos;

    private SpriteRenderer sr;
    private int originalSortingOrder;
    private Vector3 originalScale;

    private float mouseDownTime = 0f;
    private bool isDragging = false;
    private bool longPressTriggered = false;
    private bool menuShown = false;         // context menu visible
    private bool mouseMoved = false;        // moved past threshold after press

    private Coroutine scaleCoroutine;

    // pixels: treat as click vs drag
    private const float movementThreshold = 5f;
    private Vector3 mouseDownScreenPos;     // screen pos at mouse down

    private FolderIcon hoveredFolder = null; // folder under drag

    /// <summary>True after a valid OnMouseDown on this icon (not blocked by UI); prevents stray OnMouseUp from UI click-through.</summary>
    private bool mouseDownOnThisIcon;

    public DialogueAsset dialogueData;

    static bool IsPointerOverBlockingUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalSortingOrder = sr.sortingOrder;
        originalScale = transform.localScale;

        // init menu
        if (contextMenu != null)
        {
            contextMenu.Init(this);
            contextMenu.Hide();
        }
    }

    void Update()
    {
        if (menuShown && Input.GetMouseButtonDown(0))
        {
            if (!IsClickOnMenu())
            {
                CloseMenu();
            }
        }
    }

    // pointer over UI counts as on-menu
    bool IsClickOnMenu()
    {
        if (contextMenu == null) return false;
        return EventSystem.current != null
               && EventSystem.current.IsPointerOverGameObject();
    }
    void OnMouseDown()
    {
        if (IsPointerOverBlockingUI())
            return;

        // menu open: let Update handle outside click
        if (menuShown) return;

        mouseDownOnThisIcon = true;
        mouseDownTime = Time.time;
        mouseDownScreenPos = Input.mousePosition;
        isDragging = false;
        longPressTriggered = false;
        mouseMoved = false;
        startDragPos = transform.position;

        ScaleTo(originalScale * pressScale);
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1)) // RMB
        {
            if (IsPointerOverBlockingUI())
                return;
            StartAppDialogue();
        }
    }

    void OnMouseDrag()
    {
        if (!mouseDownOnThisIcon)
            return;

        if (!isDragging && !menuShown && IsPointerOverBlockingUI())
            return;

        // movement threshold
        if (!mouseMoved)
        {
            float movedPixels = Vector3.Distance(Input.mousePosition, mouseDownScreenPos);
            if (movedPixels > movementThreshold)
            {
                mouseMoved = true;
            }
        }

        // wait for long-press time
        if (!longPressTriggered)
        {
            if (Time.time - mouseDownTime >= longPressDuration)
            {
                longPressTriggered = true;

                if (mouseMoved)
                {
                    // moved before long press ended -> drag
                    EnterDragState();
                }
                else
                {
                    // still -> show menu
                    ShowMenu();
                }
            }
            return;
        }

        // menu up and then move -> close menu and drag
        if (menuShown && mouseMoved)
        {
            CloseMenu();
            EnterDragState();
            // fall through so drag runs this frame
        }

        if (isDragging)
        {
            DragUpdate();
        }
    }

    void OnMouseUp()
    {
        if (!mouseDownOnThisIcon)
            return;
        mouseDownOnThisIcon = false;

        // menu visible and not dragging: keep menu
        if (menuShown && !isDragging)
        {
            ScaleTo(originalScale);
            return;
        }

        if (!longPressTriggered)
        {
            // released before long press -> single click
            ScaleTo(originalScale, onComplete: OnAppClicked);
            return;
        }

        if (!isDragging)
        {
            ScaleTo(originalScale);
            return;
        }

        // end drag
        FinishDrag();
    }
    // ==================== state ====================

    void EnterDragState()
    {
        isDragging = true;

        FolderPanel sourceFolderPanel = currentCell != null ? currentCell.ownerFolderPanel : null;
        if (currentCell != null)
            currentCell.RemoveIcon();

        if (sourceFolderPanel != null && sourceFolderPanel.Owner != null)
            sourceFolderPanel.Owner.RefreshPreview();

        SetZ(-10f); // drag layer
        ScaleTo(originalScale * dragScale);
    }

    void ShowMenu()
    {
        menuShown = true;
        ScaleTo(originalScale); // normal size when menu opens

        if (contextMenu != null)
            contextMenu.Show();
    }

    void CloseMenu()
    {
        menuShown = false;

        if (contextMenu != null)
            contextMenu.Hide();
    }

    void DragUpdate()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = -10f;

        transform.position = mousePos;

        TryCloseOpenFolderIfPointerLeftPanel(mousePos);

        Collider2D hitCollider = Physics2D.OverlapPoint(mousePos, gridLayer);

        if (hitCollider != null)
        {
            // folder vs cell
            FolderIcon folder = hitCollider.GetComponent<FolderIcon>();
            GridCell cell = hitCollider.GetComponent<GridCell>();

            if (folder != null)
            {
                // over folder
                if (folder != hoveredFolder)
                {
                    // leave cell highlight
                    if (hoveredCell != null) { hoveredCell.Unhighlight(); hoveredCell = null; }
                    // leave old folder
                    if (hoveredFolder != null) hoveredFolder.OnDragExit();

                    hoveredFolder = folder;
                    hoveredFolder.OnDragEnter();
                }
            }
            else if (cell != null)
            {
                // over grid cell
                if (hoveredFolder != null) { hoveredFolder.OnDragExit(); hoveredFolder = null; }

                if (cell != hoveredCell)
                {
                    if (hoveredCell != null) hoveredCell.Unhighlight();
                    hoveredCell = cell;
                    hoveredCell.Highlight();
                }
            }
        }
        else
        {
            if (hoveredCell != null) { hoveredCell.Unhighlight(); hoveredCell = null; }
            if (hoveredFolder != null) { hoveredFolder.OnDragExit(); hoveredFolder = null; }
        }
    }

    /// <summary>While dragging an icon that belongs to an open folder, close the folder once the pointer leaves the panel so the home grid is visible.</summary>
    void TryCloseOpenFolderIfPointerLeftPanel(Vector3 mouseWorld)
    {
        if (!isDragging || currentCell == null)
            return;

        FolderPanel panel = currentCell.ownerFolderPanel;
        if (panel == null || !panel.gameObject.activeInHierarchy || panel.Owner == null)
            return;

        if (!panel.WorldPointIsInsidePanelBounds(new Vector2(mouseWorld.x, mouseWorld.y)))
            panel.Owner.ClosePanel();
    }

    void FinishDrag()
    {
        sr.sortingOrder = originalSortingOrder;
        isDragging = false;

        if (hoveredFolder != null)
        {
            // drop into folder
            hoveredFolder.ReceiveIcon(this);
            hoveredFolder = null;
            hoveredCell = null;
        }
        else if (hoveredCell != null && hoveredCell.IsEmpty)
        {
            hoveredCell.Unhighlight();
            FolderPanel dropFolder = hoveredCell.ownerFolderPanel;
            hoveredCell.SetIcon(this);
            isInFolder = dropFolder != null;
            if (dropFolder != null && dropFolder.Owner != null)
                dropFolder.Owner.RefreshPreview();
            hoveredCell = null;
        }
        else
        {
            if (hoveredCell != null) { hoveredCell.Unhighlight(); hoveredCell = null; }
            if (hoveredFolder != null) { hoveredFolder.OnDragExit(); hoveredFolder = null; }

            if (currentCell != null)
            {
                FolderPanel restoreFolder = currentCell.ownerFolderPanel;
                currentCell.SetIcon(this);
                if (restoreFolder != null && restoreFolder.Owner != null)
                    restoreFolder.Owner.RefreshPreview();
            }
            else
                transform.position = startDragPos;
        }

        ScaleTo(originalScale);
    }
    void OnAppClicked()
    {
        if (appView == null)
        {
            Debug.LogWarning("App view is not assigned.");
            return;
        }

        OpenApp();
    }

    void OpenApp()
    {
        // 如果在文件夹里，先关闭文件夹
        if (currentCell != null && currentCell.ownerFolderPanel != null)
        {
            var panel = currentCell.ownerFolderPanel;

            if (panel.Owner != null)
            {
                panel.Owner.ClosePanel();
            }
        }

        appView.SetActive(true);

        // optional: bring app UI to front
        appView.transform.SetAsLastSibling();
    }

    public void CloseApp()
    {
        gameObject.SetActive(false);
    }

    /// <summary>Called from the long-press menu allow-notifications toggle; tasks read <see cref="notificationsAllowed"/>.</summary>
    public void SetNotificationsAllowed(bool allowed)
    {
        notificationsAllowed = allowed;
    }

    /// <summary>Find an app by appID in the scene (includes inactive); for task validation.</summary>
    public static AppIcon FindByAppId(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        var all = FindObjectsOfType<AppIcon>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].appID == id)
                return all[i];
        }
        return null;
    }

    public void Uninstall()
    {
        FolderPanel folderPanel = currentCell != null ? currentCell.ownerFolderPanel : null;

        if (currentCell != null)
        {
            currentCell.RemoveIcon();
            currentCell = null;
        }

        if (folderPanel != null && folderPanel.Owner != null)
            folderPanel.Owner.RefreshPreview();

        // hide menu
        if (contextMenu != null)
        {
            contextMenu.Hide();
        }

        // 4 destroy
        Destroy(gameObject);
    }

    // ==================== scale ====================

    void ScaleTo(Vector3 targetScale, System.Action onComplete = null)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(ScaleCoroutine(targetScale, onComplete));
    }

    void SetZ(float z)
    {
        Vector3 pos = transform.position;
        pos.z = z;
        transform.position = pos;
    }

    void StartAppDialogue()
    {
        if (dialogueData == null)
        {
            Debug.LogWarning("Dialogue data is not assigned.");
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogueData.lines);
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
