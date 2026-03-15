using UnityEngine;
using UnityEngine.UI;

public class AppContextMenu : MonoBehaviour
{
    private AppIcon owner;

    [Header("按钮绑定")]
    public Button uninstallButton;
    public Button settingsButton;

    public void Init(AppIcon appIcon)
    {
        owner = appIcon;

        // 在代码里绑定按钮事件，不需要在Inspector里手动拖
        if (uninstallButton != null)
            uninstallButton.onClick.AddListener(OnUninstallClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void OnUninstallClicked()
    {
        Debug.Log($"卸载 {owner.gameObject.name}");
        // TODO: 卸载逻辑
        Hide();
    }

    void OnSettingsClicked()
    {
        Debug.Log($"设置 {owner.gameObject.name}");
        // TODO: 设置逻辑
        Hide();
    }
}