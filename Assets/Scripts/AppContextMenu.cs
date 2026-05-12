using UnityEngine;
using UnityEngine.UI;

public class AppContextMenu : MonoBehaviour
{
    private AppIcon owner;

    [Header("Buttons")]
    public Button uninstallButton;
    public Button settingsButton;

    [Header("Notifications")]
    [Tooltip("Optional: assign the Toggle under panel. If empty, tries panel/AllowNotifications then Unicode path for legacy hierarchy.")]
    public Toggle notificationsToggle;

    public void Init(AppIcon appIcon)
    {
        owner = appIcon;

        ResolveNotificationsToggle();

        if (uninstallButton != null)
            uninstallButton.onClick.AddListener(OnUninstallClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (notificationsToggle != null)
        {
            notificationsToggle.onValueChanged.RemoveListener(OnNotificationsToggled);
            notificationsToggle.onValueChanged.AddListener(OnNotificationsToggled);
            notificationsToggle.SetIsOnWithoutNotify(owner.notificationsAllowed);
        }
    }

    void ResolveNotificationsToggle()
    {
        if (notificationsToggle != null) return;
        var t = transform.Find("panel/AllowNotifications");
        if (t == null)
            t = transform.Find("panel/\u5141\u8bb8\u901a\u77e5");
        if (t != null)
            notificationsToggle = t.GetComponent<Toggle>();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        if (notificationsToggle != null && owner != null)
            notificationsToggle.SetIsOnWithoutNotify(owner.notificationsAllowed);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void OnUninstallClicked()
    {
        if (owner != null)
            owner.Uninstall();
    }

    void OnNotificationsToggled(bool on)
    {
        if (owner != null)
            owner.SetNotificationsAllowed(on);
    }

    void OnSettingsClicked()
    {
        Debug.Log($"Settings {owner.gameObject.name}");
        // TODO: settings behaviour
        Hide();
    }
}
