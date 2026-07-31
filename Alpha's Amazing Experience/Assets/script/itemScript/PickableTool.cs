using System.Collections.Generic;
using UnityEngine;

// 通用可拾取工具类（适用于 Axe, Knife, Lighter, Gun 等）
public class PickableTool : MonoBehaviour, IInteractable
{
    [Header("工具数据配置")]
    [Tooltip("必须挂载对应的 ScriptableObject 数据 (例如 AxeData)")]
    public ItemData itemData;

    // 直接读取数据配置中的 ID，防止手写字符串出错
    public string ItemName => itemData != null ? itemData.itemID : "unknown_tool";

    public List<string> GetSupportedCommands()
    {
        // 基础工具目前都支持 pick 和 look
        return new List<string> { "pick", "look" };
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                Debug.Log($"这是一把 {ItemName}，你可以把它捡起来 (pick)。");
                return true;

            case "pick":
                if (itemData == null)
                {
                    Debug.LogError($"[{gameObject.name}] 丢失了 ItemData 引用！");
                    return false;
                }

                // 调用全局物品栏进行拾取
                // InventoryManager 内部会判断槽位是否已满。若未满，存入数据并返回 true
                bool success = InventoryManager.Instance.Pick(itemData, gameObject);

                if (success)
                {
                    // 注意：销毁 GameObject 的操作已经在 InventoryManager.Pick 内部完成了
                    Debug.Log($"成功拾取了 {ItemName}！");
                }
                else
                {
                    Debug.Log("物品栏已满，或者当前槽位被占用，无法拾取！");
                }
                return success;

            default:
                return false; // 不支持的指令拦截
        }
    }
}