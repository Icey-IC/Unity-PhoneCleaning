using UnityEngine;
using System.Collections.Generic;

public class FolderPanel : MonoBehaviour
{
    public GameObject cellPrefab;
    public float cellSize = 0.8f;
    public int columns = 3;
    public int rows = 3;

    public FolderOverlay overlay;

    private GridCell[] cells = new GridCell[9];
    private List<AppIcon> pendingIcons = new List<AppIcon>();

    private FolderIcon owner;
    public FolderIcon Owner => owner;

    float panelZ = -8f;
    float cellZ = -9f;
    float iconZ = -10f;

    public void Init(FolderIcon folderIcon)
    {
        owner = folderIcon;
        GenerateCells();
        gameObject.SetActive(false);
    }

    void GenerateCells()
    {
        for (int i = 0; i < rows * columns; i++)
        {
            int row = i / columns;
            int col = i % columns;

            float x = (col - 1) * cellSize;
            float y = (1 - row) * cellSize;

            GameObject cellObj = Instantiate(cellPrefab, transform);

            // ✅ 用 localPosition（修复不居中）
            cellObj.transform.localPosition = new Vector3(x, y, 0f);

            GridCell cell = cellObj.GetComponent<GridCell>();

            // 设置 z
            Vector3 pos = cell.transform.position;
            pos.z = cellZ;
            cell.transform.position = pos;

            cells[i] = cell;
        }
    }

    public bool AddIcon(AppIcon icon)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].IsEmpty)
            {
                cells[i].currentIcon = icon;
                icon.currentCell = cells[i];
                icon.isInFolder = true;

                icon.transform.position = new Vector3(9999f, 9999f, iconZ);

                if (!pendingIcons.Contains(icon))
                    pendingIcons.Add(icon);

                return true;
            }
        }
        return false;
    }

    public void Show()
    {
        gameObject.SetActive(true);

        // Panel 到最前
        transform.position = new Vector3(0f, 0f, panelZ);

        // 所有 cell 提前
        foreach (var cell in cells)
        {
            Vector3 pos = cell.transform.position;
            pos.z = cellZ;
            cell.transform.position = pos;
        }

        overlay.Show(this);

        SyncPending();
        MoveAllIconsToFront();
    }

    public void Hide()
    {
        gameObject.SetActive(false);

        // ✅ 隐藏所有内部 icon
        foreach (var cell in cells)
        {
            if (!cell.IsEmpty && cell.currentIcon != null)
            {
                cell.currentIcon.gameObject.SetActive(false);
            }
        }

        if (overlay != null)
            overlay.Hide();

        if (owner != null)
            owner.NotifyPanelClosed();
    }

    void SyncPending()
    {
        foreach (var icon in pendingIcons)
        {
            if (icon != null && icon.currentCell != null)
            {
                Vector3 pos = icon.currentCell.transform.position;
                pos.z = iconZ;

                icon.transform.position = pos;
            }
        }

        pendingIcons.Clear();
    }

    void MoveAllIconsToFront()
    {
        foreach (var cell in cells)
        {
            if (!cell.IsEmpty && cell.currentIcon != null)
            {
                var icon = cell.currentIcon;

                icon.gameObject.SetActive(true); // ✅ 恢复显示

                Vector3 pos = icon.transform.position;
                pos.z = iconZ;
                icon.transform.position = pos;
            }
        }
    }

    public bool IsFull()
    {
        foreach (var c in cells)
            if (c.IsEmpty) return false;

        return true;
    }

    public List<AppIcon> GetAllIcons()
    {
        List<AppIcon> list = new List<AppIcon>();

        foreach (var c in cells)
            if (!c.IsEmpty)
                list.Add(c.currentIcon);

        return list;
    }
}