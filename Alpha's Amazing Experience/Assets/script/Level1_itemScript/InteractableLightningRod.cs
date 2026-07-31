using System.Collections.Generic;
using UnityEngine;

public class InteractableLightningRod : MonoBehaviour, IInteractable
{
    [Header("基础配置")]
    public string rodName = "lightningRod";

    [Header("本地表现 (当前场景)")]
    [Tooltip("屋顶上的电线物体 (eleline1)")]
    public GameObject localWireObject;

    public string ItemName => rodName;

    void Start()
    {
        // 场景加载时，检查自己是否已经被绑过线
        SceneObjectID soid = GetComponent<SceneObjectID>();
        if (soid != null && WorldStateManager.Instance.GetState(soid.id) == "tied")
        {
            if (localWireObject != null) localWireObject.SetActive(true);
        }
    }

    public List<string> GetSupportedCommands()
    {
        return new List<string> { "look", "tie" };
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                Debug.Log($"这是一根避雷针({rodName})。也许可以用电线(electricLine)绑(tie)在上面导电。");
                return true;

            case "tie":
                // 1. 检查状态：是否已经绑过了
                SceneObjectID soid = GetComponent<SceneObjectID>();
                if (soid != null && WorldStateManager.Instance.GetState(soid.id) == "tied")
                {
                    Debug.Log("避雷针上已经绑好电线了，不需要再绑。");
                    return true;
                }

                // 2. 检查物品：手里必须拿着电线
                if (heldItem != null && heldItem.itemID == "electricLine")
                {
                    // 消耗电线
                    InventoryManager.Instance.ConsumeSelectedItem();

                    // 显示当前场景的电线 (eleline1)
                    if (localWireObject != null) localWireObject.SetActive(true);

                    // 记录本地状态：避雷针已绑线
                    if (soid != null) WorldStateManager.Instance.SaveState(soid.id, "tied");

                    // 【最关键的一步】：向世界管家写入一个纯字符串的“全局暗号”
                    // 这个字符串不依赖任何物体 ID，哪怕切了场景，管家也记得它！
                    WorldStateManager.Instance.SaveState("Global_RoofWireTied", "true");

                    Debug.Log("你将电线牢牢地绑在了避雷针上，电线顺着屋檐垂了下去...");
                    return true;
                }
                else
                {
                    Debug.Log("你需要拿着一捆电线(electricLine)才能进行绑线操作！");
                    return false;
                }

            default:
                return false;
        }
    }
}