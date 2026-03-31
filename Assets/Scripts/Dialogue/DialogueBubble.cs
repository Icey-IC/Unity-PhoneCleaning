using UnityEngine;
using TMPro;
using System;

public class DialogueBubble : MonoBehaviour
{
    private TextMeshProUGUI textUI;
    private Action onClick;

    void Awake()
    {
        textUI = GetComponentInChildren<TextMeshProUGUI>(true);

        if (textUI == null)
        {
            Debug.LogError("? 没找到 TextMeshProUGUI！");
        }
    }

    public void Init(string text, Action clickCallback)
    {
        if (textUI == null)
        {
            Debug.LogError("? textUI 为 null，无法设置文本");
            return;
        }

        textUI.text = text;
        onClick = clickCallback;
    }

    public void OnClick()
    {
        onClick?.Invoke();
        Destroy(gameObject);
    }
}