using System.Collections.Generic;
using UnityEngine;

public class InteractableGenerator : MonoBehaviour, IInteractable
{
    [Header("基础配置")]
    public string generatorName = "Generator";

    [Header("表现配置")]
    [Tooltip("接通后需要显示的电线实体 (eleLineConnectFromGenerator)")]
    public GameObject connectedWireObject;

    // 内部核心状态：双线独立
    private bool isStarted = false;
    private bool isTied = false;

    public string ItemName => generatorName;

    void Start()
    {
        // 场景加载 或 被底座刚刚唤醒时，向管家核对两个独立状态
        SceneObjectID soid = GetComponent<SceneObjectID>();
        if (soid != null)
        {
            // 查状态1：是否已启动
            if (WorldStateManager.Instance.GetState(soid.id + "_started") == "true")
            {
                isStarted = true;
            }

            // 查状态2：是否已接线
            if (WorldStateManager.Instance.GetState(soid.id + "_tied") == "true")
            {
                isTied = true;
                if (connectedWireObject != null) connectedWireObject.SetActive(true);
            }
        }
    }

    public List<string> GetSupportedCommands()
    {
        // 动态指令菜单：缺什么补什么
        List<string> cmds = new List<string> { "look" };

        if (!isStarted) cmds.Add("fire");
        if (!isTied) cmds.Add("tie");

        return cmds;
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                string powerStatus = isStarted ? "机器正在发出震耳欲聋的轰鸣声。" : "目前没有启动，需要木材(Wood)作为燃料(start)。";
                string wireStatus = isTied ? "已经接上了一根电线。" : "还没有连接电线，也许可以用电线(electricLine)接上(tie)。";
                Debug.Log($"这是一台木柴发电机({generatorName})。{powerStatus} {wireStatus}");
                return true;

            case "fire":
                if (!isStarted)
                {
                    // 1. 统计木材数量，并检查是否有打火机
                    int woodCount = InventoryManager.Instance.GetItemCount("Wood");
                    bool hasLighter = InventoryManager.Instance.HasItem("lighter");

                    // 2. 只有同时满足 >= 3 个木材 且 拥有打火机 时，才允许启动
                    if (woodCount >= 3 && hasLighter)
                    {
                        // 3. 执行扣除逻辑：连续调用 3 次 ConsumeItem 消耗掉 3 个木材
                        InventoryManager.Instance.ConsumeItem("Wood");
                        InventoryManager.Instance.ConsumeItem("Wood");
                        InventoryManager.Instance.ConsumeItem("Wood");

                        // 【注意】：代码里没有写 ConsumeItem("lighter")，所以打火机完美保留！

                        // 4. 改变状态并持久化记录
                        isStarted = true;
                        SceneObjectID soid = GetComponent<SceneObjectID>();
                        if (soid != null) WorldStateManager.Instance.SaveState(soid.id + "_started", "true");

                        Debug.Log("你掏出打火机(lighter)点燃了3块木材(Wood)塞进锅炉，发电机喷出一股黑烟，开始隆隆作响地运转起来！");
                        return true;
                    }
                    else
                    {
                        // 5. 精准的失败提示反馈
                        if (!hasLighter)
                        {
                            Debug.Log("万事俱备，只欠东风。你需要一个打火机(lighter)才能点燃这些木材！");
                        }
                        else if (woodCount < 3)
                        {
                            Debug.Log($"燃料不足！启动发电机需要整整 3 块木材(Wood)，而你现在只有 {woodCount} 块。去砍点桌椅或树木吧！");
                        }
                        return false;
                    }
                }
                return false;

            case "tie":
                if (!isTied)
                {
                    // 校验是否拿着电线
                    if (heldItem != null && heldItem.itemID == "electricLine")
                    {
                        InventoryManager.Instance.ConsumeSelectedItem();
                        isTied = true;

                        // 显示连接好的电线
                        if (connectedWireObject != null) connectedWireObject.SetActive(true);

                        // 【关键技巧】：给 ID 加上 _tied 后缀进行保存
                        SceneObjectID soid = GetComponent<SceneObjectID>();
                        if (soid != null) WorldStateManager.Instance.SaveState(soid.id + "_tied", "true");

                        Debug.Log("你将电线牢牢地接在了发电机的输出端上！");
                        return true;
                    }
                    else
                    {
                        Debug.Log("你需要手里拿着电线(electricLine)才能接线！");
                        return false;
                    }
                }
                return false;

            default:
                return false;
        }
    }
}