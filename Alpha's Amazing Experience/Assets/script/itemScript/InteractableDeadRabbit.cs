using System.Collections.Generic;
using UnityEngine;

public class InteractableDeadRabbit : MonoBehaviour, IInteractable
{
    [Header("基础配置")]
    [Tooltip("死兔的物品数据档案")]
    public ItemData itemData;

    [Header("生成物配置")]
    [Tooltip("剥皮后生成的生兔肉实体预制体")]
    public GameObject rawRabbitPrefab;
    [Tooltip("掉落在玩家身边的偏移量")]
    public Vector3 dropOffset = new Vector3(1.5f, 0f, 0f);

    public string ItemName => "dead_rabbit";

    public List<string> GetSupportedCommands()
    {
        return new List<string> { "peel", "look", "pick" };
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                Debug.Log("这是一只死兔(dead_rabbit)。你需要一把刀(knife)来进行剥皮(peel)。");
                return true;

            case "peel":
                if (heldItem != null && heldItem.itemID == "knife")
                {
                    WorldStateManager.Instance.SpawnAndRecord("raw rabbit", transform.position);
                    Debug.Log("死兔剥皮为生兔");
                    WorldStateManager.Instance.MarkAsDestroyed(gameObject);
                    Destroy(gameObject); // 销毁死兔实体
                    return true;
                }
                else
                {
                    Debug.Log("剥皮失败：你需要手里拿着刀(knife)！");
                    return false;
                }

            case "pick":
                if (itemData == null)
                {
                    Debug.LogError("死兔的数据配置丢失！");
                    return false;
                }
                // 复用全局拾取逻辑
                return InventoryManager.Instance.Pick(itemData, gameObject);

            default:
                return false;
        }
    }
}