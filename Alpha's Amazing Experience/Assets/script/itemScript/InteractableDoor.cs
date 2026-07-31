using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractableDoor : MonoBehaviour, IInteractable
{
    [Header("场景切换配置")]
    [Tooltip("前往的场景名称")]
    public string targetSceneName;
    [Tooltip("到达新场景后，玩家出现在的出生点名称")]
    public string targetSpawnPointName;

    public string ItemName => "door";

    public List<string> GetSupportedCommands()
    {
        return new List<string> { "enter", "look", "open" };
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                Debug.Log($"这是一扇门。输入 enter 可以进入 {targetSceneName}。");
                return true;

            case "enter":
            case "open":
                if (!string.IsNullOrEmpty(targetSceneName))
                {
                    // 【关键步骤】把目标出生点名称交给信使
                    SceneConfig.TargetSpawnPointName = targetSpawnPointName;

                    // 加载场景
                    SceneManager.LoadScene(targetSceneName);
                    return true;
                }
                return false;

            default:
                return false;
        }
    }
}