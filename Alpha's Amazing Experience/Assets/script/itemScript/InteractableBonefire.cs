//using System.Collections.Generic;
//using UnityEngine;

//public class InteractableBonefire : MonoBehaviour, IInteractable
//{
//    [Header("生成物配置")]
//    [Tooltip("烤熟后生成的熟兔肉实体预制体")]
//    public GameObject cookedRabbitPrefab;
//    [Tooltip("烤熟的肉掉落在玩家身边的偏移量")]
//    public Vector3 dropOffset = new Vector3(1.5f, -0.5f, 0f);

//    public string ItemName => "bonefire";

//    public List<string> GetSupportedCommands()
//    {
//        return new List<string> { "cook", "look" };
//    }

//    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
//    {
//        switch (command)
//        {
//            case "look":
//                Debug.Log("这是一团温暖的篝火(bonefire)。你可以用它来烤肉(cook)。");
//                return true;

//            case "cook":
//                if (heldItem != null && heldItem.itemID == "raw rabbit")
//                {
//                    // 1. 生成产物：把熟肉实体掉落在地上
//                    if (cookedRabbitPrefab != null)
//                    {
//                        Vector3 spawnPos = playerPosition + dropOffset;
//                        Instantiate(cookedRabbitPrefab, spawnPos, Quaternion.identity);
//                        Debug.Log("烹饪成功！生兔肉变成了熟兔肉(cooked_rabbit)掉在了地上。");
//                    }

//                    // 2. 消耗食材：调用刚才写好的方法，清除手中的生肉（篝火不销毁）
//                    InventoryManager.Instance.ConsumeSelectedItem();
//                    return true;
//                }
//                else
//                {
//                    Debug.Log("烹饪失败：手里没有拿着生兔肉(raw_rabbit)！");
//                    return false;
//                }

//            default:
//                return false;
//        }
//    }
//}

using System.Collections.Generic;
using UnityEngine;

public class InteractableBonefire : MonoBehaviour, IInteractable
{
    [Header("生成物配置")]
    [Tooltip("烤熟的肉掉落在玩家身边的偏移量")]
    public Vector3 dropOffset = new Vector3(1.5f, -0.5f, 0f);

    // 【删除旧代码】：不再需要 public GameObject cookedRabbitPrefab; 
    // 生成权利已全部移交给 WorldStateManager

    public string ItemName => "bonefire";

    public List<string> GetSupportedCommands()
    {
        return new List<string> { "cook", "look" };
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                Debug.Log("这是一团温暖的篝火(bonefire)。你可以用它来烤肉(cook)。");
                return true;

            case "cook":
                if (heldItem != null && heldItem.itemID == "raw rabbit")
                {
                    // 1. 消耗食材：调用物品栏管家，清除手里的生兔肉数据
                    InventoryManager.Instance.ConsumeSelectedItem();

                    // 2. 生成产物：向世界管家申请生成熟兔肉实体，它会自动发身份证并登记在白名单
                    // 注意：必须保证传入的 ID ("cooked_rabbit") 和你在 ItemData 里填写的完全一致！
                    WorldStateManager.Instance.SpawnAndRecord("cooked rabbit", playerPosition + dropOffset);

                    Debug.Log("烹饪成功！生兔肉变成了熟兔肉(cooked_rabbit)掉在了地上。");
                    return true;
                }
                else
                {
                    Debug.Log("烹饪失败：手里没有拿着生兔肉(raw_rabbit)！");
                    return false;
                }

            default:
                return false;
        }
    }
}