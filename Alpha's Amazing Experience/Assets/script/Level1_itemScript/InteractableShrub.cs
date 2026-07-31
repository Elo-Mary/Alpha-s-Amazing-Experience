using System.Collections.Generic;
using UnityEngine;

public class InteractableShrub : MonoBehaviour, IInteractable
{
    [Header("基础配置")]
    [Tooltip("物品的英文指令名")]
    public string shrubName = "shrub";

    [Tooltip("玩家输入 look 时反馈的文本")]
    public string lookDescription = "一丛茂密的灌木(shrub)。可以用斧头(Axe)或刀(Knife)砍伐获取树枝。";

    [Header("掉落配置")]
    [Tooltip("树枝(branch)掉落的坐标偏移量")]
    public Vector3 dropOffset = new Vector3(1.0f, -0.5f, 0f);

    public string ItemName => shrubName;

    public List<string> GetSupportedCommands()
    {
        return new List<string> { "cut", "look" };
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                Debug.Log(lookDescription);
                return true;

            case "cut":
                // 【核心修改】：使用 || (或) 运算符，兼容 Axe 和 Knife
                if (heldItem != null && (heldItem.itemID == "Axe" || heldItem.itemID == "Knife"))
                {
                    // 1. 生成树枝
                    // 注意：需确保管家数据库里有 itemID 为 "branch" 的 ItemData
                    WorldStateManager.Instance.SpawnAndRecord("branch", playerPosition + dropOffset);
                    Debug.Log($"枝叶飞散！你用手里锋利的工具砍断了{shrubName}，获得了一根树枝(branch)。");

                    // 2. 死亡报备，将其永久从世界中抹除
                    WorldStateManager.Instance.MarkAsDestroyed(gameObject);

                    // 3. 销毁自身
                    Destroy(gameObject);
                    return true;
                }
                else
                {
                    Debug.Log($"直接用手可拔不断它，你需要一把斧头(Axe)或刀(Knife)！");
                    return false;
                }

            default:
                return false;
        }
    }
}