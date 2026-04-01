using UnityEngine;

public class UninstallButton : MonoBehaviour
{
    private AppIcon appIcon;

    void Awake()
    {
        appIcon = GetComponentInParent<AppIcon>();
    }

    public void OnClickUninstall()
    {
        if (appIcon != null)
            appIcon.Uninstall();
    }
}