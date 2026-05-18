using UnityEngine;

public class AppView : MonoBehaviour
{
    /// <summary>
    /// 关闭当前 App 页面
    /// </summary>
    public void Close()
    {
        gameObject.SetActive(false);
    }
}