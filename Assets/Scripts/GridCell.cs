using UnityEngine;

public class GridCell : MonoBehaviour
{
    public Color normalColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
    public Color hoverColor = Color.white;

    private SpriteRenderer sr;

    public AppIcon currentIcon;       // 当前占据格子的软件
    public FolderIcon currentFolder;  // 当前占据格子的文件夹

    // 格子为空的条件：软件和文件夹都没有
    public bool IsEmpty => currentIcon == null && currentFolder == null;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = normalColor;
    }

    public void Highlight()
    {
        if (IsEmpty)
            sr.color = hoverColor;
    }

    public void Unhighlight()
    {
        sr.color = normalColor;
    }

    // 放置软件
    public void SetIcon(AppIcon icon)
    {
        currentIcon = icon;
        Vector3 snapPos = transform.position;
        snapPos.z = -1f;
        icon.transform.position = snapPos;
        icon.currentCell = this;
    }

    // 移除软件
    public void RemoveIcon()
    {
        currentIcon = null;
    }

    // 放置文件夹
    public void SetFolder(FolderIcon folder)
    {
        currentFolder = folder;
        Vector3 snapPos = transform.position;
        snapPos.z = -1f;
        folder.transform.position = snapPos;
        folder.currentCell = this;
    }

    // 移除文件夹
    public void RemoveFolder()
    {
        currentFolder = null;
    }
}