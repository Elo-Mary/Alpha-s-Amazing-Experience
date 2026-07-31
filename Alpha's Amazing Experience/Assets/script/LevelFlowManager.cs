using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class LevelFlowManager : MonoBehaviour
{
    [Header("视频配置文件 (.mp4)")]
    public VideoClip prologueEndingVideo; // 序章结束视频
    public VideoClip level1StartingVideo; // 第一关开场视频

    [Header("关卡跳转配置")]
    [Tooltip("第一关的场景名称 (例如 Level1_house_out)")]
    public string nextSceneName = "Level1_house_out";
    [Tooltip("到达第一关后，玩家出现在的出生点名称")]
    public string targetSpawnPointName = "Spawn_Level1_Start";

    void OnEnable()
    {
        InventoryManager.OnItemPickedUp += CheckLevelEndCondition;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        InventoryManager.OnItemPickedUp -= CheckLevelEndCondition;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 1. 监听每次拾取
    void CheckLevelEndCondition(string itemID)
    {
        if (itemID == "cooked rabbit")
        {
            Debug.Log(">>> 触发序章通关流程！ <<<");

            // 在进入过场动画之前，强制让游戏先干净地退出思考模式（收起UI、恢复时间）
            if (GameManager.Instance.CurrentState == GameState.Thinking)
            {
                GameManager.Instance.ToggleThinkingMode();
            }

            // A. 冻结游戏输入，进入过场状态
            GameManager.Instance.SetState(GameState.Cutscene);

            // B. 播放序章结束视频
            if (prologueEndingVideo != null)
            {
                CGManager.Instance.PlayVideo(prologueEndingVideo, () =>
                {
                    // 视频播完后的回调：清空数据并加载新场景
                    PrepareAndLoadNextLevel();
                });
            }
            else
            {
                PrepareAndLoadNextLevel();
            }
        }
    }

    // 2. 清空记忆并跨越关卡
    void PrepareAndLoadNextLevel()
    {
        InventoryManager.Instance.ClearInventory();
        WorldStateManager.Instance.ClearAllStates();

        // 【修复Bug 2】将目标出生点名称交给信使
        SceneConfig.TargetSpawnPointName = targetSpawnPointName;

        // 加载我们在面板上配置的目标场景
        SceneManager.LoadScene(nextSceneName);
    }

    // 3. 监听新场景是否加载完毕
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 【修复Bug 1】不再写死名字，而是比对面板上配置的名字
        if (scene.name == nextSceneName)
        {
            Debug.Log($">>> 进入场景 {nextSceneName}，准备播放开场动画 <<<");

            // 确保状态依然是冻结的
            GameManager.Instance.SetState(GameState.Cutscene);

            if (level1StartingVideo != null)
            {
                CGManager.Instance.PlayVideo(level1StartingVideo, () =>
                {
                    // 视频播完后，正式把控制权还给玩家！
                    GameManager.Instance.SetState(GameState.Action);
                    Debug.Log("第一关正式开始，玩家已恢复控制！");
                });
            }
            else
            {
                // 如果没有配置开场视频，直接恢复控制
                GameManager.Instance.SetState(GameState.Action);
            }
        }
    }
}