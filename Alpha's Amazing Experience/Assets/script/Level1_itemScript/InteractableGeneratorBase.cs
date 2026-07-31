using System.Collections.Generic;
using UnityEngine;

public class InteractableGeneratorBase : MonoBehaviour, IInteractable
{
    [Header("基础配置")]
    [Tooltip("雷达显示的名称，用来让玩家定位放置点")]
    public string baseName = "generatorBase";

    [Header("表现配置")]
    [Tooltip("提前放在场景中、默认隐藏的发电机实体")]
    public GameObject placedGeneratorEntity;

    // 状态记录
    private bool isPlaced = false;

    public string ItemName => baseName;

    void Start()
    {
        SceneObjectID soid = GetComponent<SceneObjectID>();
        if (soid != null && WorldStateManager.Instance.GetState(soid.id) == "placed")
        {
            ApplyPlacedState();
        }
        else
        {
            ApplyEmptyState();
        }
    }

    private void ApplyPlacedState()
    {
        isPlaced = true;
        // 激活隐藏的真实发电机
        if (placedGeneratorEntity != null) placedGeneratorEntity.SetActive(true);
        // 关闭底座自身的碰撞体，让雷达以后只扫真实发电机，无视这个底座
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    private void ApplyEmptyState()
    {
        isPlaced = false;
        if (placedGeneratorEntity != null) placedGeneratorEntity.SetActive(false);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }

    public List<string> GetSupportedCommands()
    {
        if (!isPlaced)
        {
            return new List<string> { "look", "set" };
        }
        // 放好之后底座就“功成身退”了，后续指令全交给真实发电机处理
        return new List<string>();
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                if (!isPlaced)
                    Debug.Log($"地上有一块平整的区域({baseName})，似乎刚好可以用来安置(set)发电机(generator)。");
                return true;

            case "set":
                if (!isPlaced)
                {
                    if (heldItem != null && heldItem.itemID == "generator")
                    {
                        // 1. 消耗背包里的发电机
                        InventoryManager.Instance.ConsumeSelectedItem();

                        // 2. 状态持久化
                        SceneObjectID soid = GetComponent<SceneObjectID>();
                        if (soid != null) WorldStateManager.Instance.SaveState(soid.id, "placed");

                        // 3. 唤醒真实的隐藏发电机，底座退场
                        ApplyPlacedState();

                        Debug.Log("你将发电机(generator)稳稳地安置在了底座上！");
                        return true;
                    }
                    else
                    {
                        Debug.Log("你需要手里拿着发电机(generator)才能放置！");
                        return false;
                    }
                }
                return false;

            default:
                return false;
        }
    }
}