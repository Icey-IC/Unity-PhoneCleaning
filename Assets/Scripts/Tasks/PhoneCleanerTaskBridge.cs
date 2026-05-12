using UnityEngine;

/// <summary>
/// Attach to the phone manager app screen. Wire Scan and Clean buttons (or call from OnClick).
/// </summary>
public class PhoneCleanerTaskBridge : MonoBehaviour
{
    [SerializeField] LevelTaskTracker tracker;

    LevelTaskTracker Tracker => tracker != null ? tracker : LevelTaskTracker.Instance;

    public void OnScanButtonClicked()
    {
        if (Tracker != null)
            Tracker.ReportPhoneScanClicked();
    }

    public void OnCleanButtonClicked()
    {
        if (Tracker != null)
            Tracker.ReportPhoneCleanClicked();
    }
}
