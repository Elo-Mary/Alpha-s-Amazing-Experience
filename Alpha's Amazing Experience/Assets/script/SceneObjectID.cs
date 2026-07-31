using UnityEngine;
using System;

public class SceneObjectID : MonoBehaviour
{
    [Tooltip("该物体的全局唯一标识符 (预制体请务必留空！只在场景物体上生成)")]
    public string id;

    void Awake()
    {
        // 动态生成的物体，如果没有 ID 则赋予临时随机 ID
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString();
        }
    }

    void Start()
    {
        // 跨场景检查生死簿
        bool isDead = WorldStateManager.Instance.IsDestroyed(id);
        if (isDead)
        {
            Destroy(gameObject);
        }
    }

    // [ContextMenu] 可以在编辑器未运行时通过组件右键菜单触发
    [ContextMenu("生成唯一ID (Generate ID)")]
    public void GenerateID()
    {
        // #if UNITY_EDITOR 是预编译指令，确保这些编辑器特有的功能在打包发布时被自动剔除，防止报错
#if UNITY_EDITOR
        // 1. 【核心修复】：在修改变量前，告诉 Unity 录制操作。这不仅支持 Ctrl+Z 撤销，还会强制把物体标记为已修改
        UnityEditor.Undo.RecordObject(this, "Generate Unique ID");
#endif

        // 2. 写入新 ID
        id = Guid.NewGuid().ToString();
        Debug.Log($"[{gameObject.name}] 已成功生成并激活唯一ID: {id}");

#if UNITY_EDITOR
        // 3. 【核心修复】：显式将当前物体所在的场景标记为“脏数据（已修改）”，此时场景名字后面会出现 '*' 号
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }
}