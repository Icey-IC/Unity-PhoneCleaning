using UnityEngine;

public class AppView : MonoBehaviour
{
    public GameObject PhoneBGMask;

    /// <summary>
    /// 关闭当前 App 页面
    /// </summary>
    public void Close()
    {
        gameObject.SetActive(false);
        if(PhoneBGMask != null)
            PhoneBGMask.SetActive(true);
    }
}