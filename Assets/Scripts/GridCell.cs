using UnityEngine;

public class GridCell : MonoBehaviour
{
    public Color normalColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
    public Color hoverColor = Color.white;

    /// <summary>Folder grid this cell belongs to (set by <see cref="FolderPanel"/> or manually if cells live under a separate Canvas).</summary>
    public FolderPanel ownerFolderPanel;

    private SpriteRenderer sr;

    public AppIcon currentIcon;
    public FolderIcon currentFolder;

    public bool IsEmpty => currentIcon == null && currentFolder == null;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = normalColor;

        // Cell 层级
        SetZ(-2f);
    }

    void SetZ(float z)
    {
        Vector3 pos = transform.position;
        pos.z = z;
        transform.position = pos;
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

    public void SetIcon(AppIcon icon)
    {
        currentIcon = icon;

        Vector3 pos = transform.position;
        pos.z = -3f; // Icon层

        icon.transform.position = pos;
        icon.currentCell = this;

        sr.enabled = false;
    }

    public void RemoveIcon()
    {
        currentIcon = null;
        sr.enabled = true;
    }

    public void SetFolder(FolderIcon folder)
    {
        currentFolder = folder;

        Vector3 pos = transform.position;
        pos.z = -3f;

        folder.transform.position = pos;
        folder.currentCell = this;

        sr.enabled = false;
    }

    public void RemoveFolder()
    {
        currentFolder = null;
        sr.enabled = true;
    }
}