using UnityEngine;
using UnityEngine.SceneManagement;

// 强制要求挂载此脚本的物体必须带有 Collider2D
[RequireComponent(typeof(Collider2D))]
public class AutoTransitionZone : MonoBehaviour
{
    [Header("场景切换配置")]
    [Tooltip("前往的场景名称，例如 'Level1_room2'")]
    public string targetSceneName;

    [Tooltip("到达新场景后，玩家出现在的出生点物体名称")]
    public string targetSpawnPointName;

    // 当有其他带有 Rigidbody2D 的碰撞体进入此触发器时，Unity 会自动调用此方法
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 判断走进来的是不是玩家（防止被怪物、或者掉落的树枝触发传送）
        if (collision.CompareTag("Player"))
        {
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                Debug.Log($"[自动传送] 玩家触碰传送区，准备前往场景: {targetSceneName}");

                // 1. 【复用核心逻辑】把目标出生点名称交给静态信使
                SceneConfig.TargetSpawnPointName = targetSpawnPointName;

                // 2. 强制确保玩家处于行动模式 (防止玩家在边缘卡思考模式被传走)
                if (GameManager.Instance.CurrentState == GameState.Thinking)
                {
                    GameManager.Instance.ToggleThinkingMode();
                }

                // 3. 执行场景加载
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] 自动传送区没有配置目标场景名称！");
            }
        }
    }
}