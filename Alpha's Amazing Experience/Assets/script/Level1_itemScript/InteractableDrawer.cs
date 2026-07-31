using System.Collections.Generic;
using UnityEngine;

public class InteractableDrawer : MonoBehaviour, IInteractable
{
    [Header("组件引用")]
    [Tooltip("用于检测思考模式的触发器，搜刮后将被禁用")]
    public Collider2D triggerCollider;

    [Header("生成配置")]
    [Tooltip("胶水掉落的偏移量")]
    public Vector3 dropOffset = new Vector3(1.0f, 0f, 0f);

    public string ItemName => "drawer";

    void Start()
    {
        // 场景加载时，向管家查询自身状态
        SceneObjectID soid = GetComponent<SceneObjectID>();
        if (soid != null && WorldStateManager.Instance.GetState(soid.id) == "searched")
        {
            // 如果已经被搜过，直接关闭碰撞体，玩家再也扫不到它
            if (triggerCollider != null) triggerCollider.enabled = false;
        }
    }

    public List<string> GetSupportedCommands()
    {
        // 同样支持观察和搜索
        return new List<string> { "search", "look" };
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                Debug.Log("这是一个柜子(drawer)。你可以尝试搜索(search)里面的物品。");
                return true;

            case "search":
                // 1. 生成产物：向世界管家申请生成胶水
                // 注意：必须保证传入的 ID ("glue") 与你配置的 ItemData 的 itemID 完全一致！
                WorldStateManager.Instance.SpawnAndRecord("glue", playerPosition + dropOffset);

                Debug.Log("搜索成功！你在柜子里找到了一瓶胶水(glue)。");

                // 2. 状态改变：关闭自身的交互触发器
                if (triggerCollider != null)
                {
                    triggerCollider.enabled = false;
                }

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