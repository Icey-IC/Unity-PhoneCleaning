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
    public LargeAppIcon currentLargeIcon;

    public bool IsBlockedByLargeIcon => currentLargeIcon != null;
    public bool IsEmpty => currentIcon == null && currentFolder == null && !IsBlockedByLargeIcon;

    /// <summary>Whether a draggable app icon can be dropped on this cell.</summary>
    public bool CanAcceptDrop => IsEmpty;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = normalColor;

        // Cell render layer
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
        if (CanAcceptDrop)
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
        pos.z = -3f; // Icon layer

        icon.transform.position = pos;
        icon.currentCell = this;

        sr.enabled = false;
        SetColliderEnabled(false);
    }

    public void RemoveIcon()
    {
        currentIcon = null;
        sr.enabled = true;
        SetColliderEnabled(true);
    }

    public void SetFolder(FolderIcon folder)
    {
        currentFolder = folder;

        Vector3 pos = transform.position;
        pos.z = -3f;

        folder.transform.position = pos;
        folder.currentCell = this;

        sr.enabled = false;
        SetColliderEnabled(false);
    }

    public void RemoveFolder()
    {
        currentFolder = null;
        sr.enabled = true;
        SetColliderEnabled(true);
    }

    public void SetLargeIcon(LargeAppIcon icon)
    {
        currentLargeIcon = icon;
        sr.enabled = false;
        SetColliderEnabled(false);
    }

    public void RemoveLargeIcon()
    {
        currentLargeIcon = null;
        sr.enabled = true;
        SetColliderEnabled(true);
    }

    void SetColliderEnabled(bool enabled)
    {
        var col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = enabled;
    }
}