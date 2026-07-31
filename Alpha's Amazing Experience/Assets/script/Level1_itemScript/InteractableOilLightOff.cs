using System.Collections.Generic;
using UnityEngine;

public class InteractableOilLightOff : MonoBehaviour, IInteractable
{
    [Header("基础配置")]
    public string lightName = "oilLightOff";
    public string lookDescription = "一盏熄灭的油灯(oilLightOff)。可以用食用油(cookingOil)或满的油箱(oilBox_full)将其点燃(fire/add)。";

    [Header("物品数据引用 (用于支持自身拾取)")]
    [Tooltip("拖入 oilLightOff 的 ItemData 资源文件")]
    public ItemData oilLightOffItemData;

    [Header("点燃产物配置")]
    [Tooltip("点燃后的油灯预制体对应的物品ID (即 oilLight)")]
    public string litLightID = "oilLight";

    public string ItemName => lightName;

    public List<string> GetSupportedCommands()
    {
        // 同时支持观察、拾取、点燃和添加燃料
        return new List<string> { "look", "pick", "fire", "add" };
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                Debug.Log(lookDescription);
                return true;

            case "pick":
                if (oilLightOffItemData != null)
                {
                    // 调用物品栏管理器将其收入背包
                    if (InventoryManager.Instance.Pick(oilLightOffItemData, gameObject))
                    {
                        return true;
                    }
                }
                return false;

            case "fire":
            case "add":
                // 核心校验：检查手里拿的是否是食用油或满油箱
                if (heldItem != null && (heldItem.itemID == "cookingOil" || heldItem.itemID == "oilBox_full"))
                {
                    string fuelName = heldItem.itemID;

                    // 1. 消耗掉手里的燃料物品
                    InventoryManager.Instance.ConsumeSelectedItem();

                    // 2. 呼叫管家在当前油灯的位置生成点燃状态的油灯 (oilLight)
                    WorldStateManager.Instance.SpawnAndRecord(litLightID, transform.position);
                    Debug.Log($"你将 {fuelName} 倒入灯芯并点燃，油灯({litLightID})亮起了温暖的光芒！");

                    // 3. 报备世界管家，销毁当前熄灭状态的油灯实体
                    WorldStateManager.Instance.MarkAsDestroyed(gameObject);
                    Destroy(gameObject);
                    return true;
                }
                else
                {
                    Debug.Log("你需要手持食用油(cookingOil)或满的油箱(oilBox_full)才能为油灯注入燃料并点燃！");
                    return false;
                }

            default:
                return false;
        }
    }
}