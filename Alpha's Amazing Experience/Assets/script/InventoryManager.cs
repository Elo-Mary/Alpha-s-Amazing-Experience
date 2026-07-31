using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // 核心数据：长度固定的 8 个槽位
    private ItemData[] slots = new ItemData[8];
    public int CurrentSelectedIndex { get; private set; } = 0;

    // 事件广播：当物品数据变化或选中项变化时通知 UI
    public static event Action<ItemData[]> OnInventoryUpdated;
    public static event Action<int> OnSlotSelectionChanged;
    // 在变量定义区新增事件
    public static event System.Action<string> OnItemPickedUp;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // 【新增】让挂载它的 __Managers 跨场景不被销毁
        }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        // 游戏开始时初始化广播一次，确保 UI 状态同步
        OnInventoryUpdated?.Invoke(slots);
        OnSlotSelectionChanged?.Invoke(CurrentSelectedIndex);
    }

    // 切换当前选中的槽位 (传入索引 0-7)
    public void SelectSlot(int index)
    {
        if (index >= 0 && index < 8)
        {
            CurrentSelectedIndex = index;
            OnSlotSelectionChanged?.Invoke(CurrentSelectedIndex);
        }
    }

    // 获取当前手中拿着的物品数据（供解析器校验）
    public ItemData GetSelectedItem()
    {
        return slots[CurrentSelectedIndex];
    }

    // 拾取物品逻辑
    public bool Pick(ItemData itemData, GameObject sceneObject)
    {
        if (slots[CurrentSelectedIndex] == null)
        {
            slots[CurrentSelectedIndex] = itemData; // 存入数据
            // 【新增】告诉世界管家，这个物体已经“死”了，以后切场景不要再刷出来
            WorldStateManager.Instance.MarkAsDestroyed(sceneObject);
            Destroy(sceneObject);                   // 销毁场景中的实体

            OnInventoryUpdated?.Invoke(slots);      // 刷新UI

            // 在 Pick() 方法的 if (slots[CurrentSelectedIndex] == null) 大括号里的【最后面】加上：
            OnItemPickedUp?.Invoke(itemData.itemID);

            Debug.Log($"[物品栏] 拾取了物品: {itemData.itemID} 到槽位 {CurrentSelectedIndex + 1}");
            return true;
        }
        else
        {
            Debug.Log($"[物品栏] 当前选中的槽位 {CurrentSelectedIndex + 1} 已满，无法拾取！");
            return false;
        }
    }

    // 丢弃/放置物品逻辑 (严格接收槽位索引与生成坐标两个参数)
    public void DropFromInventory(int slotIndex, Vector3 dropPosition)
    {
        if (slotIndex >= 0 && slotIndex < 8 && slots[slotIndex] != null)
        {
            ItemData dataToDrop = slots[slotIndex];
            slots[slotIndex] = null; // 清空内部数据

            if (dataToDrop.itemPrefab != null)
            {
                // 生成掉落物
                GameObject droppedObj = Instantiate(dataToDrop.itemPrefab, dropPosition, Quaternion.identity);

                // 【新增】给它发一张新的动态身份证
                string newDynamicID = System.Guid.NewGuid().ToString();
                SceneObjectID soid = droppedObj.GetComponent<SceneObjectID>();
                if (soid == null) soid = droppedObj.AddComponent<SceneObjectID>();
                soid.id = newDynamicID;

                // 【新增】告诉管家，把这件物品记在当前场景的白名单上
                WorldStateManager.Instance.RecordDroppedItem(
                    SceneManager.GetActiveScene().name,
                    dataToDrop.itemID,
                    dropPosition,
                    newDynamicID
                );
            }

            OnInventoryUpdated?.Invoke(slots); // 刷新UI
        }
    }

    // 新增：直接消耗当前选中槽位里的物品（用于烹饪、吃喝等不产生掉落物的场景）
    public void ConsumeSelectedItem()
    {
        if (slots[CurrentSelectedIndex] != null)
        {
            string consumedID = slots[CurrentSelectedIndex].itemID;
            slots[CurrentSelectedIndex] = null; // 清空内部数据

            OnInventoryUpdated?.Invoke(slots);  // 刷新UI，对应的格子会变空
            Debug.Log($"[物品栏] 消耗了槽位 {CurrentSelectedIndex + 1} 的物品: {consumedID}");
        }
    }

    // 新增清空物品栏的方法
    public void ClearInventory()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = null;
        }
        CurrentSelectedIndex = 0;
        OnInventoryUpdated?.Invoke(slots);
        Debug.Log("[物品栏] 跨关卡清空完毕！");
    }

    // 新增：检查背包中是否存在指定 ID 的物品
    public bool HasItem(string itemID)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].itemID == itemID)
            {
                return true;
            }
        }
        return false;
    }

    // 新增：消耗背包中指定 ID 的一个物品，并刷新 UI
    public void ConsumeItem(string itemID)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].itemID == itemID)
            {
                slots[i] = null; // 移除该物品
                OnInventoryUpdated?.Invoke(slots); // 触发 UI 刷新事件
                Debug.Log($"[物品栏] 因合成/修复消耗了物品: {itemID}");
                return; // 每次只消耗一个，直接退出循环
            }
        }
    }

    // 新增：统计背包中特定 ID 物品的总数量
    public int GetItemCount(string itemID)
    {
        int count = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].itemID == itemID)
            {
                count++;
            }
        }
        return count;
    }
}