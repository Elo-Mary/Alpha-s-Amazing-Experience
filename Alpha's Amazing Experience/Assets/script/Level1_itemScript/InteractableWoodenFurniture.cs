using System.Collections.Generic;
using UnityEngine;

public class InteractableWoodenFurniture : MonoBehaviour, IInteractable
{
    [Header("基础配置")]
    [Tooltip("物品的英文指令名 (请在面板填入 desk 或 chair)")]
    public string furnitureName;

    [Tooltip("玩家输入 look 时反馈的文本")]
    public string lookDescription;

    [Header("掉落配置")]
    [Tooltip("木材(Wood)掉落的坐标偏移量")]
    public Vector3 dropOffset = new Vector3(1.0f, -0.5f, 0f);

    // 动态返回在 Inspector 中填写的名字，让雷达和大脑能正确识别
    public string ItemName => furnitureName;

    public List<string> GetSupportedCommands()
    {
        // 只支持看和砍
        return new List<string> { "cut", "look" };
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                // 容错处理：如果面板忘了填描述，给个默认提示
                string desc = string.IsNullOrEmpty(lookDescription) ? $"这是一个木制的{furnitureName}。" : lookDescription;
                Debug.Log(desc);
                return true;

            case "cut":
                // 【核心逻辑】：严格校验手里拿的是否是大写的 "Axe"
                if (heldItem != null && heldItem.itemID == "Axe")
                {
                    // 1. 呼叫管家生成大写的 "Wood"，并自动记录到白名单
                    WorldStateManager.Instance.SpawnAndRecord("Wood", playerPosition + dropOffset);
                    Debug.Log($"你用斧头劈碎了{furnitureName}，掉落了一块木材(Wood)。");

                    // 2. 死亡报备：告诉管家这个家具已被彻底销毁，以后切场景永远不要再刷出来
                    WorldStateManager.Instance.MarkAsDestroyed(gameObject);

                    // 3. 销毁自身实体
                    Destroy(gameObject);
                    return true;
                }
                else
                {
                    Debug.Log($"你需要一把斧头(Axe)才能劈碎{furnitureName}！");
                    return false;
                }

            default:
                return false;
        }
    }
}