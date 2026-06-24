using UnityEngine;

public class RandomADSpawner : MonoBehaviour
{
    [Header("可随机的Prefab列表")]
    public GameObject[] prefabs;

    private GameObject currentChild;

    void OnEnable()
    {
        SpawnRandomChild();
    }

    void OnDisable()
    {
        ClearChild();
    }

    void SpawnRandomChild()
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("没有设置可用的Prefab！");
            return;
        }

        // 每次激活前先清理旧的
        ClearChild();

        // 随机一个Prefab
        int index = Random.Range(0, prefabs.Length);
        GameObject prefab = prefabs[index];

        // 生成并作为子物体
        currentChild = Instantiate(prefab, transform);
        //currentChild.transform.localPosition = Vector3.zero;
        //currentChild.transform.localRotation = Quaternion.identity;
        //currentChild.transform.localScale = Vector3.one;
    }
    public void SpawnRandomPrefab()
    {
        SpawnRandomChild();
    }

    void ClearChild()
    {
        if (currentChild != null)
        {
            Destroy(currentChild);
            currentChild = null;
        }
    }
}