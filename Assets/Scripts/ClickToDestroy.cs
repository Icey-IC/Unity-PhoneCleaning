using UnityEngine;

public class ClickToDestroy : MonoBehaviour
{
    public void DeleteSelf()
    {
        Destroy(gameObject);
    }
}