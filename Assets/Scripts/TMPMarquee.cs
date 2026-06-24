using UnityEngine;
using TMPro;

public class TMPMarquee : MonoBehaviour
{
    public float speed = 100f; // 像素/秒

    private RectTransform rectTransform;
    private float textWidth;
    private float parentWidth;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        // 获取文本宽度
        TMP_Text text = GetComponent<TMP_Text>();
        text.ForceMeshUpdate();
        textWidth = text.preferredWidth;

        // 父物体宽度（显示区域）
        parentWidth = ((RectTransform)transform.parent).rect.width;

        // 初始位置：在右边外侧
        rectTransform.anchoredPosition =
            new Vector2(parentWidth, rectTransform.anchoredPosition.y);
    }

    void Update()
    {
        rectTransform.anchoredPosition +=
            Vector2.left * speed * Time.deltaTime;

        // 完全离开左边后重置
        if (rectTransform.anchoredPosition.x < -textWidth)
        {
            rectTransform.anchoredPosition =
                new Vector2(parentWidth,
                            rectTransform.anchoredPosition.y);
        }
    }
}