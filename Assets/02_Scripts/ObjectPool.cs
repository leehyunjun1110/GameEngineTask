using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class PoolItem
    {
        public GameObject prefab;
        public int initialSize = 5;
        public Queue<GameObject> poolQueue = new Queue<GameObject>();
    }

    public List<PoolItem> items;
    private Dictionary<string, PoolItem> poolDictionary;

    void Awake()
    {
        // 빠른 검색을 위한 딕셔너리 초기화
        poolDictionary = new Dictionary<string, PoolItem>();

        foreach (var item in items)
        {
            poolDictionary[item.prefab.name] = item;
        }
    }

    void Start()
    {
        // 각 풀 아이템 초기화
        foreach (var item in items)
        {
            InitializePool(item);
        }
    }

    private void InitializePool(PoolItem item)
    {
        // 부모 오브젝트 생성 (정리를 위함)
        GameObject poolParent = new GameObject($"Pool_{item.prefab.name}");
        poolParent.transform.SetParent(transform);

        for (int i = 0; i < item.initialSize; i++)
        {
            GameObject obj = CreateNewPoolObject(item.prefab, poolParent.transform);
            item.poolQueue.Enqueue(obj);
        }
    }

    private GameObject CreateNewPoolObject(GameObject prefab, Transform parent)
    {
        GameObject obj = Instantiate(prefab, parent);
        obj.SetActive(false);
        return obj;
    }

    public GameObject GetFromPool(GameObject prefab)
    {
        if (!poolDictionary.TryGetValue(prefab.name, out PoolItem poolItem))
        {
            Debug.LogWarning($"Prefab {prefab.name} not found in pool. Creating new pool.");

            // 새 풀 아이템 생성
            PoolItem newItem = new PoolItem { prefab = prefab };
            items.Add(newItem);
            poolDictionary[prefab.name] = newItem;
            InitializePool(newItem);
            poolItem = newItem;
        }

        // 비활성화된 오브젝트 찾기
        GameObject obj = null;
        foreach (GameObject pooledObj in poolItem.poolQueue)
        {
            if (!pooledObj.activeInHierarchy)
            {
                obj = pooledObj;
                break;
            }
        }

        // 모든 오브젝트가 사용중이면 새로 생성
        if (obj == null)
        {
            Transform parent = transform.Find($"Pool_{prefab.name}");
            if (parent == null)
            {
                parent = new GameObject($"Pool_{prefab.name}").transform;
                parent.SetParent(transform);
            }

            obj = CreateNewPoolObject(prefab, parent);
            poolItem.poolQueue.Enqueue(obj);
        }

        obj.SetActive(true);
        return obj;
    }

    // 오브젝트를 풀로 반환
    public void ReturnToPool(GameObject obj, float delay = 0f)
    {
        if (delay > 0)
            StartCoroutine(ReturnToPoolAfterDelay(obj, delay));
        else
            obj.SetActive(false);
    }

    private IEnumerator ReturnToPoolAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
            obj.SetActive(false);
    }
}