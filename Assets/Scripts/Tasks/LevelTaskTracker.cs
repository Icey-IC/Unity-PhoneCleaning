using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Tracks five level objectives, updates task list UI, shows submit when all done.
/// Wire task line TMPs (5), submit root, and configure lists in Inspector.
/// </summary>
[DefaultExecutionOrder(-10)]
public class LevelTaskTracker : MonoBehaviour
{
    public static LevelTaskTracker Instance { get; private set; }

    public enum TaskIndex
    {
        PhoneClean = 0,
        UninstallJunk = 1,
        FolderSort = 2,
        Notifications = 3,
        GalleryClean = 4
    }

    const int TaskCount = 5;

    [Header("UI")]
    [Tooltip("Five lines, same order as TaskIndex.")]
    public TextMeshProUGUI[] taskLineLabels = new TextMeshProUGUI[TaskCount];

    [Tooltip("Hidden until all tasks complete; place bottom-right on task panel.")]
    public GameObject submitButtonRoot;

    [Header("Phone manager (管家)")]
    [Tooltip("Player must tap Detect then Clean inside the manager app.")]
    public bool requirePhoneCleanTask = true;

    [Header("Uninstall")]
    [Tooltip("App IDs that must be uninstalled (no AppIcon left with that id).")]
    public List<string> appIdsMustBeUninstalled = new List<string>();

    [Header("Notifications")]
    [Tooltip("If true, every AppIcon in the scene must have a matching entry below.")]
    public bool requireNotificationRuleForEveryApp = true;

    public List<NotificationRequirement> notificationRequirements = new List<NotificationRequirement>();

    [Header("Submit")]
    public UnityEvent onSubmitClicked;
    [Tooltip("If enabled, loads build-index scene after invoking the event above.")]
    public bool alsoLoadNextScene;
    public string nextSceneName;

    bool phoneScanDone = true;
    bool phoneCleanDone;
    readonly bool[] taskDone = new bool[TaskCount];
    readonly string[] taskBaseText = new string[TaskCount];
    bool submitShown;

    void Awake()
    {
        Instance = this;
        if (submitButtonRoot != null)
            submitButtonRoot.SetActive(false);
    }

    void Start()
    {
        for (int i = 0; i < taskLineLabels.Length && i < TaskCount; i++)
        {
            if (taskLineLabels[i] != null)
                taskBaseText[i] = taskLineLabels[i].text;
        }
    }

    void LateUpdate()
    {
        RefreshAllTasks();
    }

    public void ReportPhoneScanClicked()
    {
        phoneScanDone = true;
    }

    public void ReportPhoneCleanClicked()
    {
        if (phoneScanDone)
            phoneCleanDone = true;
    }

    /// <summary>Call from buttons on the phone manager app view.</summary>
    public void ResetPhoneCleanProgress()
    {
        phoneScanDone = true;
        phoneCleanDone = false;
    }

    void RefreshAllTasks()
    {
        SetTask(TaskIndex.PhoneClean, !requirePhoneCleanTask || (phoneScanDone && phoneCleanDone));
        SetTask(TaskIndex.UninstallJunk, EvaluateUninstall());
        SetTask(TaskIndex.FolderSort, EvaluateFolderSort());
        SetTask(TaskIndex.Notifications, EvaluateNotifications());
        SetTask(TaskIndex.GalleryClean, EvaluateGallery());

        if (submitShown && !AllTasksDone())
        {
            submitShown = false;
            if (submitButtonRoot != null)
                submitButtonRoot.SetActive(false);
        }
        else if (!submitShown && AllTasksDone())
        {
            submitShown = true;
            if (submitButtonRoot != null)
                submitButtonRoot.SetActive(true);
        }
    }

    bool AllTasksDone()
    {
        for (int i = 0; i < TaskCount; i++)
        {
            if (!taskDone[i]) return false;
        }
        return true;
    }

    void SetTask(TaskIndex index, bool done)
    {
        int i = (int)index;
        if (i < 0 || i >= TaskCount) return;
        if (taskDone[i] == done) return;

        taskDone[i] = done;
        ApplyLineVisual(i, done);
    }

