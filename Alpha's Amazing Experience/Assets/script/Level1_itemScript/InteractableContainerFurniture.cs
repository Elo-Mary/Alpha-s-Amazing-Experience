using System.Collections.Generic;
using UnityEngine;

public class InteractableContainerFurniture : MonoBehaviour, IInteractable
{
    [Header("基础配置")]
    [Tooltip("物品的英文指令名 (如 drawer1, drawer2)")]
    public string furnitureName = "drawer";
    public string lookDescription = "这是一个柜子。";

    [Header("搜刮配置")]
    [Tooltip("里面装的物品 ID (如 cookingOil, Knife, grindstone)")]
    public string lootItemID;
    [Tooltip("搜刮掉落物品的偏移量")]
    public Vector3 lootDropOffset = new Vector3(1.0f, -0.5f, 0f);

    [Header("砍伐配置")]
    [Tooltip("是否可以被斧头砍碎？")]
    public bool canBeCut = true;
    [Tooltip("被砍碎后掉落的建材 ID")]
    public string cutDropItemID = "Wood";
    [Tooltip("建材掉落的偏移量")]
    public Vector3 woodDropOffset = new Vector3(-1.0f, -0.5f, 0f);

    // 动态返回名称
    public string ItemName => furnitureName;

    // 内部状态：记录是否已经被搜过
    // 注意：这里不能像以前那样关闭 Collider，否则搜完就无法再砍了！
    private bool isSearched = false;

    void Start()
    {
        // 加载时询问管家，是否已经被搜过
        SceneObjectID soid = GetComponent<SceneObjectID>();
        if (soid != null && WorldStateManager.Instance.GetState(soid.id) == "searched")
        {
            isSearched = true;
        }
    }

    public List<string> GetSupportedCommands()
    {
        // 动态返回支持的指令
        List<string> cmds = new List<string> { "look", "search" };
        if (canBeCut) cmds.Add("cut");
        return cmds;
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                string status = isSearched ? " 里面已经被搜空了。" : " 看起来里面藏了东西。";
                Debug.Log(lookDescription + status);
                return true;

            case "search":
                if (isSearched)
                {
                    Debug.Log($"{furnitureName} 里面空空如也，你已经搜过了。");
                    return true;
                }

                // 1. 生成内容物
                WorldStateManager.Instance.SpawnAndRecord(lootItemID, playerPosition + lootDropOffset);
                Debug.Log($"搜索成功！你在里面找到了 {lootItemID}。");

                // 2. 标记状态并上报管家
                isSearched = true;
                SceneObjectID soid = GetComponent<SceneObjectID>();
                if (soid != null) WorldStateManager.Instance.SaveState(soid.id, "searched");

                return true;

            case "cut":
                if (!canBeCut) return false;

                if (heldItem != null && heldItem.itemID == "Axe")
                {
                    // 1. 必掉木材
                    WorldStateManager.Instance.SpawnAndRecord(cutDropItemID, playerPosition + woodDropOffset);
                    Debug.Log($"你劈碎了 {furnitureName}，掉落了木材({cutDropItemID})。");

                    // 2. 核心机制：如果还没被搜过，把里面的东西也爆出来！
                    if (!isSearched && !string.IsNullOrEmpty(lootItemID))
                    {
                        WorldStateManager.Instance.SpawnAndRecord(lootItemID, playerPosition + lootDropOffset);
                        Debug.Log($"伴随着木屑飞溅，里面藏着的 {lootItemID} 也掉了出来！");
                    }

                    // 3. 死亡报备并彻底销毁自身
                    WorldStateManager.Instance.MarkAsDestroyed(gameObject);
                    Destroy(gameObject);
                    return true;
                }
                else
                {
                    Debug.Log($"你需要一把斧头(Axe)才能劈碎它！");
                    return false;
                }

            default:
                return false;
        }
    }
}