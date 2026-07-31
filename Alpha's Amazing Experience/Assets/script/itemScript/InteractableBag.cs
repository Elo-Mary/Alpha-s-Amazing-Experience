using System.Collections.Generic;
using UnityEngine;

public class InteractableBag : MonoBehaviour, IInteractable
{
    [Header("组件引用 (由外部空物体配置)")]
    [Tooltip("用于切换贴图的渲染器")]
    public SpriteRenderer targetSpriteRenderer;
    [Tooltip("用于检测思考模式的触发器")]
    public Collider2D triggerCollider;

    [Header("表现设置 (可选)")]
    [Tooltip("被翻开后的背包贴图（比如拉链拉开的样子）")]
    public Sprite searchedSprite;

    [Header("掉落物位置偏移")]
    // 为了防止三件物品掉在同一个坐标重叠在一起，我们给它们设置不同的偏移量
    public Vector3 knifeOffset = new Vector3(-1.0f, -0.5f, 0f);
    public Vector3 lighterOffset = new Vector3(0.0f, -1.0f, 0f);
    public Vector3 axeOffset = new Vector3(1.0f, -0.5f, 0f);

    public string ItemName => "bag";

    // 加载场景时，主动向管家询问自己的状态
    void Start()
    {
        SceneObjectID soid = GetComponent<SceneObjectID>();
        if (soid != null && WorldStateManager.Instance.GetState(soid.id) == "searched")
        {
            ApplySearchedState(); // 如果管家说我被搜过了，直接变成拉开的状态
        }
    }

    // 封装状态改变方法：替换贴图并关闭检测
    void ApplySearchedState()
    {
        if (targetSpriteRenderer != null && searchedSprite != null)
        {
            targetSpriteRenderer.sprite = searchedSprite;
        }
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false; // 关闭碰撞体，雷达将再也扫不到它
        }
    }

    public List<string> GetSupportedCommands()
    {
        return new List<string> { "search", "look" };
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                Debug.Log("这是一个鼓鼓囊囊的旅行背包(bag)。你可以尝试搜索(search)它。");
                return true;

            case "search":
                // 1. 连续生成三件产物：调用全局管家凭空生成，并自动记录进白名单
                // 注意：传入的 ID 必须与你配置的 ItemData 的 itemID 完全一致！
                WorldStateManager.Instance.SpawnAndRecord("knife", playerPosition + knifeOffset);
                WorldStateManager.Instance.SpawnAndRecord("lighter", playerPosition + lighterOffset);
                WorldStateManager.Instance.SpawnAndRecord("axe", playerPosition + axeOffset);

                Debug.Log("搜索成功！你从背包里找出了小刀(knife)、打火机(lighter)和斧头(axe)。");

                // 2. 状态改变：变为已搜索状态
                ApplySearchedState();

                // 3. 记录持久化状态：向管家报备自己已经被搜过了
                SceneObjectID soid = GetComponent<SceneObjectID>();
                if (soid != null)
                {
                    WorldStateManager.Instance.SaveState(soid.id, "searched");
                }
                return true;

            default:
                return false;
        }
    }
}