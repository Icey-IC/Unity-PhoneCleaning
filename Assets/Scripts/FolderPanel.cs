using UnityEngine;
using System.Collections.Generic;

public class FolderPanel : MonoBehaviour
{
    [Header("面板格子配置")]
    public GameObject cellPrefab;
    public float cellSize = 0.8f;       // 格子间距
    public int columns = 3;
    public int rows = 3;

    private GridCell[] cells = new GridCell[9];
    private FolderIcon owner;

    public void Init(FolderIcon folderIcon)
    {
        owner = folderIcon;
        GenerateCells();
        gameObject.SetActive(false);
    }

    void GenerateCells()
    {
        // 以面板中心为原点，生成3x3格子
        for (int i = 0; i < rows * columns; i++)
        {
            int row = i / columns;
            int col = i % columns;

            float x = (col - 1) * cellSize; // -1, 0, 1
            float y = (1 - row) * cellSize; // 1, 0, -1

            Vector3 localPos = new Vector3(x, y, 0);
            GameObject cellObj = Instantiate(cellPrefab, transform);
            cellObj.transform.localPosition = localPos;
            cellObj.name = $"FolderCell_{i}";

            cells[i] = cellObj.GetComponent<GridCell>();
        }
    }

    // 将软件放入面板，自动找第一个空格子
    public bool AddIcon(AppIcon icon)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].IsEmpty)
            {
                // 从原来的格子移除
                if (icon.currentCell != null)
                    icon.currentCell.RemoveIcon();

                cells[i].SetIcon(icon);
                return true;
            }
        }
        return false; // 面板已满
    }

    // 获取当前面板内所有软件
    public List<AppIcon> GetAllIcons()
    {
        List<AppIcon> icons = new List<AppIcon>();
        for (int i = 0; i < cells.Length; i++)
        {
            if (!cells[i].IsEmpty)
                icons.Add(cells[i].currentIcon);
        }
        return icons;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        // 固定在世界坐标，不受任何父物体影响（Awake里已SetParent(null)）
        transform.position = new Vector3(0f, 0f, -2f);
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public bool IsFull()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].IsEmpty) return false;
        }
        return true;
    }
}