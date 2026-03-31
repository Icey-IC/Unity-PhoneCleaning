using UnityEngine;

public class FolderOverlay : MonoBehaviour
{
    private FolderPanel currentPanel;

    public void Show(FolderPanel panel)
    {
        currentPanel = panel;
        gameObject.SetActive(true);

        SetZ(-7f); // 在 panel 后面一层
    }

    public void Hide()
    {
        currentPanel = null;
        gameObject.SetActive(false);
    }

    void OnMouseDown()
    {
        if (currentPanel != null)
        {
            // ? 必须走 Owner 关闭（修复只能打开一次 bug）
            currentPanel.Owner.ClosePanel();
        }
    }

    void SetZ(float z)
    {
        Vector3 pos = transform.position;
        pos.z = z;
        transform.position = pos;
    }
}