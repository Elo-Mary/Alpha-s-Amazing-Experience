using System.Collections.Generic;
using UnityEngine;

public class InteractableBrokenLadder : MonoBehaviour, IInteractable
{
    [Header("基础配置")]
    public string ladderName = "brokenLadder";
    public string lookDescription = "一个破损的梯子(brokenLadder)。可以用锤子(hammer)配合木材(Wood)与胶水(glue)将其修复(fix)。";

    [Header("物品数据引用 (用于支持拾取)")]
    [Tooltip("拖入 brokenLadder 的 ItemData 资源文件")]
    public ItemData brokenLadderItemData;

    [Header("修复产物配置")]
    [Tooltip("修好后的梯子预制体对应的物品ID (如 Ladder)")]
    public string repairedLadderID = "Ladder";

    public string ItemName => ladderName;

    public List<string> GetSupportedCommands()
    {
        // 核心设计：同时支持看、捡、修，避免组件冲突
        return new List<string> { "look", "pick", "fix" };
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                Debug.Log(lookDescription);
                return true;

            case "pick":
                if (brokenLadderItemData != null)
                {
                    // 调用现有的拾取逻辑，送入 8 格背包
                    if (InventoryManager.Instance.Pick(brokenLadderItemData, gameObject))
                    {
                        return true;
                    }
                }
                return false;

            case "fix":
                // 1. 校验手里拿的是否是锤子
                if (heldItem == null || heldItem.itemID != "hammer")
                {
                    Debug.Log("你需要手里拿着锤子(hammer)才能修复梯子！");
                    return false;
                }

                // 2. 核心校验：检查背包里是否同时拥有木材和胶水
                bool hasWood = InventoryManager.Instance.HasItem("Wood");
                bool hasGlue = InventoryManager.Instance.HasItem("glue");

                if (hasWood && hasGlue)
                {
                    // 3. 材料足够，执行扣除
                    InventoryManager.Instance.ConsumeItem("Wood");
                    InventoryManager.Instance.ConsumeItem("glue");

                    // 4. 呼叫管家在当前位置生成修好后的全新梯子 (Ladder)
                    WorldStateManager.Instance.SpawnAndRecord(repairedLadderID, transform.position);
                    Debug.Log("你挥动锤子，消耗了一份木材(Wood)和胶水(glue)，成功将梯子(Ladder)修好了！");

                    // 5. 报备世界管家，销毁当前的破损梯子实体
                    WorldStateManager.Instance.MarkAsDestroyed(gameObject);
                    Destroy(gameObject);
                    return true;
                }
                else
                {
                    // 材料不足的提示反馈
                    Debug.Log("修复失败！你没有足够的木材(Wood)或胶水(glue)。");
                    return false;
                }

            default:
                return false;
        }
    }
}