using UnityEngine;

public class CoreInitializer : MonoBehaviour
{
    // 静态的单例引用
    public static CoreInitializer Instance { get; private set; }

    void Awake()
    {
        // 检查全局是否已经存在 GameCore
        if (Instance == null)
        {
            // 如果我是第一个，我就是老大，跨场景保留我
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 如果我已经存在了（说明我是从硬盘里重新加载出来的克隆体）
            // 那么立刻销毁我自己，绝不干涉那个带着玩家数据的“老前辈”
            Destroy(gameObject);
        }
    }
}