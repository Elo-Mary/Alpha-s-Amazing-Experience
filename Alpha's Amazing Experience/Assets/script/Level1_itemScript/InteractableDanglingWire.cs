using System.Collections.Generic;
using UnityEngine;

public class InteractableDanglingWire : MonoBehaviour, IInteractable
{
    [Header("基础配置")]
    public string wireName = "eleLineFromRoof";
    public string lookDescription = "一段从屋顶垂下来的电线(eleLineFromRoof)。还差一点才能连到圣诞树上，也许可以再用一段电线(electricLine)接上(tie)。";

    [Header("表现配置")]
    [Tooltip("接通后需要显示的完整连接电线 (eleLineConnectFromRoof)")]
    public GameObject connectedWireObject;

    public string ItemName => wireName;

    // 当被 GlobalStateListener 唤醒时自动执行
    void OnEnable()
    {
        // 检查管家的记录，看看自己是不是已经被接好了
        SceneObjectID soid = GetComponent<SceneObjectID>();
        if (soid != null && WorldStateManager.Instance.GetState(soid.id) == "connected")
        {
            ApplyConnectedState();
        }
    }

    // 封装连接后的状态表现
    private void ApplyConnectedState()
    {
        if (connectedWireObject != null)
        {
            connectedWireObject.SetActive(true); // 显示完整的线
        }
        gameObject.SetActive(false); // 隐藏当前这截半拉子线
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
                Debug.Log(lookDescription);
                return true;

            case "tie":
                // 校验手里拿的是否是电线
                if (heldItem != null && heldItem.itemID == "electricLine")
                {
                    // 1. 消耗掉手里的电线
                    InventoryManager.Instance.ConsumeSelectedItem();

                    // 2. 状态持久化：向管家报备这截线已经接好了
                    SceneObjectID soid = GetComponent<SceneObjectID>();
                    if (soid != null)
                    {
                        WorldStateManager.Instance.SaveState(soid.id, "connected");
                    }

                    Debug.Log("你熟练地将两段电线拧在一起，成功连接到了圣诞树上！");

                    // 3. 改变表现：显示接好的线，隐藏自己
                    ApplyConnectedState();

                    return true;
                }
                else
                {
                    Debug.Log("你需要再拿一段电线(electricLine)才能把它们接起来！");
                    return false;
                }

            default:
                return false;
        }
    }
}