    void ApplyLineVisual(int index, bool done)
    {
        if (taskLineLabels == null || index >= taskLineLabels.Length || taskLineLabels[index] == null)
            return;

        var tmp = taskLineLabels[index];
        string raw = taskBaseText[index];
        if (string.IsNullOrEmpty(raw))
            raw = StripRichTextDone(tmp.text);

        if (done)
        {
            string safe = (raw ?? "").Replace("</noparse>", string.Empty, System.StringComparison.Ordinal);
            tmp.text = $"<color=#888888><s><noparse>{safe}</noparse></s></color>";
        }
        else
            tmp.text = raw;
    }

    static string StripRichTextDone(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Replace("<color=#888888><s><noparse>", "").Replace("</noparse></s></color>", "");
    }

    bool EvaluateUninstall()
    {
        if (appIdsMustBeUninstalled == null || appIdsMustBeUninstalled.Count == 0)
            return true;

        foreach (var id in appIdsMustBeUninstalled)
        {
            if (string.IsNullOrEmpty(id)) continue;
            if (AppIcon.FindByAppId(id) != null)
                return false;
        }
        return true;
    }

    bool EvaluateFolderSort()
    {
        var icons = FindObjectsOfType<AppIcon>(true);
        var folders = FindObjectsOfType<FolderIcon>(true);
        foreach (var icon in icons)
        {
            if (icon == null || string.IsNullOrEmpty(icon.appID)) continue;
            if (!IconSatisfiesFolderAssignment(icon, folders))
                return false;
        }
        return true;
    }

    static bool IconSatisfiesFolderAssignment(AppIcon icon, FolderIcon[] folders)
    {
        var homes = new List<FolderIcon>();
        foreach (var f in folders)
        {
            if (f == null || f.allowedAppIDs == null) continue;
            if (f.allowedAppIDs.Contains(icon.appID))
                homes.Add(f);
        }

        if (homes.Count == 0)
            return true;

        if (!icon.isInFolder)
            return false;

        var panel = GetFolderPanelForIcon(icon);
        if (panel == null || panel.Owner == null)
            return false;

        return homes.Contains(panel.Owner);
    }

    static FolderPanel GetFolderPanelForIcon(AppIcon icon)
    {
        if (icon?.currentCell == null) return null;
        if (icon.currentCell.ownerFolderPanel != null)
            return icon.currentCell.ownerFolderPanel;
        return icon.currentCell.GetComponentInParent<FolderPanel>();
    }

    bool EvaluateNotifications()
    {
        var icons = FindObjectsOfType<AppIcon>(true);
        if (notificationRequirements == null)
            notificationRequirements = new List<NotificationRequirement>();

        if (requireNotificationRuleForEveryApp)
        {
            foreach (var icon in icons)
            {
                if (icon == null || string.IsNullOrEmpty(icon.appID)) continue;
                var req = notificationRequirements.Find(r => r.appId == icon.appID);
                if (req == null)
                    return false;
                if (icon.notificationsAllowed != req.notificationsMustBeOn)
                    return false;
            }
            return true;
        }

        foreach (var req in notificationRequirements)
        {
            if (string.IsNullOrEmpty(req.appId)) continue;
            var icon = AppIcon.FindByAppId(req.appId);
            if (icon == null) continue;
            if (icon.notificationsAllowed != req.notificationsMustBeOn)
                return false;
        }
        return true;
    }

    bool EvaluateGallery()
    {
        // Count PhotoItems in loaded scenes (includes inactive under closed gallery UI), not only GalleryManager.allPhotos
        // (Awake may not have run on disabled objects, so the list can be empty until the player opens the app).
        return CountDeletablePhotoItemsInLoadedScenes() == 0;
    }

    static int CountDeletablePhotoItemsInLoadedScenes()
    {
        int n = 0;
        var photos = Object.FindObjectsOfType<PhotoItem>(true);
        for (int i = 0; i < photos.Length; i++)
        {
            var p = photos[i];
            if (p == null || !p.canDelete) continue;
            if (!p.gameObject.scene.IsValid()) continue;
            n++;
        }
        return n;
    }

    public void OnSubmitPressed()
    {
        onSubmitClicked?.Invoke();
        if (alsoLoadNextScene && !string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}

[System.Serializable]
public class NotificationRequirement
{
    public string appId;
    public bool notificationsMustBeOn;
}
