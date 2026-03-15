using UnityEngine;

public class GridCell : MonoBehaviour
{
    public Color normalColor = new Color(0.8f, 0.8f, 0.8f, 0.5f); // 默认半透明灰色
    public Color hoverColor = Color.white;                        // 鼠标悬停变白

    private SpriteRenderer sr;
    public AppIcon currentIcon; // 当前占据这个格子的图标

    // 判断当前格子是否为空
    public bool IsEmpty => currentIcon == null;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = normalColor;
    }

    // 鼠标悬停且为空时高亮
    public void Highlight()
    {
        if (IsEmpty)
        {
            sr.color = hoverColor;
        }
    }

    // 取消高亮
    public void Unhighlight()
    {
        sr.color = normalColor;
    }

    // 将图标放置在这个格子上
    public void SetIcon(AppIcon icon)
    {
        currentIcon = icon;
        // 让图标的坐标完美吸附到格子中心
        icon.transform.position = transform.position;
        icon.currentCell = this;
    }

    // 移除图标
    public void RemoveIcon()
    {
        currentIcon = null;
    }
}