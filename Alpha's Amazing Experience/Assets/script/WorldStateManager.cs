using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldStateManager : MonoBehaviour
{
    public static WorldStateManager Instance { get; private set; }

    [Header("全局物品数据库")]
    [Tooltip("必须把项目里所有的 ItemData (比如 AxeData, BranchData) 拖进这里！")]
    public List<ItemData> allItems = new List<ItemData>();

    // 【死亡黑名单】：记录被拿走或销毁的物体 ID
    private HashSet<string> destroyedIDs = new HashSet<string>();

    // 【动态白名单】：记录玩家在各个场景丢弃的物品
    [System.Serializable]
    public class DroppedItem
    {
        public string dynamicID; // 丢下时分配的新身份证
        public string itemID;    // 物品数据的名字 (比如 "axe")
        public Vector3 position; // 丢在哪了
    }
    private Dictionary<string, List<DroppedItem>> sceneDroppedItems = new Dictionary<string, List<DroppedItem>>();

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    // 1. 记入死亡黑名单 (被捡起、被烧毁时调用)
    public void MarkAsDestroyed(GameObject obj)
    {
        SceneObjectID soid = obj.GetComponent<SceneObjectID>();
        if (soid != null && !string.IsNullOrEmpty(soid.id))
        {
            destroyedIDs.Add(soid.id);
        }
    }

    // 2. 记录动态生成的物品 (玩家扔东西时调用)
    public void RecordDroppedItem(string sceneName, string itemID, Vector3 pos, string newID)
    {
        if (!sceneDroppedItems.ContainsKey(sceneName))
        {
            sceneDroppedItems[sceneName] = new List<DroppedItem>();
        }
        sceneDroppedItems[sceneName].Add(new DroppedItem { dynamicID = newID, itemID = itemID, position = pos });
    }

    // ================== 【新增：状态记录本】 ==================
    // 记录那些没有被销毁，但是形态发生改变的物体（如：被砍过的灌木丛）
    private Dictionary<string, string> objectStates = new Dictionary<string, string>();

    public void SaveState(string id, string state)
    {
        if (!string.IsNullOrEmpty(id)) objectStates[id] = state;
    }

    public string GetState(string id)
    {
        return objectStates.ContainsKey(id) ? objectStates[id] : "";
    }

    // ================== 【新增：全局统一生成接口】 ==================
    // 所有不经过背包的、在场景中“私自”生成的持久化物品，都必须调用这个方法！
    public GameObject SpawnAndRecord(string itemID, Vector3 position)
    {
        // 查数据库找预制体
        ItemData data = allItems.Find(x => x.itemID == itemID);
        if (data != null && data.itemPrefab != null)
        {
            // 生成实体
            GameObject newObj = Instantiate(data.itemPrefab, position, Quaternion.identity);

            // 当场给它发一张新的动态身份证
            string newDynamicID = System.Guid.NewGuid().ToString();
            SceneObjectID soid = newObj.GetComponent<SceneObjectID>();
            if (soid == null) soid = newObj.AddComponent<SceneObjectID>();
            soid.id = newDynamicID;

            // 立刻记入白名单！
            RecordDroppedItem(SceneManager.GetActiveScene().name, itemID, position, newDynamicID);
            return newObj;
        }
        else
        {
            Debug.LogError($"[管家] 无法生成！数据库中找不到 ID 为 '{itemID}' 的配置。");
            return null;
        }
    }

    // ================== 核心逻辑：每次场景加载完成时执行 ==================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // A. 处决黑名单物体 (清理掉场景自带的、但已经被你捡走的物品)
        SceneObjectID[] allObjects = FindObjectsOfType<SceneObjectID>();
        foreach (var obj in allObjects)
        {
            if (destroyedIDs.Contains(obj.id))
            {
                Destroy(obj.gameObject);
            }
        }

        // B. 生成白名单物体 (把你上次在这个场景扔在地上的东西还原出来)
        if (sceneDroppedItems.ContainsKey(scene.name))
        {
            foreach (var dropped in sceneDroppedItems[scene.name])
            {
                // 如果这个丢弃物后来又被你捡走了，那就不生成它
                if (destroyedIDs.Contains(dropped.dynamicID)) continue;

                ItemData data = allItems.Find(x => x.itemID == dropped.itemID);
                if (data != null && data.itemPrefab != null)
                {
                    GameObject newObj = Instantiate(data.itemPrefab, dropped.position, Quaternion.identity);

                    // 给重新生成的物体赋予它当初登记的动态 ID
                    SceneObjectID soid = newObj.GetComponent<SceneObjectID>();
                    if (soid == null) soid = newObj.AddComponent<SceneObjectID>();
                    soid.id = dropped.dynamicID;
                }
            }
        }
    }

    // 新增清空世界记忆的方法
    public void ClearAllStates()
    {
        destroyedIDs.Clear();
        sceneDroppedItems.Clear();
        objectStates.Clear();
        Debug.Log("[世界管家] 关卡记忆已彻底清空！");
    }

    // 检查某个 ID 是否已经被记录为销毁
    public bool IsDestroyed(string targetID)
    {
        // 假设你用来存销毁记录的集合叫 destroyedIDs
        return destroyedIDs.Contains(targetID);
    }
}