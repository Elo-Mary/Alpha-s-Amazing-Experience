using System.Collections.Generic;
using UnityEngine;

public class InteractableTree : MonoBehaviour, IInteractable
{
    [Header("基础配置")]
    [Tooltip("物品的英文指令名")]
    public string treeName = "tree";

    [Tooltip("玩家输入 look 时反馈的文本")]
    public string lookDescription = "一棵粗壮的树(tree)。可以用斧头(Axe)将其砍倒。";

    [Header("掉落配置")]
    [Tooltip("木材(Wood)掉落的坐标偏移量")]
    public Vector3 dropOffset = new Vector3(1.0f, -0.5f, 0f);

    public string ItemName => treeName;

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
                    // 1. 生成大写的 Wood
                    WorldStateManager.Instance.SpawnAndRecord("Wood", playerPosition + dropOffset);
                    Debug.Log($"木屑飞溅！你砍倒了{treeName}，获得了一块木材(Wood)。");

                    // 2. 死亡报备，将其永久从世界中抹除
                    WorldStateManager.Instance.MarkAsDestroyed(gameObject);

                    // 3. 销毁自身
                    Destroy(gameObject);
                    return true;
                }
                else
                {
                    Debug.Log($"这棵树太粗壮了，你需要一把斧头(Axe)才能砍伐！");
                    return false;
                }

            default:
                return false;
        }
    }
}