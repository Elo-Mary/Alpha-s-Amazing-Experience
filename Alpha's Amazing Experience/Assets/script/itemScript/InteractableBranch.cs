using System.Collections.Generic;
using UnityEngine;

public class InteractableBranch : MonoBehaviour, IInteractable
{
    [Header("基础配置")]
    [Tooltip("树枝的物品数据档案")]
    public ItemData itemData;

    [Header("生成物配置")]
    [Tooltip("执行set指令后生成的柴堆预制体")]
    public GameObject woodPrefab;
    [Tooltip("布置柴堆时，相对于玩家位置的偏移量")]
    public Vector3 setOffset = new Vector3(1.5f, 0f, 0f);

    // 物品标识名称
    public string ItemName => itemData != null ? itemData.itemID : "branch";

    public List<string> GetSupportedCommands()
    {
        // 树枝支持拾取、观察，以及特殊的布置指令
        return new List<string> { "pick", "look", "set" };
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                Debug.Log($"这是一些 {ItemName}。你可以把它捡起来 (pick)，或者就地布置 (set) 成柴堆。");
                return true;

            case "pick":
                if (itemData == null)
                {
                    Debug.LogError("树枝的数据配置丢失！");
                    return false;
                }
                // 复用全局拾取逻辑
                return InventoryManager.Instance.Pick(itemData, gameObject);

            case "set":
                // 1. 申请生成柴堆
                WorldStateManager.Instance.SpawnAndRecord("wood", playerPosition + setOffset);
                Debug.Log("布置成功！树枝被搭成了一个柴堆(wood)。");

                // 2. 死前报备黑名单
                WorldStateManager.Instance.MarkAsDestroyed(gameObject);

                // 3. 销毁自身
                Destroy(gameObject);
                return true;

            default:
                return false;
        }
    }
}