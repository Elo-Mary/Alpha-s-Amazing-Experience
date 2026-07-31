using System.Collections.Generic;
using UnityEngine;

public class Bush : MonoBehaviour, IInteractable
{
    [Header("组件引用 (由外部空物体配置)")]
    [Tooltip("用于切换贴图的渲染器")]
    public SpriteRenderer targetSpriteRenderer;
    [Tooltip("用于检测思考模式的触发器")]
    public Collider2D triggerCollider;

    [Header("表现与生成设置")]
    [Tooltip("被砍伐后的残骸贴图")]
    public Sprite cutSprite;
    [Tooltip("砍伐后掉落的树枝预制体")]
    public GameObject branchPrefab;
    [Tooltip("基于玩家位置的掉落偏移量")]
    public Vector3 dropOffset = new Vector3(1.5f, -0.5f, 0f);

    // 物品标识名称
    public string ItemName => "bush";

    public List<string> GetSupportedCommands()
    {
        return new List<string> { "cut", "look" };
    }

    // ... 前面的变量定义全部保持不变 ...

    // 【新增】在加载场景时，主动向管家询问自己的状态
    void Start()
    {
        SceneObjectID soid = GetComponent<SceneObjectID>();
        if (soid != null && WorldStateManager.Instance.GetState(soid.id) == "cut")
        {
            ApplyCutState(); // 如果管家说我被砍过，直接变成残骸
        }
    }

    // 封装一个变成残骸的方法，方便复用
    void ApplyCutState()
    {
        if (targetSpriteRenderer != null && cutSprite != null) targetSpriteRenderer.sprite = cutSprite;
        if (triggerCollider != null) triggerCollider.enabled = false;
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                return true;

            case "cut":
                if (heldItem != null && heldItem.itemID == "axe")
                {
                    // 【核心修改 1】让管家来生成树枝，而不是自己 Instantiate！
                    WorldStateManager.Instance.SpawnAndRecord("branch", playerPosition + dropOffset);

                    // 表现层变更为残骸
                    ApplyCutState();

                    // 【核心修改 2】向管家登记自己的重伤状态
                    SceneObjectID soid = GetComponent<SceneObjectID>();
                    if (soid != null)
                    {
                        WorldStateManager.Instance.SaveState(soid.id, "cut");
                    }
                    return true;
                }
                return false;

            default:
                return false;
        }
    }
}