using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CellIconMapping
{
    public int row;
    public int col;
    public AppIcon icon;       // 软件图标，和下面二选一
    public FolderIcon folder;  // 文件夹图标，和上面二选一
}

public class GridManager : MonoBehaviour
{
    public GameObject cellPrefab;

    [Header("自定义行和列的坐标")]
    public float[] rowYPositions = new float[5] { 1.1f, 0f, -0.9f, -1.8f, -3.2f };
    public float[] columnXPositions = new float[5] { -1.6f, -0.8f, 0f, 0.8f, 1.6f };

    [Header("最后一行特殊配置")]
    public bool overrideLastRow = true;
    public float[] lastRowColumnXPositions = new float[4] { -1.2f, -0.4f, 0.4f, 1.2f };

    [Header("初始布局配置")]
    public List<CellIconMapping> initialLayout = new List<CellIconMapping>();

    private GridCell[,] cellGrid;

    void Start()
    {
        GenerateGrid();
        ApplyInitialLayout();
    }

    void GenerateGrid()
    {
        cellGrid = new GridCell[rowYPositions.Length, columnXPositions.Length];

        for (int row = 0; row < rowYPositions.Length; row++)
        {
            bool isLastRow = (row == rowYPositions.Length - 1);
            float[] xPositions = (isLastRow && overrideLastRow && lastRowColumnXPositions.Length > 0)
                ? lastRowColumnXPositions
                : columnXPositions;

            for (int col = 0; col < xPositions.Length; col++)
            {
                Vector2 spawnPos = new Vector2(xPositions[col], rowYPositions[row]);
                GameObject cellObj = Instantiate(cellPrefab, spawnPos, Quaternion.identity, transform);
                cellObj.name = $"Cell_{row}_{col}";
                cellGrid[row, col] = cellObj.GetComponent<GridCell>();
            }
        }
    }

    void ApplyInitialLayout()
    {
        foreach (var mapping in initialLayout)
        {
            // 边界检查
            if (mapping.row >= rowYPositions.Length || mapping.col >= columnXPositions.Length)
            {
                Debug.LogWarning($"初始布局配置越界：row={mapping.row}, col={mapping.col}");
                continue;
            }

            GridCell cell = cellGrid[mapping.row, mapping.col];

            if (cell == null || !cell.IsEmpty)
            {
                Debug.LogWarning($"Cell_{mapping.row}_{mapping.col} 不存在或已被占用");
                continue;
            }

            // 软件图标和文件夹图标二选一，两个都填或都不填时报警告
            if (mapping.icon != null && mapping.folder != null)
            {
                Debug.LogWarning($"Cell_{mapping.row}_{mapping.col} 同时指定了icon和folder，请只填一个，已跳过");
                continue;
            }

            if (mapping.icon != null)
            {
                cell.SetIcon(mapping.icon);
            }
            else if (mapping.folder != null)
            {
                cell.SetFolder(mapping.folder);
            }
            else
            {
                Debug.LogWarning($"Cell_{mapping.row}_{mapping.col} 未指定任何图标");
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.4f);

        if (rowYPositions != null && columnXPositions != null)
        {
            for (int row = 0; row < rowYPositions.Length; row++)
            {
                bool isLastRow = (row == rowYPositions.Length - 1);
                float[] xPositions = (isLastRow && overrideLastRow && lastRowColumnXPositions != null && lastRowColumnXPositions.Length > 0)
                    ? lastRowColumnXPositions
                    : columnXPositions;

                for (int col = 0; col < xPositions.Length; col++)
                {
                    Vector2 pos = new Vector2(xPositions[col], rowYPositions[row]);
                    Gizmos.DrawWireCube(pos, new Vector3(0.6f, 0.6f, 0f));
                }
            }
        }
    }
}