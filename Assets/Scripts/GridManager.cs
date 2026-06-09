using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CellIconMapping
{
    public int row;
    public int col;
    public AppIcon icon;       // App icon (mutually exclusive with folder)
    public FolderIcon folder;  // Folder icon (mutually exclusive with icon)
}

[System.Serializable]
public class LargeIconMapping
{
    [Tooltip("Top-left cell of the 2x2 block")]
    public int row;
    public int col;
    public LargeAppIcon icon;
}

public class GridManager : MonoBehaviour
{
    public GameObject cellPrefab;

    [Header("Grid row/column world positions")]
    public float[] rowYPositions = new float[5] { 1.1f, 0f, -0.9f, -1.8f, -3.2f };
    public float[] columnXPositions = new float[5] { -1.6f, -0.8f, 0f, 0.8f, 1.6f };

    [Header("Last row overrides")]
    public bool overrideLastRow = true;
    public float[] lastRowColumnXPositions = new float[4] { -1.2f, -0.4f, 0.4f, 1.2f };

    [Header("Initial layout (1x1 icons)")]
    public List<CellIconMapping> initialLayout = new List<CellIconMapping>();

    [Header("Initial layout (2x2 large icons)")]
    public List<LargeIconMapping> initialLargeLayout = new List<LargeIconMapping>();

    private GridCell[,] cellGrid;

    void Start()
    {
        GenerateGrid();
        ApplyInitialLargeLayout();
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

    public int GetColumnCountForRow(int row)
    {
        if (rowYPositions == null || row < 0 || row >= rowYPositions.Length)
            return 0;

        bool isLastRow = row == rowYPositions.Length - 1;
        return (isLastRow && overrideLastRow && lastRowColumnXPositions != null && lastRowColumnXPositions.Length > 0)
            ? lastRowColumnXPositions.Length
            : columnXPositions.Length;
    }

    public GridCell GetCell(int row, int col)
    {
        if (cellGrid == null || row < 0 || col < 0
            || row >= cellGrid.GetLength(0) || col >= cellGrid.GetLength(1))
            return null;
        return cellGrid[row, col];
    }

    /// <summary>Returns the 2x2 block with top-left at (row, col), or null if out of bounds or cells missing.</summary>
    public GridCell[] TryGet2x2Block(int row, int col)
    {
        if (rowYPositions == null || row + 1 >= rowYPositions.Length)
            return null;

        int colsTop = GetColumnCountForRow(row);
        int colsBottom = GetColumnCountForRow(row + 1);
        if (col < 0 || col + 1 >= colsTop || col + 1 >= colsBottom)
            return null;

        GridCell c00 = GetCell(row, col);
        GridCell c01 = GetCell(row, col + 1);
        GridCell c10 = GetCell(row + 1, col);
        GridCell c11 = GetCell(row + 1, col + 1);

        if (c00 == null || c01 == null || c10 == null || c11 == null)
            return null;

        return new[] { c00, c01, c10, c11 };
    }

    public bool TryPlaceLargeIcon(LargeAppIcon icon, int row, int col)
    {
        if (icon == null)
            return false;

        GridCell[] cells = TryGet2x2Block(row, col);
        if (cells == null)
        {
            Debug.LogWarning($"Large icon out of bounds at row={row}, col={col}");
            return false;
        }

        foreach (var cell in cells)
        {
            if (!cell.IsEmpty)
            {
                Debug.LogWarning($"2x2 block at row={row}, col={col} is not empty; cannot place large icon");
                return false;
            }
        }

        icon.OccupyCells(cells);
        return true;
    }

    void ApplyInitialLargeLayout()
    {
        foreach (var mapping in initialLargeLayout)
        {
            if (mapping.icon == null)
            {
                Debug.LogWarning($"Large icon layout at row={mapping.row}, col={mapping.col} has no icon assigned");
                continue;
            }

            TryPlaceLargeIcon(mapping.icon, mapping.row, mapping.col);
        }
    }

    void ApplyInitialLayout()
    {
        foreach (var mapping in initialLayout)
        {
            if (mapping.row >= rowYPositions.Length || mapping.col >= columnXPositions.Length)
            {
                Debug.LogWarning($"Initial layout out of bounds: row={mapping.row}, col={mapping.col}");
                continue;
            }

            GridCell cell = cellGrid[mapping.row, mapping.col];

            if (cell == null || !cell.IsEmpty)
            {
                Debug.LogWarning($"Cell_{mapping.row}_{mapping.col} is missing or already occupied");
                continue;
            }

            if (mapping.icon != null && mapping.folder != null)
            {
                Debug.LogWarning($"Cell_{mapping.row}_{mapping.col} has both icon and folder; assign only one");
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
                Debug.LogWarning($"Cell_{mapping.row}_{mapping.col} has no icon or folder assigned");
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
