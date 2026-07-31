using System.Collections.Generic;
using UnityEngine;

public class InteractableCar : MonoBehaviour, IInteractable
{
    [Header("组件引用")]
    [Tooltip("用于检测思考模式的触发器，抽干油后将被禁用")]
    public Collider2D triggerCollider;

    [Header("基础配置")]
    public string carName = "car";
    public string lookDescription = "一辆废弃的汽车(car)。也许可以用空油箱(oilBox)从里面抽点汽油(pump)。";

    [Header("产物掉落配置")]
    [Tooltip("装满的油箱掉落的坐标偏移量")]
    public Vector3 dropOffset = new Vector3(1.5f, -0.5f, 0f);

    // 核心状态：记录是否已经被抽过油
    private bool isPumped = false;

    public string ItemName => carName;

    void Start()
    {
        // 场景加载时，向管家查询这辆车是否已经被抽过油了
        SceneObjectID soid = GetComponent<SceneObjectID>();
        if (soid != null && WorldStateManager.Instance.GetState(soid.id) == "pumped")
        {
            ApplyPumpedState(); // 如果抽过了，直接关闭交互
        }
    }

    // 封装关闭交互的表现
    private void ApplyPumpedState()
    {
        isPumped = true;
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false; // 关闭碰撞体，雷达将彻底无视它
        }
    }

    public List<string> GetSupportedCommands()
    {
        return new List<string> { "look", "pump" };
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                Debug.Log(lookDescription);
                return true;

            case "pump":
                // 校验手里拿的是否是空油箱 (假设空油箱 ID 为 oilBox)
                if (heldItem != null && heldItem.itemID == "oilBox")
                {
                    // 1. 消耗掉手里的空油箱
                    InventoryManager.Instance.ConsumeSelectedItem();

                    // 2. 呼叫管家在地上生成一桶满油箱 (假设满油箱 ID 为 oilBox_full)
                    WorldStateManager.Instance.SpawnAndRecord("oilBox_full", playerPosition + dropOffset);
                    Debug.Log($"咕噜咕噜... 抽油成功！你获得了一桶沉甸甸的满油箱(oilBox_full)。");

                    // 3. 改变状态，使得汽车彻底隐身（无法再被雷达扫到）
                    ApplyPumpedState();

                    // 4. 状态持久化：向管家报备这辆车已经被抽干了
                    SceneObjectID soid = GetComponent<SceneObjectID>();
                    if (soid != null)
                    {
                        WorldStateManager.Instance.SaveState(soid.id, "pumped");
                    }
                    return true;
                }
                else
                {
                    Debug.Log("你需要手里拿着一个空油箱(oilBox)才能抽油！");
                    return false;
                }

            default:
                return false;
        }
    }
}