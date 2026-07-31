using System.Collections.Generic;
using UnityEngine;

public class InteractableClothes : MonoBehaviour, IInteractable
{
    [Header("组件引用")]
    [Tooltip("用于切换贴图的渲染器（可选）")]
    public SpriteRenderer targetSpriteRenderer;
    [Tooltip("用于检测思考模式的触发器")]
    public Collider2D triggerCollider;

    [Header("表现设置")]
    [Tooltip("被翻找后的衣服贴图（比如变得凌乱）")]
    public Sprite searchedSprite;

    [Tooltip("钥匙掉落的偏移量")]
    public Vector3 dropOffset = new Vector3(1.0f, -0.5f, 0f);

    public string ItemName => "clothes";

    // 场景加载时，向管家查询自身状态
    void Start()
    {
        SceneObjectID soid = GetComponent<SceneObjectID>();
        if (soid != null && WorldStateManager.Instance.GetState(soid.id) == "searched")
        {
            ApplySearchedState(); // 如果已经被搜过，直接进入凌乱状态并关闭交互
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
                Debug.Log("这是一件挂着的衣服(clothes)。你可以尝试搜索(search)它的口袋。");
                return true;

            case "search":
                // 1. 生成产物：向世界管家申请生成钥匙，它会自动发身份证并登记在白名单
                // 注意：传入的 "key" 必须与你在 KeyData 里的 itemID 完全一致！
                WorldStateManager.Instance.SpawnAndRecord("key", playerPosition + dropOffset);

                Debug.Log("搜索成功！你在衣服口袋里找到了一把钥匙(key)。");

                // 2. 状态改变：变为凌乱/已搜索状态
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