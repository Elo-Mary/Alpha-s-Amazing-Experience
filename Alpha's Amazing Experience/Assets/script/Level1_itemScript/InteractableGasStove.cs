using System.Collections.Generic;
using UnityEngine;

public class InteractableGasStove : MonoBehaviour, IInteractable
{
    [Header("表现配置")]
    [Tooltip("用于切换火焰贴图的渲染器")]
    public SpriteRenderer targetSpriteRenderer;
    [Tooltip("煤气灶打开并点燃火焰的贴图")]
    public Sprite fireOnSprite;

    [Header("产物掉落配置")]
    [Tooltip("转化后产物掉落的偏移量")]
    public Vector3 dropOffset = new Vector3(1.0f, -0.5f, 0f);

    // 核心状态：是否已点燃（单向不可逆）
    private bool isOn = false;

    // 伪装成柜子
    public string ItemName => "drawer4";

    void Start()
    {
        // 场景加载时，向管家查询自身是否已经被点燃过
        SceneObjectID soid = GetComponent<SceneObjectID>();
        if (soid != null && WorldStateManager.Instance.GetState(soid.id) == "on")
        {
            ApplyFireState(); // 直接恢复燃烧状态
        }
    }

    // 封装状态改变方法：替换为火焰贴图并修改状态变量
    private void ApplyFireState()
    {
        isOn = true;
        if (targetSpriteRenderer != null && fireOnSprite != null)
        {
            targetSpriteRenderer.sprite = fireOnSprite;
        }
    }

    public List<string> GetSupportedCommands()
    {
        // 【动态指令菜单】：根据当前状态，雷达扫出的可用指令完全不同！
        if (!isOn)
        {
            // 未打开时，只能看和开
            return new List<string> { "look", "switch", "set" };
        }
        else
        {
            // 打开后，不再响应开关，而是像篝火一样支持烹饪/转化
            return new List<string> { "look", "cook" };
        }
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                if (isOn)
                {
                    Debug.Log("这是一个正在熊熊燃烧的煤气灶(drawer4)。你可以像使用篝火一样用它烹饪(cook)物品。");
                }
                else
                {
                    Debug.Log("这是一个煤气灶(drawer4)，目前火是关着的。你可以尝试用 switch 或 set 打开它。");
                }
                return true;

            case "switch":
            case "set":
                if (!isOn)
                {
                    // 1. 改变表现层状态
                    ApplyFireState();
                    Debug.Log("你拧动开关，煤气灶(drawer4)亮起了蓝色的火焰！");

                    // 2. 状态持久化：向管家报备它已被永久点燃
                    SceneObjectID soid = GetComponent<SceneObjectID>();
                    if (soid != null)
                    {
                        WorldStateManager.Instance.SaveState(soid.id, "on");
                    }
                    return true;
                }
                return false;

            case "cook":
                if (isOn)
                {
                    // 照搬篝火的烹饪转化逻辑
                    if (heldItem != null && heldItem.itemID == "raw_rabbit")
                    {
                        InventoryManager.Instance.ConsumeSelectedItem();
                        WorldStateManager.Instance.SpawnAndRecord("cooked_rabbit", playerPosition + dropOffset);
                        Debug.Log("转化成功！你在煤气灶上把食材变成了熟兔肉(cooked_rabbit)。");
                        return true;
                    }
                    else
                    {
                        Debug.Log("你需要拿着合适的生食或材料，才能在火上转化！");
                        return false;
                    }
                }
                return false;

            default:
                return false;
        }
    }
}