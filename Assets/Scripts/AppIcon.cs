using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class AppIcon : MonoBehaviour
{
    public LayerMask gridLayer;
    public float longPressDuration = 0.5f;

    [Header("App标识")]
    public string appID;

    [Header("App界面")]
    public GameObject appView; // 每个app自己的界面

    [HideInInspector]
    public bool isInFolder = false;

    [Header("缩放反馈配置")]
    public float pressScale = 0.9f;
    public float dragScale = 1.1f;
    public float scaleAnimDuration = 0.1f;

    [Header("右键菜单配置")]
    public AppContextMenu contextMenu; // 在Inspector里拖入子物体上的菜单组件

    [HideInInspector] public GridCell currentCell;
    private GridCell hoveredCell;
    private Vector3 startDragPos;

    private SpriteRenderer sr;
    private int originalSortingOrder;
    private Vector3 originalScale;

    private float mouseDownTime = 0f;
    private bool isDragging = false;
    private bool longPressTriggered = false;
    private bool menuShown = false;         // 小窗口是否已弹出
    private bool mouseMoved = false;        // 长按后鼠标是否移动过

    private Coroutine scaleCoroutine;

    // 用于判断鼠标是否移动的阈值（像素）
    private const float movementThreshold = 5f;
    private Vector3 mouseDownScreenPos;     // 按下时的屏幕坐标

    private FolderIcon hoveredFolder = null; // 当前悬停的文件夹

    public DialogueAsset dialogueData;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalSortingOrder = sr.sortingOrder;
        originalScale = transform.localScale;

        // 初始化菜单
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

    // 改为无参数版本
    bool IsClickOnMenu()
    {
        if (contextMenu == null) return false;
        return EventSystem.current != null
               && EventSystem.current.IsPointerOverGameObject();
    }
    void OnMouseDown()
    {
        //  在文件夹内 → 不允许拖拽
        if (isInFolder)
        {
            // 但仍然允许点击（走点击逻辑）
            return;
        }

        // 如果菜单已打开，本次点击交给Update里的关闭逻辑处理，不启动新的拖拽流程
        if (menuShown) return;

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
        if (Input.GetMouseButtonDown(1)) // 右键
        {
            StartAppDialogue();
        }
    }

    void OnMouseDrag()
    {
        // 检测鼠标是否移动超过阈值
        if (!mouseMoved)
        {
            if (isInFolder) return; //  直接禁止拖拽

            float movedPixels = Vector3.Distance(Input.mousePosition, mouseDownScreenPos);
            if (movedPixels > movementThreshold)
            {
                mouseMoved = true;
            }
        }

        // 等待长按阈值
        if (!longPressTriggered)
        {
            if (Time.time - mouseDownTime >= longPressDuration)
            {
                longPressTriggered = true;

                if (mouseMoved)
                {
                    // 长按时已经移动了 → 直接进入拖拽
                    EnterDragState();
                }
                else
                {
                    // 长按时没有移动 → 弹出菜单
                    ShowMenu();
                }
            }
            return;
        }

        // 菜单已显示且鼠标开始移动 → 关闭菜单，进入拖拽
        if (menuShown && mouseMoved)
        {
            CloseMenu();
            EnterDragState();
            // 注意不要 return，让下方拖拽逻辑立刻生效
        }

        if (isDragging)
        {
            DragUpdate();
        }
    }

    void OnMouseUp()
    {
        // 在文件夹内 → 只触发点击
        if (isInFolder)
        {
            OnAppClicked();
            return;
        }

        // 菜单显示中且没有进入拖拽 → 松手后菜单保留
        if (menuShown && !isDragging)
        {
            ScaleTo(originalScale);
            return;
        }

        if (!longPressTriggered)
        {
            // 没撑到0.5秒就松手 → 单击
            ScaleTo(originalScale, onComplete: OnAppClicked);
            return;
        }

        if (!isDragging)
        {
            ScaleTo(originalScale);
            return;
        }

        // 正常拖拽结束
        FinishDrag();
    }
    // ==================== 状态操作 ====================

    void EnterDragState()
    {
        isDragging = true;

        if (currentCell != null)
            currentCell.RemoveIcon();

        SetZ(-10f); // 拖拽层
        ScaleTo(originalScale * dragScale);
    }

    void ShowMenu()
    {
        menuShown = true;
        ScaleTo(originalScale); // 弹出菜单时恢复原始大小

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

        Collider2D hitCollider = Physics2D.OverlapPoint(mousePos, gridLayer);

        if (hitCollider != null)
        {
            // 优先检测是否是文件夹
            FolderIcon folder = hitCollider.GetComponent<FolderIcon>();
            GridCell cell = hitCollider.GetComponent<GridCell>();

            if (folder != null)
            {
                // 悬停在文件夹上
                if (folder != hoveredFolder)
                {
                    // 离开旧的格子高亮
                    if (hoveredCell != null) { hoveredCell.Unhighlight(); hoveredCell = null; }
                    // 离开旧文件夹
                    if (hoveredFolder != null) hoveredFolder.OnDragExit();

                    hoveredFolder = folder;
                    hoveredFolder.OnDragEnter();
                }
            }
            else if (cell != null)
            {
                // 悬停在普通格子上
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
    void FinishDrag()
    {
        sr.sortingOrder = originalSortingOrder;
        isDragging = false;

        if (hoveredFolder != null)
        {
            // 放入文件夹
            hoveredFolder.ReceiveIcon(this);
            hoveredFolder = null;
            hoveredCell = null;
        }
        else if (hoveredCell != null && hoveredCell.IsEmpty)
        {
            hoveredCell.Unhighlight();
            hoveredCell.SetIcon(this);
            hoveredCell = null;
        }
        else
        {
            if (hoveredCell != null) { hoveredCell.Unhighlight(); hoveredCell = null; }
            if (hoveredFolder != null) { hoveredFolder.OnDragExit(); hoveredFolder = null; }

            if (currentCell != null)
                currentCell.SetIcon(this);
            else
                transform.position = startDragPos;
        }

        ScaleTo(originalScale);
    }
    void OnAppClicked()
    {
        if (appView == null)
        {
            Debug.LogWarning("没有绑定 App 界面");
            return;
        }

        OpenApp();
    }

    void OpenApp()
    {
        appView.SetActive(true);

        // ?? 可选：把app放到最前（UI）
        appView.transform.SetAsLastSibling();
    }

    public void CloseApp()
    {
        gameObject.SetActive(false);
    }

    public void Uninstall()
    {
        // 1?? 从格子移除
        if (currentCell != null)
        {
            currentCell.RemoveIcon();
            currentCell = null;
        }

        // 2?? 如果在文件夹里（可选扩展）
        // ?? 如果你后面支持 folder 内删除，这里要额外处理

        // 3?? 关闭自己的菜单（避免残留UI）
        if (contextMenu != null)
        {
            contextMenu.Hide();
        }

        // 4?? 销毁自己
        Destroy(gameObject);
    }

    // ==================== 缩放动画 ====================

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
            Debug.LogWarning("没有配置对话");
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