using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject cellPrefab;

    [Header("自定义行和列的坐标")]
    [Tooltip("定义5行的 Y 轴坐标（建议按从上到下的顺序填写）")]
    public float[] rowYPositions = new float[5] { 1.1f, 0f, -0.9f, -1.8f, -3.2f };

    [Tooltip("定义7列的 X 轴坐标（建议按从左到右的顺序填写）")]
    public float[] columnXPositions = new float[5] { -1.6f, -0.8f, 0f, 0.8f, 1.6f };

    [Header("最后一行特殊配置")]
    [Tooltip("勾选后，最后一行将使用下方自定义的列坐标，而不是 columnXPositions")]
    public bool overrideLastRow = true;

    [Tooltip("最后一行的 X 轴坐标（留空则沿用 columnXPositions）")]
    public float[] lastRowColumnXPositions = new float[4] { -1.2f, -0.4f, 0.4f, 1.2f };

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int row = 0; row < rowYPositions.Length; row++)
        {
            // 判断是否是最后一行，且启用了覆盖配置
            bool isLastRow = (row == rowYPositions.Length - 1);
            float[] xPositions = (isLastRow && overrideLastRow && lastRowColumnXPositions.Length > 0)
                ? lastRowColumnXPositions
                : columnXPositions;

            for (int col = 0; col < xPositions.Length; col++)
            {
                Vector2 spawnPos = new Vector2(xPositions[col], rowYPositions[row]);
                GameObject cell = Instantiate(cellPrefab, spawnPos, Quaternion.identity, transform);
                cell.name = $"Cell_{row}_{col}";
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