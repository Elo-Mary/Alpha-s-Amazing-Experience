using System.Collections.Generic;
using UnityEngine;

public class InteractableToolbox : MonoBehaviour, IInteractable
{
    [Header("基础配置")]
    [Tooltip("物品的英文指令名")]
    public string toolboxName = "toolbox";

    [Tooltip("玩家输入 look 时反馈的文本")]
    public string lookDescription = "一个工具箱(toolbox)。也许可以用斧头(Axe)暴力劈开(cut)。";

    [Header("掉落配置")]
    [Tooltip("电线(electricLine)掉落的坐标偏移量")]
    public Vector3 dropOffset = new Vector3(1.0f, 0.5f, 0f);

    public string ItemName => toolboxName;

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
                // 严格校验是否拿着大写的 Axe
                if (heldItem != null && heldItem.itemID == "Axe")
                {
                    // 1. 生成电线 (electricLine)
                    // 注意：需确保管家数据库里有 itemID 为 "electricLine" 的 ItemData
                    WorldStateManager.Instance.SpawnAndRecord("electricLine", playerPosition + dropOffset);
                    Debug.Log($"火花四溅！你用斧头粗暴地劈开了{toolboxName}，从里面掉出了一捆电线(electricLine)。");

                    // 2. 死亡报备，将其永久从世界中抹除
                    WorldStateManager.Instance.MarkAsDestroyed(gameObject);

                    // 3. 销毁自身
                    Destroy(gameObject);
                    return true;
                }
                else
                {
                    Debug.Log($"这个工具箱十分坚固，你需要一把斧头(Axe)才能劈开它！");
                    return false;
                }

            default:
                return false;
        }
    }
}