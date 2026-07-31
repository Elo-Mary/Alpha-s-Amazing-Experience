using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractableLockedDoor : MonoBehaviour, IInteractable
{
    [Header("基础配置")]
    [Tooltip("雷达显示的名称，如果有多个门可以用 door_wood 等区分")]
    public string doorName = "door";

    [Header("表现配置")]
    [Tooltip("用于切换开关门贴图的渲染器")]
    public SpriteRenderer targetSpriteRenderer;
    [Tooltip("门被打开后的贴图")]
    public Sprite doorOpenSprite;

    [Header("解锁配置")]
    [Tooltip("开门所需钥匙的 Item ID (例如 key 或 l1_key)")]
    public string requiredKeyID = "key";

    [Header("场景切换配置")]
    public string targetSceneName;
    public string targetSpawnPointName;

    // 核心状态：门是否已开
    private bool isOpen = false;

    public string ItemName => doorName;

    void Start()
    {
        // 场景加载时，查询管家这扇门是不是已经被打开过了
        SceneObjectID soid = GetComponent<SceneObjectID>();
        if (soid != null && WorldStateManager.Instance.GetState(soid.id) == "opened")
        {
            ApplyOpenState();
        }
    }

    // 封装开门表现
    private void ApplyOpenState()
    {
        isOpen = true;
        if (targetSpriteRenderer != null && doorOpenSprite != null)
        {
            targetSpriteRenderer.sprite = doorOpenSprite;
        }
    }

    public List<string> GetSupportedCommands()
    {
        // 【动态指令】：没开的时候能看、能开、能踹；开了之后只能看和进
        if (!isOpen)
        {
            return new List<string> { "look", "switch", "kick" };
        }
        else
        {
            return new List<string> { "look", "enter" };
        }
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                if (isOpen)
                    Debug.Log($"这扇门({doorName})已经敞开了。输入 enter 可以进去。");
                else
                    Debug.Log($"这扇门({doorName})紧紧关闭着，似乎上了锁。你可以尝试用钥匙打开(open)，或者直接踹开(kick)。");
                return true;

            case "switch":
                if (!isOpen)
                {
                    // 检查手里拿的是不是对的钥匙
                    if (heldItem != null && heldItem.itemID == requiredKeyID)
                    {
                        // 消耗钥匙
                        InventoryManager.Instance.ConsumeSelectedItem();

                        // 开门表现与记录
                        ApplyOpenState();
                        RecordOpenedState();

                        Debug.Log("伴随着清脆的咔哒声，你用钥匙打开了门！");
                        return true;
                    }
                    else
                    {
                        Debug.Log("你需要拿着正确的钥匙才能打开这扇门！");
                        return false;
                    }
                }
                return false;

            case "kick":
                if (!isOpen)
                {
                    // 暴力破门，不需要钥匙
                    ApplyOpenState();
                    RecordOpenedState();

                    Debug.Log("砰的一声巨响！你简单粗暴地一脚踹开了这扇门。");
                    return true;
                }
                return false;

            case "enter":
                if (isOpen)
                {
                    if (!string.IsNullOrEmpty(targetSceneName))
                    {
                        Debug.Log("你走进了门内...");
                        // 交接坐标并跨场景
                        SceneConfig.TargetSpawnPointName = targetSpawnPointName;
                        SceneManager.LoadScene(targetSceneName);
                        return true;
                    }
                }
                else
                {
                    Debug.Log("门还关着，你进不去！");
                }
                return false;

            default:
                return false;
        }
    }

    // 辅助方法：向世界管家报备状态
    private void RecordOpenedState()
    {
        SceneObjectID soid = GetComponent<SceneObjectID>();
        if (soid != null)
        {
            WorldStateManager.Instance.SaveState(soid.id, "opened");
        }
    }
}